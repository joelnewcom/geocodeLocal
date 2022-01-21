using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GeoCodeLocal
{
    public class IterState
    {
        private const int LOG_EVERY_N_LINES = 10000;
        int lineCounter;
        int failedCounter;
        int skippedCounter;
        Stopwatch batchStopWatch;

        internal List<CoordinateEntry> coordinates { get; set; }
        internal List<FailureEntry> failures { get; set; }
        private readonly int totalLines;

        internal List<DawCoordinateEntry> dawCoordinates { get; set; }

        public IterState(int totalLines)
        {
            this.totalLines = totalLines;
            lineCounter = 0;
            failedCounter = 0;
            skippedCounter = 0;
            batchStopWatch = new Stopwatch();
            coordinates = new List<CoordinateEntry>();
            dawCoordinates = new List<DawCoordinateEntry>();
            failures = new List<FailureEntry>();
        }

        internal void start()
        {
            batchStopWatch.Start();
        }
        internal void logSummary()
        {
            Console.WriteLine("There were {0} lines", lineCounter);
            Console.WriteLine("There were {0} failures", failedCounter);
            Console.WriteLine("There were {0} skipped lines", skippedCounter);
            Console.WriteLine("Successrate {0:N2} %", ((float)lineCounter - skippedCounter - failedCounter) * 100 / ((float)lineCounter - skippedCounter));
        }

        public void iterate()
        {
            lineCounter++;
            logging();
        }

        internal void Skip()
        {
            skippedCounter++;
        }

        internal void AddFailure(FailureEntry failureEntry)
        {
            failedCounter++;
            failures.Add(failureEntry);
        }

        internal void AddCoordinate(CoordinateEntry coordinateEntry)
        {
            coordinates.Add(coordinateEntry);
        }

        internal void AddCoordinate(DawCoordinateEntry coordinateEntry){
            dawCoordinates.Add(coordinateEntry);
        }

        private void logging()
        {
            if (lineCounter % 100 == 0)
            {
                Console.Write(".");
            }
            if (lineCounter % 10000 == 0)
            {
                batchStopWatch.Stop();

                Console.WriteLine("{0}/{1} lines processed within {2} ms. FailedCounter: {3}. SkippedCounter: {4}. Forecast until end of file reached: {5}",
                    lineCounter,
                    totalLines,
                    batchStopWatch.ElapsedMilliseconds,
                    failedCounter,
                    skippedCounter,
                    Runner.formatMilliseconds((totalLines - lineCounter) / LOG_EVERY_N_LINES * batchStopWatch.ElapsedMilliseconds));

                batchStopWatch.Reset();
                batchStopWatch.Start();
            }
        }
    }


}