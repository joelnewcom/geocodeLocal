using System;

namespace GeoCodeLocal
{
    public class CoordinateEntry
    {
        public CoordinateEntry(string uuid, string address, Coordinate coordinate)
        {
            this.uuid = uuid;
            this.address = address;
            this.coordinate = coordinate;
        }

        public String uuid { get; }
        public Coordinate coordinate { get; }
        public String address { get; }
    }
}