using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace GeoCodeLocal
{
    public class Runner
    {
        private const int LOG_EVERY_N_LINES = 10000;
        HttpClient client = new HttpClient();
        Stopwatch stopwatch = new Stopwatch();
        Stopwatch batchStopWatch = new Stopwatch();
        Store store = new Store();
        private string inputFile;
        private string mode;

        private ILineParser parser;

        public Runner(string inputFile, string mode, ILineParser parser)
        {
            this.inputFile = inputFile;
            this.mode = mode;
            this.parser = parser;
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

            List<CoordinateEntry> list = new List<CoordinateEntry>();

            foreach (String line in dataLines)
            {
                lineCounter++;
                if (lineCounter % 1000 == 0)
                {
                    store.bulkSave(list);
                    list = new List<CoordinateEntry>();
                }
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

                if (!parser.IsValid(line))
                {
                    skippedCounter++;
                    continue;
                }

                Address adress = parser.Parse(line);

                // if (store.getId(adress.Id) != null)
                // {
                //     skippedCounter++;
                //     continue;
                // }

                List<NominatinResponse>? nominatinResponse = new List<NominatinResponse>();
                HttpResponseMessage responseMessage;
                try
                {
                    // Make call. Here we loose time
                    responseMessage = await client.GetAsync(CreateQueryParams(adress.Street, adress.City, adress.Postalcode));
                    nominatinResponse = JsonSerializer.Deserialize<List<NominatinResponse>>(await responseMessage.Content.ReadAsStringAsync());
                }
                catch (HttpRequestException e)
                {
                    Console.Write(e.Message);
                    skippedCounter++;
                    continue;
                }
                catch (JsonException e)
                {
                    Console.Write(e.Message);
                    skippedCounter++;
                    continue;
                }

                if (nominatinResponse is null || nominatinResponse.Count() != 1)
                {
                    skippedCounter++;
                    continue;
                }

                list.Add(new CoordinateEntry(adress.Id, nominatinResponse[0].lat, nominatinResponse[0].lon));
            }

            stopwatch.Stop();
            store.Close();
            Console.WriteLine("Elapsed Time is {0} ms", stopwatch.ElapsedMilliseconds);
            Console.WriteLine("There were {0} lines.", lineCounter);
            Console.WriteLine("There were {0} failures.", skippedCounter);
            Console.ReadLine();
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
        public string CreateQueryParams(string street, string city, string postalcode)
        {
            int paramCounter = 0;
            StringBuilder stringBuilder = new StringBuilder("http://localhost:8080/search?exclude_place_ids=2005309,1223080,1398494,1414204,2017555,1257761,2030933,2582412,877133,1990445,2302011,867600");

            if (!String.IsNullOrWhiteSpace(street) && !"NULL".Equals(street))
            {
                stringBuilder.Append("&street=" + street);
                paramCounter++;
            }

            if (!String.IsNullOrWhiteSpace(city) && !"NULL".Equals(city))
            {
                stringBuilder.Append((paramCounter > 0 ? "&" : "?") + "city=" + city);
                paramCounter++;
            }

            if (!String.IsNullOrWhiteSpace(postalcode) && !"NULL".Equals(postalcode))
            {
                stringBuilder.Append((paramCounter > 0 ? "&" : "?") + "postalcode=" + postalcode);
            }

            return stringBuilder.ToString();
        }
    }
}