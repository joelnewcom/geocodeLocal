using GeoCodeLocal;

/// cantonShort;postalCode;city;streetName;streetNumber
public class SamZurcherParser : ILineParser
{
    public bool IsValid(string line)
    {
        return line.Split(";").Length == 5;
    }

    public Address Parse(string line)
    {
        String[] splittedLine = line.Split(";");
        return new Address
        {
            Id = Guid.NewGuid().ToString(),
            Street = splittedLine[3] + " " + splittedLine[4],
            City = splittedLine[2],
            Postalcode = splittedLine[1]
        };
    }
}