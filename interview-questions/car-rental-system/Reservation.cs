using System;

namespace CarRentalSystem
{
    public class Reservation
    {
        public string ReservationId { get; }
        public Customer Customer { get; }
        public Car Car { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public Store PickupStore { get; }
        public Store ReturnStore { get; }
        public double TotalPrice { get; }

        public Reservation(string reservationId, Customer customer, Car car, 
                          DateTime startDate, DateTime endDate, 
                          Store pickupStore, Store returnStore)
        {
            ReservationId = reservationId;
            Customer = customer;
            Car = car;
            StartDate = startDate;
            EndDate = endDate;
            PickupStore = pickupStore;
            ReturnStore = returnStore;
            TotalPrice = CalculateTotalPrice();
        }

        private double CalculateTotalPrice()
        {
            int daysRented = (EndDate - StartDate).Days + 1;
            double basePrice = Car.RentalPricePerDay * daysRented;
            
            // Add one-way fee if pickup and return stores are different
            if (PickupStore.StoreId != ReturnStore.StoreId)
            {
                basePrice += 50.0; // One-way rental fee
            }
            
            return basePrice;
        }
    }
}