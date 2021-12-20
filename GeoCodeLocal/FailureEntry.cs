namespace GeoCodeLocal
{
    public class FailureEntry
    {
        public FailureEntry(string uuid, string address, String reason, String osmTypeIds)
        {
            this.uuid = uuid;
            this.address = address;
            this.osmTypeIds = osmTypeIds;
            this.reason = reason;
        }

        public String uuid { get; }
        public String reason { get; }
        public String osmTypeIds { get; }
        public String address { get; }
    }
}