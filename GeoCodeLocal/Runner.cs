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

        public Runner(string inputFile, string mode)
        {
            this.inputFile = inputFile;
            this.mode = mode;
        }

        public async Task run()
        {
            int lineCounter = 0;
            int skippedCounter = 0;
            String id = "";
            String street = "";
            String city = "";
            String postalcode = "";
            stopwatch.Start();
            batchStopWatch.Start();

            IEnumerable<string> lines = File.ReadLines(inputFile);
            String headerLine = lines.First();
            IEnumerable<string> dataLines = lines.Skip(1);

            int totalLines = dataLines.Count();

            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = 1;

            foreach (String line in dataLines)
            {
                lineCounter++;
                String[] splittedLine = line.Split(",");
                if (splittedLine.Length != 5)
                {
                    skippedCounter++;
                    continue;
                }

                id = splittedLine[0];
                street = splittedLine[1] + " " + splittedLine[2];
                city = splittedLine[4];
                postalcode = splittedLine[3];


                string? v = store.getId(id);
                if (v != null)
                {
                    skippedCounter++;
                    continue;
                }

                List<NominatinResponse>? nominatinResponse = new List<NominatinResponse>();
                try
                {
                    // Make call. Here we loose time
                    HttpResponseMessage responseMessage = await client.GetAsync(CreateQueryParams(street, city, postalcode));
                    nominatinResponse = JsonSerializer.Deserialize<List<NominatinResponse>>(await responseMessage.Content.ReadAsStringAsync());
                }
                catch (HttpRequestException e)
                {
                    Console.Write(e.Message);
                    skippedCounter++;
                    continue;
                }

                if (nominatinResponse is null || nominatinResponse.Count() != 1)
                {
                    skippedCounter++;
                    return;
                }

                store.save(id, nominatinResponse[0].lat, nominatinResponse[0].lon);

                if (lineCounter % 100 == 0)
                {
                    Console.Write(".");
                }
                if (lineCounter % 10000 == 0)
                {
                    batchStopWatch.Stop();
                    Console.WriteLine("{0}/{1} lines processed within {2} ms. Forecast until end of file reached: {3}",
                    lineCounter,
                    totalLines,
                    batchStopWatch.ElapsedMilliseconds,
                    formatMilliseconds((totalLines - lineCounter) / LOG_EVERY_N_LINES * batchStopWatch.ElapsedMilliseconds));

                    batchStopWatch.Reset();
                    batchStopWatch.Start();
                }
            }

            stopwatch.Stop();
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
        public string CreateQueryParams(string street, string city, string postalcode)
        {
            int paramCounter = 0;
            StringBuilder stringBuilder = new StringBuilder("http://localhost:8080/search");

            if (!String.IsNullOrWhiteSpace(street) && !"NULL".Equals(street))
            {
                stringBuilder.Append("?street=" + street);
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