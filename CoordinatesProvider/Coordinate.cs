namespace CoordinatesProvider
{
    public class Coordinate
    {
        public Coordinate(string customerId, string longitude, string latitude, string fullAddress)
        {
            CustomerId = customerId;
            Longitude = longitude;
            Latitude = latitude;
            FullAddress = fullAddress;
        }

        public string CustomerId { get; }
        public string Longitude { get; }
        public string Latitude { get; }
        public string FullAddress { get; }
    }
}