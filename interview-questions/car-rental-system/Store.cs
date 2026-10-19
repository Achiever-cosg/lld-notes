namespace CarRentalSystem
{
    public class Store
    {
        public string StoreId { get; }
        public string Name { get; }
        public string Address { get; }
        public string City { get; }
        public string ContactInfo { get; }

        public Store(string storeId, string name, string address, string city, string contactInfo)
        {
            StoreId = storeId;
            Name = name;
            Address = address;
            City = city;
            ContactInfo = contactInfo;
        }
    }
}
