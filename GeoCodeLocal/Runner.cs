using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace GeoCodeLocal
{
    public class Runner
    {
        private const int LOG_EVERY_N_LINES = 10000;

        public const String EXCLUDES = "exclude_place_ids=2005309,1223080,1398494,1414204,2017555,1257761,2030933,2582412,877133,1990445,2302011,867600";
        HttpClient client = new HttpClient();
        Stopwatch stopwatch = new Stopwatch();
        Stopwatch batchStopWatch = new Stopwatch();
        Store store;
        private string inputFile;
        private string mode;

        private ILineParser parser;

        public Runner(string inputFile, string mode, ILineParser parser)
        {
            this.inputFile = inputFile;
            this.mode = mode;
            this.parser = parser;
            this.store = new Store(mode);
        }

        public async Task run()
        {
            int lineCounter = 0;
            int skippedCounter = 0;
            stopwatch.Start();
            batchStopWatch.Start();

            IEnumerable<string> lines = File.ReadLines(inputFile);
            String headerLine = lines.First();
            IEnumerable<string> dataLines = lines.Skip(1);

            int totalLines = dataLines.Count();

            List<CoordinateEntry> coordinates = new List<CoordinateEntry>();
            List<FailureEntry> failures = new List<FailureEntry>();

            foreach (String line in dataLines)
            {
                lineCounter++;
                logging(lineCounter, skippedCounter, totalLines);

                if (coordinates.Count() > 1000)
                {
                    store.saveBulk(coordinates);
                    coordinates = new List<CoordinateEntry>();
                }

                if (failures.Count() > 1000)
                {
                    store.saveFailures(failures);
                    failures = new List<FailureEntry>();
                }

                if (!parser.IsValid(line))
                {
                    failures.Add(new FailureEntry("null", line, "line is invalid", "null"));
                    skippedCounter++;
                    continue;
                }

                Address address = parser.Parse(line);

                if ("proceed".Equals(mode) && store.getId(address.Id) != null)
                {
                    failures.Add(new FailureEntry("null", line, "line already stored", "null"));
                    skippedCounter++;
                    continue;
                }

                List<NominatinResponse>? nominatinResponse = new List<NominatinResponse>();
                HttpResponseMessage responseMessage;
                try
                {
                    responseMessage = await client.GetAsync(CreateQueryParams(address.Street, address.City, address.Postalcode));
                    nominatinResponse = JsonSerializer.Deserialize<List<NominatinResponse>>(await responseMessage.Content.ReadAsStringAsync());
                }
                catch (HttpRequestException e)
                {
                    failures.Add(new FailureEntry(address.Id, line, e.Message, "null"));
                    skippedCounter++;
                    continue;
                }
                catch (JsonException e)
                {
                    failures.Add(new FailureEntry(address.Id, line, e.Message, "null"));
                    skippedCounter++;
                    continue;
                }

                if (nominatinResponse is null || nominatinResponse.Count() < 1)
                {
                    failures.Add(new FailureEntry(address.Id, line, "no result", "null"));
                    skippedCounter++;
                    continue;
                }

                if (nominatinResponse.Count() > 1)
                {

                    List<NominatinResponse> buildings = new List<NominatinResponse>();
                    // categories: place, highway, building, amenity
                    buildings = nominatinResponse.Where(n => !"highway".Equals(n.category)).ToList();

                    if (buildings.Count != 1)
                    {
                        StringBuilder osmIds = new StringBuilder();
                        foreach (NominatinResponse item in nominatinResponse)
                        {
                            osmIds.Append("category: " + item.category + "; osmid: " + item.osm_id + ", ");
                        }
                        failures.Add(new FailureEntry(address.Id, line, "more than one result", osmIds.ToString()));
                        continue;
                    }
                    nominatinResponse = buildings;
                }

                if (nominatinResponse[0].lat is null || nominatinResponse[0].lon is null)
                {
                    failures.Add(new FailureEntry(address.Id, line, "lat or long is null", "null"));
                    skippedCounter++;
                    continue;
                }
                coordinates.Add(new CoordinateEntry(address.Id, nominatinResponse[0].lat, nominatinResponse[0].lon, line));
            }

            if (coordinates.Count() > 0)
            {
                store.saveBulk(coordinates);
            }

            if (failures.Count() > 0)
            {
                store.saveFailures(failures);
            }

            stopwatch.Stop();
            store.Close();
            Console.WriteLine("Elapsed Time is {0} ms", stopwatch.ElapsedMilliseconds);
            Console.WriteLine("There were {0} lines.", lineCounter);
            Console.WriteLine("There were {0} failures.", skippedCounter);
            Console.WriteLine("Successrate: {0:N2} %.", 100.00 - skippedCounter * 100 / lineCounter);
            Console.ReadLine();
        }

        private void logging(int lineCounter, int skippedCounter, int totalLines)
        {
            if (lineCounter % 100 == 0)
            {
                Console.Write(".");
            }
            if (lineCounter % 10000 == 0)
            {
                batchStopWatch.Stop();
                Console.WriteLine("{0}/{1} lines processed within {2} ms. FailedCounter: {3}. Forecast until end of file reached: {4}",
                lineCounter,
                totalLines,
                batchStopWatch.ElapsedMilliseconds,
                skippedCounter,
                formatMilliseconds((totalLines - lineCounter) / LOG_EVERY_N_LINES * batchStopWatch.ElapsedMilliseconds));
                batchStopWatch.Reset();
                batchStopWatch.Start();
            }
        }

        private String formatMilliseconds(long ms)
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
        public string CreateQueryParams(string street, string city, string postalcode)
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
}