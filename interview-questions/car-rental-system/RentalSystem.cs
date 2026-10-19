using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CarRentalSystem
{
    public class RentalSystem
    {
        private static RentalSystem instance;
        private readonly Dictionary<string, Store> stores;
        private readonly Dictionary<string, Car> cars;
        private readonly Dictionary<string, Reservation> reservations;
        private readonly IPaymentProcessor paymentProcessor;

        private RentalSystem()
        {
            stores = new Dictionary<string, Store>();
            cars = new Dictionary<string, Car>();
            reservations = new Dictionary<string, Reservation>();
            paymentProcessor = new CreditCardPaymentProcessor();  // Default to credit card payment processor
        }

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

        public void AddStore(Store store)
        {
            stores[store.StoreId] = store;
        }

        public void RemoveStore(string storeId)
        {
            stores.Remove(storeId);
        }

        public List<Store> GetAllStores()
        {
            return stores.Values.ToList();
        }

        public Store GetStore(string storeId)
        {
            stores.TryGetValue(storeId, out var store);
            return store;
        }

        public void AddCar(Car car)
        {
            cars[car.LicensePlate] = car;
        }

        public void RemoveCar(string licensePlate)
        {
            cars.Remove(licensePlate);
        }

        public List<Car> SearchCars(string make, string model, string storeId, 
                                    DateTime startDate, DateTime endDate)
        {
            return cars.Values
                .Where(car => car.Make.Equals(make, StringComparison.OrdinalIgnoreCase)
                              && car.Model.Equals(model, StringComparison.OrdinalIgnoreCase)
                              && car.CurrentStore.StoreId == storeId
                              && car.Available 
                              && IsCarAvailableAtStore(car, storeId, startDate, endDate))
                .ToList();
        }

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

        private bool IsCarAvailable(Car car, DateTime startDate, DateTime endDate)
        {
            return reservations.Values
                .Where(reservation => reservation.Car == car)
                .All(reservation => endDate < reservation.StartDate || startDate > reservation.EndDate);
        }

        private bool IsCarAvailableAtStore(Car car, string storeId, DateTime startDate, DateTime endDate)
        {
            // Check if car will be at the requested store on start date
            var conflictingReservations = reservations.Values
                .Where(reservation => reservation.Car == car)
                .Where(reservation => !(endDate < reservation.StartDate || startDate > reservation.EndDate))
                .ToList();

            if (conflictingReservations.Any())
            {
                return false;
            }

            // Check if car will be returned to this store before pickup date
            var lastReservation = reservations.Values
                .Where(reservation => reservation.Car == car && reservation.EndDate < startDate)
                .OrderByDescending(reservation => reservation.EndDate)
                .FirstOrDefault();

            if (lastReservation != null)
            {
                return lastReservation.ReturnStore.StoreId == storeId;
            }

            return car.CurrentStore.StoreId == storeId;
        }

        public Reservation MakeReservation(Customer customer, Car car, 
                                          DateTime startDate, DateTime endDate,
                                          Store pickupStore, Store returnStore)
        {
            if (IsCarAvailableAtStore(car, pickupStore.StoreId, startDate, endDate))
            {
                var reservationId = GenerateReservationId();
                var reservation = new Reservation(reservationId, customer, car, 
                                                 startDate, endDate, pickupStore, returnStore);
                reservations[reservationId] = reservation;
                car.SetAvailable(false);
                return reservation;
            }
            return null;
        }

        public void CompleteReservation(string reservationId)
        {
            if (reservations.TryGetValue(reservationId, out var reservation))
            {
                // Move car to return store
                reservation.Car.SetCurrentStore(reservation.ReturnStore);
                reservation.Car.SetAvailable(true);
            }
        }

        public void CancelReservation(string reservationId)
        {
            if (reservations.TryGetValue(reservationId, out var reservation))
            {
                reservations.Remove(reservationId);
                reservation.Car.SetAvailable(true);
            }
        }

        public bool ProcessPayment(Reservation reservation)
        {
            return paymentProcessor.ProcessPayment(reservation.TotalPrice);
        }

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
}