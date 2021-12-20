namespace GeoCodeLocal
{
    public class CoordinateEntry
    {
        public CoordinateEntry(string uuid, string lat, string lon, string address)
        {
            this.uuid = uuid;
            this.lat = lat;
            this.lon = lon;
            this.address = address;
        }

        public String uuid { get; }
        public String lat { get; }
        public String lon { get; }
        public String address { get; }
    }
}