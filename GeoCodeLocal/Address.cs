using System;

namespace GeoCodeLocal
{
    public class Address
    {

        public Address(string id, string street, string city, string postalcode)
        {
            Postalcode = postalcode;
            Id = id;
            Street = street;
            City = city;
        }

        public String Id { get; }
        public String Street { get; }
        public String City { get; }
        public String Postalcode { get; }
    }
}