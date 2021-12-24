using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace GeoCodeLocal
{
    public class Runner
    {
        public const String EXCLUDES = "exclude_place_ids=2005309,1223080,1398494,1414204,2017555,1257761,2030933,2582412,877133,1990445,2302011,867600";
        HttpClient client = new HttpClient();
        Stopwatch stopwatch = new Stopwatch();
        Store store;
        private string inputFile;
        private Mode mode;

        private ILineParser parser;

        public Runner(string inputFile, Mode mode, ILineParser parser)
        {
            this.inputFile = inputFile;
            this.mode = mode;
            this.parser = parser;
            this.store = new Store(mode);
        }

        public async Task run()
        {
            stopwatch.Start();

            IEnumerable<string> lines = File.ReadLines(inputFile);
            String headerLine = lines.First();
            IEnumerable<string> dataLines = lines.Skip(1);

            IterState iterState = new IterState(dataLines.Count());
            iterState.start();
            foreach (String line in dataLines)
            {

                iterState.iterate();
                iterState.coordinates = safeCoordinatesSoFarAndReset(iterState.coordinates);
                iterState.failures = safeFailuresSoFarAndReset(iterState.failures);

                if (!parser.IsValid(line))
                {
                    iterState.AddFailure(new FailureEntry("null", line, "line is invalid", "null"));
                    continue;
                }

                Address address = parser.Parse(line);

                if (isSkippable(address))
                {
                    iterState.Skip();
                    continue;
                }

                ResultState resultState = await geoCodeAddress(address);

                if (resultState.isFailed)
                {
                    iterState.AddFailure(new FailureEntry(address.Id, line, resultState.typeOfFailure, resultState.reasonOfFailure));
                    continue;
                }

                iterState.AddCoordinate(new CoordinateEntry(address.Id, line, resultState.coordinate));
            }

            if (iterState.coordinates.Count() > 0)
            {
                store.saveBulk(iterState.coordinates);
            }

            if (iterState.failures.Count() > 0)
            {
                store.saveFailures(iterState.failures);
            }

            stopwatch.Stop();
            store.Close();
            Console.WriteLine("Elapsed Time is {0} ms", stopwatch.ElapsedMilliseconds);
            iterState.logSummary();
            Console.ReadLine();
        }

        private List<FailureEntry> safeFailuresSoFarAndReset(List<FailureEntry> failures)
        {
            if (failures.Count() > 1000)
            {
                store.saveFailures(failures);
                failures = new List<FailureEntry>();
            }

            return failures;
        }

        private List<CoordinateEntry> safeCoordinatesSoFarAndReset(List<CoordinateEntry> coordinates)
        {
            if (coordinates.Count() > 1000)
            {
                store.saveBulk(coordinates);
                coordinates = new List<CoordinateEntry>();
            }

            return coordinates;
        }

        private async Task<ResultState> geoCodeAddress(Address address)
        {
            List<NominatimResponse>? nominatimResponse = new List<NominatimResponse>();
            try
            {
                HttpResponseMessage responseMessage = await client.GetAsync(CreateQueryParams(address.Street, address.City, address.Postalcode));
                nominatimResponse = JsonSerializer.Deserialize<List<NominatimResponse>>(await responseMessage.Content.ReadAsStringAsync());
            }
            catch (HttpRequestException e)
            {
                return new ResultState(null, true, "HttpRequestException", e.Message);
            }
            catch (JsonException e)
            {
                return new ResultState(null, true, "JsonException", e.Message);
            }

            if (nominatimResponse is null || nominatimResponse.Count() < 1)
            {
                return new ResultState(null, true, "No result from Nominatim", "null");
            }

            // We are interested in categories: place, building, amenity
            nominatimResponse = nominatimResponse.Where(n => !"highway".Equals(n.category)).ToList();

            if (nominatimResponse.Count() < 1)
            {
                return new ResultState(null, true, "no building", "null");
            }

            if (nominatimResponse.Count() > 1)
            {
                int sameLatitudes = nominatimResponse.Select(x => x.lat).Distinct().Count();
                int samelongitudes = nominatimResponse.Select(x => x.lon).Distinct().Count();

                if (sameLatitudes.Equals(nominatimResponse.Count) && samelongitudes.Equals(nominatimResponse.Count))
                {
                    nominatimResponse = new List<NominatimResponse> { nominatimResponse.First() };
                }
            }

            if (nominatimResponse.Count != 1)
            {
                StringBuilder osmIds = new StringBuilder();
                foreach (NominatimResponse item in nominatimResponse)
                {
                    osmIds.Append("category: " + item.category + "; osmid: " + item.osm_id + ", ");
                }
                return new ResultState(null, true, "got more than one location", osmIds.ToString());
            }

            string? lat = nominatimResponse[0].lat;
            string? lon = nominatimResponse[0].lon;

            if (lat is not null && lon is not null)
            {
                return new ResultState(new Coordinate(lat, lon), false, null, null);

            }
            else
            {
                return new ResultState(null, true, "lat or long is null", "null");
            }

        }

        private bool isSkippable(Address address)
        {
            return Mode.proceed.Equals(mode) && store.getId(address.Id) != null;
        }

        public static String formatMilliseconds(long ms)
        {
            TimeSpan t = TimeSpan.FromMilliseconds(ms);
            return string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                        t.Hours,
                        t.Minutes,
                        t.Seconds,
                        t.Milliseconds);
        }

        /// place_id<2005309> = highway
        /// (paramCounter > 0 ? "&" : "?")
        public string CreateQueryParams(string? street, string? city, string? postalcode)
        {
            int paramCounter = 0;
            StringBuilder stringBuilder = new StringBuilder("http://localhost:8080/search?" + EXCLUDES);

            if (!String.IsNullOrWhiteSpace(street) && !"NULL".Equals(street))
            {
                stringBuilder.Append("&street=" + street);
                paramCounter++;
            }

            if (!String.IsNullOrWhiteSpace(city) && !"NULL".Equals(city))
            {
                stringBuilder.Append("&city=" + city);
                paramCounter++;
            }

            if (!String.IsNullOrWhiteSpace(postalcode) && !"NULL".Equals(postalcode))
            {
                stringBuilder.Append("&postalcode=" + postalcode);
            }

            return stringBuilder.ToString();
        }
    }

    internal class ResultState
    {
        public ResultState(Coordinate? coordinate, bool isFailed, string? typeOfFailure, string? reasonOfFailure)
        {
            this.coordinate = coordinate;
            this.isFailed = isFailed;
            this.reasonOfFailure = reasonOfFailure;
            this.typeOfFailure = typeOfFailure;
        }

        internal Coordinate? coordinate { get; }
        internal Boolean isFailed { get; }

        internal String? typeOfFailure { get; }

        internal String? reasonOfFailure { get; }
    }

    public class Coordinate
    {
        public Coordinate(string lat, string lon)
        {
            this.lat = lat;
            this.lon = lon;
        }
        public string lat { get; }
        public string lon { get; }
    }
}