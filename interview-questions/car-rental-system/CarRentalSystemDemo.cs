using System;
using System.Collections.Generic;

namespace CarRentalSystem
{
    public class CarRentalSystemDemo
    {
        public static void Run()
        {
            var rentalSystem = RentalSystem.Instance;

            // Add stores
            var store1 = new Store("S001", "Downtown Branch", "123 Main St", "New York", "212-555-0100");
            var store2 = new Store("S002", "Airport Branch", "456 Airport Rd", "New York", "212-555-0200");
            var store3 = new Store("S003", "Central Branch", "789 Central Ave", "Boston", "617-555-0100");

            rentalSystem.AddStore(store1);
            rentalSystem.AddStore(store2);
            rentalSystem.AddStore(store3);

            // Add cars to different stores
            rentalSystem.AddCar(new Car("Toyota", "Camry", 2022, "ABC123", 50.0, store1));
            rentalSystem.AddCar(new Car("Honda", "Civic", 2021, "XYZ789", 45.0, store1));
            rentalSystem.AddCar(new Car("Ford", "Mustang", 2023, "DEF456", 80.0, store2));
            rentalSystem.AddCar(new Car("Toyota", "Camry", 2023, "GHI789", 55.0, store3));

            // Create customers
            var customer1 = new Customer("John Doe", "john@example.com", "DL1234");

            // Scenario 1: Same store pickup and return
            Console.WriteLine("=== Scenario 1: Same Store Rental ===");
            var startDate = DateTime.Now;
            var endDate = startDate.AddDays(3);
            List<Car> availableCars = rentalSystem.SearchCars("Toyota", "Camry", store1.StoreId, startDate, endDate);
            
            if (availableCars.Count > 0)
            {
                Car selectedCar = availableCars[0];
                var reservation = rentalSystem.MakeReservation(customer1, selectedCar, startDate, endDate, 
                                                               store1, store1);
                if (reservation != null)
                {
                    bool paymentSuccess = rentalSystem.ProcessPayment(reservation);
                    if (paymentSuccess)
                    {
                        Console.WriteLine($"Reservation successful!");
                        Console.WriteLine($"Reservation ID: {reservation.ReservationId}");
                        Console.WriteLine($"Pickup Store: {reservation.PickupStore.Name}");
                        Console.WriteLine($"Return Store: {reservation.ReturnStore.Name}");
                        Console.WriteLine($"Total Price: ${reservation.TotalPrice}");
                        
                        // Complete the reservation
                        rentalSystem.CompleteReservation(reservation.ReservationId);
                        Console.WriteLine($"Car returned to {reservation.ReturnStore.Name}");
                    }
                    else
                    {
                        Console.WriteLine("Payment failed. Reservation canceled.");
                        rentalSystem.CancelReservation(reservation.ReservationId);
                    }
                }
            }

            Console.WriteLine("\n=== Scenario 2: One-Way Rental (Different Stores) ===");
            var startDate2 = DateTime.Now.AddDays(5);
            var endDate2 = startDate2.AddDays(2);
            List<Car> availableCars2 = rentalSystem.SearchCars("Ford", "Mustang", store2.StoreId, startDate2, endDate2);
            
            if (availableCars2.Count > 0)
            {
                Car selectedCar2 = availableCars2[0];
                var reservation2 = rentalSystem.MakeReservation(customer1, selectedCar2, startDate2, endDate2, 
                                                                store2, store1);
                if (reservation2 != null)
                {
                    Console.WriteLine($"One-way reservation successful!");
                    Console.WriteLine($"Reservation ID: {reservation2.ReservationId}");
                    Console.WriteLine($"Pickup Store: {reservation2.PickupStore.Name}");
                    Console.WriteLine($"Return Store: {reservation2.ReturnStore.Name}");
                    Console.WriteLine($"Total Price: ${reservation2.TotalPrice} (includes $50 one-way fee)");
                }
            }

            Console.WriteLine("\n=== Scenario 3: Search by City ===");
            List<Car> bostonCars = rentalSystem.SearchCarsByCity("Toyota", "Camry", "Boston", 
                                                                 DateTime.Now, DateTime.Now.AddDays(1));
            Console.WriteLine($"Found {bostonCars.Count} Toyota Camry in Boston");
            foreach (var car in bostonCars)
            {
                Console.WriteLine($"  - {car.Year} {car.Make} {car.Model} at {car.CurrentStore.Name}");
            }
        }
    }
}