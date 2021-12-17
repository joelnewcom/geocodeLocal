namespace GeoCodeLocal
{
    public class ZKPExtractParser : ILineParser
    {
        public bool IsValid(String line)
        {
            return line.Split(",").Length == 5;
        }
        public Address Parse(String line)
        {
            String[] splittedLine = line.Split(",");
            return new Address
            {
                Id = splittedLine[0],
                Street = splittedLine[1] + " " + splittedLine[2],
                City = splittedLine[4],
                Postalcode = splittedLine[3]
            };
        }
    }
}