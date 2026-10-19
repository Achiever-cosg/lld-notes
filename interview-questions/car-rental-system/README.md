# Designing a Multi-Location Car Rental System

## Requirements
1. The car rental system should allow customers to browse and reserve available cars for specific dates **across multiple store locations**.
2. Each car should have details such as make, model, year, license plate number, rental price per day, **and current store location**.
3. Customers should be able to search for cars based on various criteria, such as car type, price range, availability, **and store location or city**.
4. The system should handle reservations, including creating, modifying, and canceling reservations **with pickup and return store information**.
5. The system should keep track of the availability of cars and update their status accordingly **including tracking which store they're currently at**.
6. The system should handle customer information, including name, contact details, and driver's license information.
7. The system should handle payment processing for reservations **including one-way rental fees**.
8. The system should be able to handle concurrent reservations and ensure data consistency.
9. **The system should support one-way rentals (pickup at one store, return at another)**.
10. **The system should track car location changes as reservations are completed**.

## Classes, Interfaces and Enumerations
1. The **Store** class represents a rental location with properties such as store ID, name, address, city, and contact information.
2. The **Car** class represents a car in the rental system, with properties such as make, model, year, license plate number, rental price per day, availability status, **and current store location**.
3. The **Customer** class represents a customer, with properties like name, contact information, and driver's license number.
4. The **Reservation** class represents a reservation made by a customer for a specific car and date range. It includes properties such as reservation ID, customer, car, start date, end date, **pickup store, return store**, and total price (including one-way fees).
5. The **PaymentProcessor** interface defines the contract for payment processing, and the CreditCardPaymentProcessor and PayPalPaymentProcessor classes are concrete implementations of the payment processor.
6. The **RentalSystem** class is the core of the car rental system and follows the Singleton pattern to ensure a single instance of the rental system.
7. The RentalSystem class uses concurrent data structures (Dictionary) to handle concurrent access to stores, cars, and reservations.
8. The **RentalSystem** class provides methods for:
   - Adding and removing stores
   - Adding and removing cars
   - Searching for available cars by store or city
   - Making reservations with pickup/return stores
   - Completing reservations (moves car to return store)
   - Canceling reservations
   - Processing payments
9. The **CarRentalSystemDemo** class serves as the entry point of the application and demonstrates multi-location scenarios including same-store rentals, one-way rentals, and city-based searches.