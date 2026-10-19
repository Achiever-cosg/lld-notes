# Multi-Location Car Rental System - Interview Guide

## 1. Requirement Clarification (5-10 min)

**Interviewer**: "Design a car rental system."

**Your Questions**:
- "Can customers browse available cars and make reservations online?"
  - Yes, customers should search and reserve cars for specific dates.

- "What car details do we need to track?"
  - Make, model, year, license plate, and rental price per day.

- "How should customers search for cars?"
  - By make, model, price range, and date availability.

- **"Does the system support multiple store locations?"** ⭐
  - **Yes! Support multiple rental locations/branches.**

- **"Can customers pick up from one location and return to another?"** ⭐
  - **Yes, support one-way rentals with additional fees.**

- **"How do we search for cars - by specific store or by city?"** ⭐
  - **Both! Search by store ID or by city name.**

- "What about reservations? Can they modify or cancel?"
  - Yes, support create, modify, and cancel reservations.

- "How do we handle payments?"
  - Support multiple payment methods (credit card, PayPal, etc.).

- "What about concurrent bookings? Two users booking the same car?"
  - Must handle concurrency and ensure data consistency.

- "Do we track customer information?"
  - Yes, name, contact info, and driver's license number.

---

## 2. Identify Core Objects (5 min)

**Think aloud**: "Let me identify the main entities..."

### Core Entities
1. **Store** - represents rental locations/branches ⭐
2. **Car** - represents vehicles in inventory (now with location tracking) ⭐
3. **Customer** - people renting cars
4. **Reservation** - booking made by customer (now with pickup/return stores) ⭐
5. **RentalSystem** - manages the entire system
6. **PaymentProcessor** - handles payments (interface for flexibility)

### Key Relationships
- Store **contains** multiple Cars
- Car **is located at** Store (current location) ⭐
- Customer **makes** Reservation
- Reservation **contains** Car, PickupStore, ReturnStore ⭐
- RentalSystem **manages** Stores, Cars, and Reservations ⭐
- RentalSystem **uses** PaymentProcessor

---

## 3. Design Discussion (10-15 min)

### A. Store Class Design ⭐ NEW
**Your thought**: "Each rental branch needs identification and location details."

```
Properties:
- StoreId (unique identifier)
- Name (e.g., "Downtown Branch")
- Address, City
- ContactInfo

Why?
- StoreId for fast lookups
- City allows location-based searches
- Simple data holder - no complex behavior
```

### B. Car Class Design (Updated) ⭐
**Your thought**: "Cars are entities with properties, availability, AND location."

```
Properties:
- Make, Model, Year, LicensePlate (immutable)
- RentalPricePerDay
- Available (mutable - changes with bookings)
- CurrentStore (mutable - changes as car moves) ⭐ NEW

Why?
- License plate is unique identifier
- Availability flag helps quick filtering
- CurrentStore tracks which location has the car
- SetCurrentStore() updates location after returns
```

### C. Customer Class Design
**Your thought**: "Simple data holder for customer info."

```
Properties:
- Name
- ContactInfo
- DriversLicenseNumber

Why?
- Read-only properties (set via constructor)
- Lightweight - no complex behavior needed
```

### D. Reservation Class Design (Updated) ⭐
**Your thought**: "Reservation connects customer, car, dates, AND locations."

```
Properties:
- ReservationId (unique identifier)
- Customer, Car (relationships)
- StartDate, EndDate
- PickupStore (where customer gets car) ⭐ NEW
- ReturnStore (where customer returns car) ⭐ NEW
- TotalPrice (calculated on creation, includes one-way fee) ⭐ UPDATED

Key Design Choices:
- Calculate total price in constructor
- Base Price = (Days × RentalPricePerDay)
- Days = (EndDate - StartDate) + 1 (inclusive)
- If PickupStore ≠ ReturnStore: add $50 one-way fee ⭐ NEW
```

### E. Payment Processing
**Interviewer**: "How would you handle different payment methods?"

**Your answer**: "I'll use Strategy pattern with an interface."

```
IPaymentProcessor (Interface)
├── CreditCardPaymentProcessor
└── PayPalPaymentProcessor

Why?
- Easy to add new payment methods
- Decouples payment logic from RentalSystem
- Testable (can mock payment processors)
```

### F. RentalSystem Design (Critical!) ⭐ UPDATED
**Your thought**: "This is the facade coordinating everything across multiple stores."

```
Design Decisions:

1. Singleton Pattern
   Why? Single point of access, shared state

2. Data Storage ⭐ UPDATED
   - Dictionary<StoreId, Store> ⭐ NEW
   - Dictionary<LicensePlate, Car>
   - Dictionary<ReservationId, Reservation>
   Why? Fast lookups O(1) for all entities

3. Core Operations ⭐ UPDATED
   - AddStore/RemoveStore (location management) ⭐ NEW
   - AddCar/RemoveCar (inventory management)
   - SearchCars(make, model, storeId, dates) ⭐ UPDATED
   - SearchCarsByCity(make, model, city, dates) ⭐ NEW
   - MakeReservation(customer, car, dates, pickupStore, returnStore) ⭐ UPDATED
   - CompleteReservation(id) - moves car to return store ⭐ NEW
   - CancelReservation (free up car)
   - ProcessPayment (delegate to processor)
   - GetCarsAtStore(storeId) ⭐ NEW
```

### G. Location Availability Logic ⭐ NEW - IMPORTANT!
**Interviewer**: "How do you ensure a car is at the right location for pickup?"

**Your answer**: "IsCarAvailableAtStore() checks three things:"

```
1. Date Availability
   - No overlapping reservations (same as before)

2. Location Tracking
   - Where is the car NOW?
   - Where will it be AFTER previous reservations?

3. Logic:
   - Find the last reservation ending before pickup date
   - If exists: car will be at that reservation's ReturnStore
   - If none: car is at its CurrentStore
   - Check if that location matches requested PickupStore

Example:
Car ABC123 currently at Store A
- Reservation 1: 1/1-1/5, Return to Store B
- New request: 1/10 pickup at Store B ✅ (car will be there)
- New request: 1/10 pickup at Store A ❌ (car at Store B)
```

### H. Concurrency Handling
**Interviewer**: "How do you handle concurrent bookings?"

**Your answer**: 
```
Approach 1 (Current): 
- Use dictionaries with careful checks
- IsCarAvailable() checks date overlaps
- IsCarAvailableAtStore() checks location logic ⭐ UPDATED

Approach 2 (Better for Production):
- Use ConcurrentDictionary in C#
- Add locking around reservation creation
- Transaction-like behavior:
  1. Check availability (date + location) ⭐ UPDATED
  2. Lock car
  3. Create reservation
  4. Mark car unavailable
  5. Release lock

Trade-offs:
- Simple approach: Race conditions possible
- Locking approach: Better consistency, slight performance hit
```

---

## 4. Implementation (15-20 min)

### Step 1: Store Class ⭐ NEW

**Interviewer**: "Let's start coding."

```csharp
// Store - Rental location
public class Store
{
    public string StoreId { get; }
    public string Name { get; }
    public string Address { get; }
    public string City { get; }
    public string ContactInfo { get; }

    public Store(string storeId, string name, string address, 
                 string city, string contactInfo)
    {
        StoreId = storeId;
        Name = name;
        Address = address;
        City = city;
        ContactInfo = contactInfo;
    }
}
```

**Talk through it**: "Simple data holder for store information."

---

### Step 2: Car Class (Updated) ⭐

```csharp
// Car - Now tracks current location
public class Car
{
    public string Make { get; }
    public string Model { get; }
    public int Year { get; }
    public string LicensePlate { get; }
    public double RentalPricePerDay { get; }
    public bool Available { get; private set; }
    public Store CurrentStore { get; private set; } // ⭐ NEW

    public Car(string make, string model, int year, 
               string licensePlate, double rentalPricePerDay, 
               Store store) // ⭐ UPDATED
    {
        Make = make;
        Model = model;
        Year = year;
        LicensePlate = licensePlate;
        RentalPricePerDay = rentalPricePerDay;
        Available = true;
        CurrentStore = store; // ⭐ NEW
    }

    public void SetAvailable(bool available)
    {
        Available = available;
    }

    public void SetCurrentStore(Store store) // ⭐ NEW
    {
        CurrentStore = store;
    }
}
```

**Talk through it**: "Cars now know their location. SetCurrentStore updates location after returns."

---

### Step 3: Customer and Reservation (Updated) ⭐

```csharp
// Customer - Data holder (unchanged)
public class Customer
{
    public string Name { get; }
    public string ContactInfo { get; }
    public string DriversLicenseNumber { get; }

    public Customer(string name, string contactInfo, 
                    string driversLicenseNumber)
    {
        Name = name;
        ContactInfo = contactInfo;
        DriversLicenseNumber = driversLicenseNumber;
    }
}

// Reservation - Now includes pickup/return stores ⭐
public class Reservation
{
    public string ReservationId { get; }
    public Customer Customer { get; }
    public Car Car { get; }
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
    public Store PickupStore { get; } // ⭐ NEW
    public Store ReturnStore { get; } // ⭐ NEW
    public double TotalPrice { get; }

    public Reservation(string reservationId, Customer customer, 
                       Car car, DateTime startDate, DateTime endDate,
                       Store pickupStore, Store returnStore) // ⭐ UPDATED
    {
        ReservationId = reservationId;
        Customer = customer;
        Car = car;
        StartDate = startDate;
        EndDate = endDate;
        PickupStore = pickupStore; // ⭐ NEW
        ReturnStore = returnStore; // ⭐ NEW
        TotalPrice = CalculateTotalPrice();
    }

    private double CalculateTotalPrice()
    {
        int daysRented = (EndDate - StartDate).Days + 1;
        double basePrice = Car.RentalPricePerDay * daysRented;
        
        // Add one-way fee if stores differ ⭐ NEW
        if (PickupStore.StoreId != ReturnStore.StoreId)
        {
            basePrice += 50.0; // One-way rental fee
        }
        
        return basePrice;
    }
}
```

**Talk through it**: "Reservation now tracks pickup/return stores. One-way rentals cost extra $50."

---

### Step 4: Payment Strategy (Unchanged)

```csharp
// Interface for payment processing
public interface IPaymentProcessor
{
    bool ProcessPayment(double amount);
}

// Concrete implementations
public class CreditCardPaymentProcessor : IPaymentProcessor
{
    public bool ProcessPayment(double amount)
    {
        // Integrate with payment gateway
        Console.WriteLine($"Processing ${amount} via Credit Card");
        return true;
    }
}

public class PayPalPaymentProcessor : IPaymentProcessor
{
    public bool ProcessPayment(double amount)
    {
        Console.WriteLine($"Processing ${amount} via PayPal");
        return true;
    }
}
```

**Talk through it**: "Strategy pattern makes it easy to add payment methods without changing RentalSystem."

---

### Step 5: The Core - RentalSystem (Updated) ⭐

```csharp
public class RentalSystem
{
    private static RentalSystem instance;
    private readonly Dictionary<string, Store> stores; // ⭐ NEW
    private readonly Dictionary<string, Car> cars;
    private readonly Dictionary<string, Reservation> reservations;
    private readonly IPaymentProcessor paymentProcessor;

    private RentalSystem()
    {
        stores = new Dictionary<string, Store>(); // ⭐ NEW
        cars = new Dictionary<string, Car>();
        reservations = new Dictionary<string, Reservation>();
        paymentProcessor = new CreditCardPaymentProcessor();
    }

    // Singleton access (unchanged)
    public static RentalSystem Instance { get; }

    // Store management ⭐ NEW
    public void AddStore(Store store)
    {
        stores[store.StoreId] = store;
    }

    public void RemoveStore(string storeId)
    {
        stores.Remove(storeId);
    }

    // Car management (unchanged)
    public void AddCar(Car car) { cars[car.LicensePlate] = car; }
    public void RemoveCar(string licensePlate) { cars.Remove(licensePlate); }

    // Search by specific store ⭐ UPDATED
    public List<Car> SearchCars(string make, string model, string storeId,
                                DateTime startDate, DateTime endDate)
    {
        return cars.Values
            .Where(car => car.Make.Equals(make, StringComparison.OrdinalIgnoreCase)
                       && car.Model.Equals(model, StringComparison.OrdinalIgnoreCase)
                       && car.CurrentStore.StoreId == storeId // ⭐ NEW
                       && car.Available 
                       && IsCarAvailableAtStore(car, storeId, startDate, endDate)) // ⭐ UPDATED
            .ToList();
    }

    // Search by city ⭐ NEW
    public List<Car> SearchCarsByCity(string make, string model, string city,
                                      DateTime startDate, DateTime endDate)
    {
        return cars.Values
            .Where(car => car.Make.Equals(make, StringComparison.OrdinalIgnoreCase)
                       && car.Model.Equals(model, StringComparison.OrdinalIgnoreCase)
                       && car.CurrentStore.City.Equals(city, StringComparison.OrdinalIgnoreCase)
                       && car.Available 
                       && IsCarAvailable(car, startDate, endDate))
            .ToList();
    }

    // Check date overlaps (unchanged)
    private bool IsCarAvailable(Car car, DateTime startDate, DateTime endDate)
    {
        return reservations.Values
            .Where(reservation => reservation.Car == car)
            .All(reservation => endDate < reservation.StartDate 
                             || startDate > reservation.EndDate);
    }

    // Check if car will be at requested store ⭐ NEW - CRITICAL!
    private bool IsCarAvailableAtStore(Car car, string storeId, 
                                       DateTime startDate, DateTime endDate)
    {
        // Check date availability first
        if (!IsCarAvailable(car, startDate, endDate))
            return false;

        // Find last reservation before pickup date
        var lastReservation = reservations.Values
            .Where(res => res.Car == car && res.EndDate < startDate)
            .OrderByDescending(res => res.EndDate)
            .FirstOrDefault();

        // Car will be at return store of last reservation, or current store
        string carLocationOnPickup = lastReservation?.ReturnStore.StoreId 
                                   ?? car.CurrentStore.StoreId;

        return carLocationOnPickup == storeId;
    }

    // Make reservation with stores ⭐ UPDATED
    public Reservation MakeReservation(Customer customer, Car car, 
                                       DateTime startDate, DateTime endDate,
                                       Store pickupStore, Store returnStore)
    {
        if (IsCarAvailableAtStore(car, pickupStore.StoreId, startDate, endDate))
        {
            var reservationId = GenerateReservationId();
            var reservation = new Reservation(reservationId, customer, car, 
                                             startDate, endDate, 
                                             pickupStore, returnStore);
            reservations[reservationId] = reservation;
            car.SetAvailable(false);
            return reservation;
        }
        return null;
    }

    // Complete reservation - moves car ⭐ NEW
    public void CompleteReservation(string reservationId)
    {
        if (reservations.TryGetValue(reservationId, out var reservation))
        {
            reservation.Car.SetCurrentStore(reservation.ReturnStore);
            reservation.Car.SetAvailable(true);
        }
    }

    // Cancel reservation (updated for stores)
    public void CancelReservation(string reservationId)
    {
        if (reservations.TryGetValue(reservationId, out var reservation))
        {
            reservations.Remove(reservationId);
            reservation.Car.SetAvailable(true);
        }
    }

    // Payment (unchanged)
    public bool ProcessPayment(Reservation reservation)
    {
        return paymentProcessor.ProcessPayment(reservation.TotalPrice);
    }

    // Get cars at specific store ⭐ NEW
    public List<Car> GetCarsAtStore(string storeId)
    {
        return cars.Values
            .Where(car => car.CurrentStore.StoreId == storeId)
            .ToList();
    }

    private string GenerateReservationId()
    {
        return "RES" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
    }
}
```

**Key points to mention**:
- Added stores dictionary for location management ⭐
- SearchCars now filters by store location ⭐
- SearchCarsByCity enables city-wide search ⭐
- IsCarAvailableAtStore checks BOTH dates AND location ⭐
- CompleteReservation moves car to return store ⭐
- MakeReservation requires pickup/return stores ⭐

---

## 5. Follow-up Questions & Answers

### Q: "How do you prevent booking a car that's at a different location?" ⭐
**A**: "IsCarAvailableAtStore tracks car movement:
```csharp
// Find where car will be on pickup date
var lastReservation = reservations
    .Where(res => res.Car == car && res.EndDate < startDate)
    .OrderByDescending(res => res.EndDate)
    .FirstOrDefault();

// Car is at last return store, or current store if no prior reservations
string carLocation = lastReservation?.ReturnStore.StoreId 
                   ?? car.CurrentStore.StoreId;

return carLocation == requestedPickupStoreId;
```
This prevents impossible bookings across locations."

---

### Q: "What if a customer doesn't return the car to the right store?"
**A**: "CompleteReservation handles this:
```csharp
public void CompleteReservation(string reservationId)
{
    var reservation = reservations[reservationId];
    // Move car to return store (not current location)
    reservation.Car.SetCurrentStore(reservation.ReturnStore);
    reservation.Car.SetAvailable(true);
}
```
Could add ActualReturnStore parameter for exceptions and charge relocation fees."

---

### Q: "How would you handle inter-store transfers?"
**A**: "Add a Transfer operation:
```csharp
public void TransferCar(string licensePlate, Store toStore)
{
    var car = cars[licensePlate];
    if (car.Available) // Only transfer available cars
    {
        car.SetCurrentStore(toStore);
        // Optional: Log transfer for auditing
    }
}
```
This lets operations rebalance inventory."

---

### Q: "How would you improve concurrency?"
**A**: "Use ConcurrentDictionary and add locking:
```csharp
private readonly object reservationLock = new object();

public Reservation MakeReservation(...)
{
    lock (reservationLock)
    {
        if (IsCarAvailable(car, startDate, endDate))
        {
            // Create reservation
        }
    }
}
```
This prevents race conditions between availability check and reservation creation."

---

### Q: "How would you add car categories (SUV, Sedan, etc.)?"
**A**: "Add an enum and property:
```csharp
public enum CarType { Sedan, SUV, Luxury, Economy }

public class Car
{
    public CarType Type { get; }
    // ... other properties
}

// Update search
public List<Car> SearchCars(CarType type, DateTime start, DateTime end)
{
    return cars.Values.Where(car => car.Type == type && ...).ToList();
}
```

---

### Q: "How would you handle pricing variations (weekend rates, seasonal pricing)?"
**A**: "Use Strategy pattern for pricing:
```csharp
public interface IPricingStrategy
{
    double CalculatePrice(Car car, DateTime start, DateTime end);
}

public class StandardPricing : IPricingStrategy { ... }
public class WeekendPricing : IPricingStrategy { ... }
public class SeasonalPricing : IPricingStrategy { ... }
```
Inject into Reservation or RentalSystem."

---

### Q: "What about late returns and penalties?"
**A**: "Extend Reservation:
```csharp
public class Reservation
{
    public DateTime? ActualReturnDate { get; private set; }
    
    public void CompleteReturn(DateTime returnDate)
    {
        ActualReturnDate = returnDate;
    }
    
    public double CalculatePenalty()
    {
        if (ActualReturnDate > EndDate)
        {
            int lateDays = (ActualReturnDate.Value - EndDate).Days;
            return lateDays * Car.RentalPricePerDay * 1.5; // 50% penalty
        }
        return 0;
    }
}
```

---

### Q: "How would you scale this system?"
**A**: 
- **Database**: Replace in-memory dictionaries with database (SQL/NoSQL)
- **Caching**: Add Redis for frequently accessed cars
- **Microservices**: Split into services (Inventory, Reservation, Payment)
- **Message Queue**: Use RabbitMQ/Kafka for async operations
- **Load Balancing**: Distribute requests across servers

---

## 6. Design Patterns Used

1. **Singleton**: RentalSystem (single instance)
2. **Strategy**: Payment processing (IPaymentProcessor)
3. **Facade**: RentalSystem (simplifies complex operations)

---

## 7. Time Complexity Analysis ⭐ UPDATED

| Operation | Complexity | Reason |
|-----------|-----------|--------|
| AddStore | O(1) | Dictionary insert |
| AddCar | O(1) | Dictionary insert |
| SearchCars (by store) | O(n×m) | n=cars at store, m=avg reservations per car |
| SearchCarsByCity | O(n×m) | n=cars in city, m=avg reservations per car |
| IsCarAvailableAtStore | O(m + log m) | m reservations + sort for last reservation |
| MakeReservation | O(m + log m) | Check availability with location logic |
| CompleteReservation | O(1) | Update car location |
| CancelReservation | O(1) | Dictionary lookup and remove |
| GetCarsAtStore | O(n) | Filter all cars by store |
| ProcessPayment | O(1) | Direct delegation |

---

## 8. Quick Demo Flow ⭐ UPDATED

```
1. Create RentalSystem instance
2. Add stores (Downtown, Airport, Central) ⭐
3. Add cars to different stores ⭐
4. Create customer
5. Search for available cars by store ⭐
6. Make reservation with pickup/return stores ⭐
7. Process payment (includes one-way fee if applicable) ⭐
8. Complete reservation (moves car to return store) ⭐
9. Show confirmation

Scenarios to demonstrate:
- Same-store rental
- One-way rental (different stores)
- Search by city
```

---

## 9. Key Takeaways for Interview

✅ **Start with clarification** - Don't assume requirements  
✅ **Think objects first** - Identify entities and relationships  
✅ **Explain design choices** - Why Singleton? Why Strategy?  
✅ **Handle edge cases** - Concurrent bookings, date overlaps  
✅ **Keep it simple first** - Can always extend later  
✅ **Discuss trade-offs** - Performance vs complexity  
✅ **Be ready for extensions** - Pricing, categories, scaling  

---

## 10. Common Mistakes to Avoid ⭐ UPDATED

❌ Not checking date overlaps properly  
❌ Forgetting to update car availability  
❌ **Not tracking car location after rentals** ⭐  
❌ **Allowing bookings when car is at wrong location** ⭐  
❌ **Forgetting one-way rental fees** ⭐  
❌ Hardcoding payment method (use interface)  
❌ Not handling null returns from MakeReservation  
❌ Ignoring concurrency issues  
❌ Making classes too complex upfront  
❌ **Not considering car movement between stores** ⭐  

---

**Total Time: ~45 minutes**
- Requirements: 5-10 min
- Design: 10-15 min  
- Code: 15-20 min
- Discussion: 5-10 min
