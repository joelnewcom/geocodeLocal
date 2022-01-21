using System;
using System.Security.Cryptography;
using System.Text;

namespace GeoCodeLocal
{
    public class DawCoordinateEntry
    {
        public string Id { get; }
        public string CustomerId { get; }
        public string AddressHash { get; }
        public string Longitude { get; }
        public string Latitude { get; }
        public string FullAddress { get; }
        public string City { get; }
        public string Street { get; }
        public string PostalCode { get; }
        public string HouseNumber { get; }
        public string Region { get; }
        public DateTime CreatedDate { get; }
        public DateTime ModifiedDate { get; }
        public int Country { get; }
        public int Results { get; }

        // Id;CustomerId;AddressHash;Longitude;Latitude;FullAddress;City;Street;PostalCode;HouseNumber;Region;CreatedDate;ModifiedDate;Country;Results
        public DawCoordinateEntry(String customerId, String longitude, String latitude, String city, String street, String postalCode, int results)
        {
            Id = Guid.NewGuid().ToString();
            Region = "NULL";
            Country = 0;
            HouseNumber = "NULL";
            CustomerId = customerId;
            Longitude = longitude;
            Latitude = latitude;
            City = city;
            Street = street;
            PostalCode = postalCode;
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
            Results = results;

          //string fullAddressDetails = $"{updateRequest.Street}{updateRequest.HouseNumber}{updateRequest.PostalCode}{updateRequest.City}{updateRequest.Region}";
            string fullAddressDetails = $"{street}{postalCode}{city}";
            string fullAddressDetailsSeparator = $"{street}, {null}, {postalCode}, {city}, {null}";

            FullAddress = fullAddressDetailsSeparator;
            AddressHash = ConvertAddressToHash(fullAddressDetails);
        }

        private string ConvertAddressToHash(string RawAddredd)
        {
            string addressHash = "";
            using (SHA256 sha256Hash = SHA256.Create())
            {
                addressHash = GetHash(sha256Hash, RawAddredd);
            }
            return addressHash;
        }

        private static string GetHash(HashAlgorithm hashAlgorithm, string input)
        {

            byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

            var sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }
    }
}