# Car Rental System - Interview Guide

## 1. Requirement Clarification (5-10 min)

**Interviewer**: "Design a car rental system."

**Your Questions**:
- "Can customers browse available cars and make reservations online?"
  - Yes, customers should search and reserve cars for specific dates.

- "What car details do we need to track?"
  - Make, model, year, license plate, and rental price per day.

- "How should customers search for cars?"
  - By make, model, price range, and date availability.

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
1. **Car** - represents vehicles in inventory
2. **Customer** - people renting cars
3. **Reservation** - booking made by customer
4. **RentalSystem** - manages the entire system
5. **PaymentProcessor** - handles payments (interface for flexibility)

### Key Relationships
- Customer **makes** Reservation
- Reservation **contains** Car
- RentalSystem **manages** Cars and Reservations
- RentalSystem **uses** PaymentProcessor

---

## 3. Design Discussion (10-15 min)

### A. Car Class Design
**Your thought**: "Cars are entities with properties and availability status."

```
Properties:
- Make, Model, Year, LicensePlate (immutable)
- RentalPricePerDay
- Available (mutable - changes with bookings)

Why?
- License plate is unique identifier
- Availability flag helps quick filtering
- Immutable properties prevent accidental changes
```

### B. Customer Class Design
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

### C. Reservation Class Design
**Your thought**: "Reservation connects customer and car with dates and calculates price."

```
Properties:
- ReservationId (unique identifier)
- Customer, Car (relationships)
- StartDate, EndDate
- TotalPrice (calculated on creation)

Key Design Choice:
- Calculate total price in constructor
- Price = (Days × RentalPricePerDay)
- Days = (EndDate - StartDate) + 1 (inclusive)
```

### D. Payment Processing
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

### E. RentalSystem Design (Critical!)
**Your thought**: "This is the facade coordinating everything."

```
Design Decisions:

1. Singleton Pattern
   Why? Single point of access, shared state

2. Data Storage
   - Dictionary<LicensePlate, Car>
   - Dictionary<ReservationId, Reservation>
   Why? Fast lookups O(1)

3. Core Operations
   - AddCar/RemoveCar (inventory management)
   - SearchCars (with availability check)
   - MakeReservation (with validation)
   - CancelReservation (free up car)
   - ProcessPayment (delegate to processor)
```

### F. Concurrency Handling
**Interviewer**: "How do you handle concurrent bookings?"

**Your answer**: 
```
Approach 1 (Current): 
- Use dictionaries with careful checks
- IsCarAvailable() checks date overlaps

Approach 2 (Better for Production):
- Use ConcurrentDictionary in C#
- Add locking around reservation creation
- Transaction-like behavior:
  1. Check availability
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

### Step 1: Start with Simple Classes

**Interviewer**: "Let's start coding."

```csharp
// Car - Simple entity
public class Car
{
    public string Make { get; }
    public string Model { get; }
    public int Year { get; }
    public string LicensePlate { get; }
    public double RentalPricePerDay { get; }
    public bool Available { get; private set; }

    public Car(string make, string model, int year, 
               string licensePlate, double rentalPricePerDay)
    {
        Make = make;
        Model = model;
        Year = year;
        LicensePlate = licensePlate;
        RentalPricePerDay = rentalPricePerDay;
        Available = true; // New cars are available
    }

    public void SetAvailable(bool available)
    {
        Available = available;
    }
}
```

**Talk through it**: "Immutable properties for car details, mutable availability flag."

---

### Step 2: Customer and Reservation

```csharp
// Customer - Data holder
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

// Reservation - Links customer, car, dates
public class Reservation
{
    public string ReservationId { get; }
    public Customer Customer { get; }
    public Car Car { get; }
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
    public double TotalPrice { get; }

    public Reservation(string reservationId, Customer customer, 
                       Car car, DateTime startDate, DateTime endDate)
    {
        ReservationId = reservationId;
        Customer = customer;
        Car = car;
        StartDate = startDate;
        EndDate = endDate;
        TotalPrice = CalculateTotalPrice();
    }

    private double CalculateTotalPrice()
    {
        int daysRented = (EndDate - StartDate).Days + 1;
        return Car.RentalPricePerDay * daysRented;
    }
}
```

**Talk through it**: "Reservation calculates price automatically. Days include both start and end."

---

### Step 3: Payment Strategy

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

### Step 4: The Core - RentalSystem

```csharp
public class RentalSystem
{
    private static RentalSystem instance;
    private readonly Dictionary<string, Car> cars;
    private readonly Dictionary<string, Reservation> reservations;
    private readonly IPaymentProcessor paymentProcessor;

    private RentalSystem()
    {
        cars = new Dictionary<string, Car>();
        reservations = new Dictionary<string, Reservation>();
        paymentProcessor = new CreditCardPaymentProcessor();
    }

    // Singleton access
    public static RentalSystem Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new RentalSystem();
            }
            return instance;
        }
    }

    // Add/Remove cars from inventory
    public void AddCar(Car car)
    {
        cars[car.LicensePlate] = car;
    }

    public void RemoveCar(string licensePlate)
    {
        cars.Remove(licensePlate);
    }

    // Search with availability check
    public List<Car> SearchCars(string make, string model, 
                                DateTime startDate, DateTime endDate)
    {
        return cars.Values
            .Where(car => car.Make.Equals(make, StringComparison.OrdinalIgnoreCase)
                       && car.Model.Equals(model, StringComparison.OrdinalIgnoreCase)
                       && car.Available 
                       && IsCarAvailable(car, startDate, endDate))
            .ToList();
    }

    // Check if car has no overlapping reservations
    private bool IsCarAvailable(Car car, DateTime startDate, DateTime endDate)
    {
        return reservations.Values
            .Where(reservation => reservation.Car == car)
            .All(reservation => endDate < reservation.StartDate 
                             || startDate > reservation.EndDate);
    }

    // Create reservation with validation
    public Reservation MakeReservation(Customer customer, Car car, 
                                       DateTime startDate, DateTime endDate)
    {
        if (IsCarAvailable(car, startDate, endDate))
        {
            var reservationId = GenerateReservationId();
            var reservation = new Reservation(reservationId, customer, 
                                             car, startDate, endDate);
            reservations[reservationId] = reservation;
            car.SetAvailable(false);
            return reservation;
        }
        return null; // Car not available
    }

    // Cancel and free up car
    public void CancelReservation(string reservationId)
    {
        if (reservations.TryGetValue(reservationId, out var reservation))
        {
            reservations.Remove(reservationId);
            reservation.Car.SetAvailable(true);
        }
    }

    // Process payment
    public bool ProcessPayment(Reservation reservation)
    {
        return paymentProcessor.ProcessPayment(reservation.TotalPrice);
    }

    // Generate unique ID
    private string GenerateReservationId()
    {
        return "RES" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
    }
}
```

**Key points to mention**:
- Singleton ensures single system instance
- IsCarAvailable checks date overlaps: `endDate < res.StartDate || startDate > res.EndDate`
- MakeReservation validates before creating
- Payment delegated to strategy pattern

---

## 5. Follow-up Questions & Answers

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

## 7. Time Complexity Analysis

| Operation | Complexity | Reason |
|-----------|-----------|--------|
| AddCar | O(1) | Dictionary insert |
| SearchCars | O(n×m) | n=cars, m=avg reservations per car |
| MakeReservation | O(m) | Check m reservations for conflicts |
| CancelReservation | O(1) | Dictionary lookup and remove |
| ProcessPayment | O(1) | Direct delegation |

---

## 8. Quick Demo Flow

```
1. Create RentalSystem instance
2. Add cars (Toyota, Honda, Ford)
3. Create customer
4. Search for available cars
5. Make reservation
6. Process payment
7. Show confirmation or handle failure
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

## 10. Common Mistakes to Avoid

❌ Not checking date overlaps properly  
❌ Forgetting to update car availability  
❌ Hardcoding payment method (use interface)  
❌ Not handling null returns from MakeReservation  
❌ Ignoring concurrency issues  
❌ Making classes too complex upfront  

---

**Total Time: ~45 minutes**
- Requirements: 5-10 min
- Design: 10-15 min  
- Code: 15-20 min
- Discussion: 5-10 min
