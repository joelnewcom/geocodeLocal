using System;

namespace GeoCodeLocal
{
    public interface ILineParser
    {
        Boolean IsValid(String line);
        Address Parse(String line);
    }

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
            return new Address(
                id: Guid.NewGuid().ToString(),
                street: splittedLine[3] + " " + splittedLine[4],
                city: splittedLine[2],
                postalcode: splittedLine[1]
                );
        }
    }

    public class Format1Parser : ILineParser
    {
        public bool IsValid(String line)
        {
            return line.Split(",").Length == 5;
        }
        public Address Parse(String line)
        {
            String[] splittedLine = line.Split(",");
            return new Address(
                id: splittedLine[0],
                street: splittedLine[1] + " " + splittedLine[2],
                city: splittedLine[4],
                postalcode: splittedLine[3]
            );
        }
    }

    public class LineParserFactory
    {
        public ILineParser create(Parser parser)
        {
            switch (parser)
            {
                case Parser.format1:
                    return new Format1Parser();
                case Parser.samzurcher:
                    return new SamZurcherParser();
                default:
                    throw new ArgumentException("no parser defined");

            }

        }
    }
}