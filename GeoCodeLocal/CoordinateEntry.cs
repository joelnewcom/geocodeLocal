namespace GeoCodeLocal
{
    public class CoordinateEntry
    {
        public CoordinateEntry(string uuid, string lat, string lon)
        {
            this.uuid = uuid;
            this.lat = lat;
            this.lon = lon;
        }

        public String uuid { get; }
        public String lat { get; }
        public String lon { get; }
    }
}