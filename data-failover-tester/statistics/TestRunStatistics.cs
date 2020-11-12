using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Contino.SRE.Tools.Data.Failover.Statistics
{
    public class TestRunStatistics
    {
        public double LongestWriteLatency => WriteLatencies.Max(ts => ts.TotalMilliseconds);

        public double LongestReadLatency => ReadLatencies.Max(ts => ts.TotalMilliseconds);

        public double AverageReadLatency(double percentile)
        {
            ReadLatencies.OrderByDescending(ts => ts.TotalMilliseconds);
            var centile = Convert.ToInt32(Math.Round(0.90 * ReadLatencies.Count(), 1));
            return ReadLatencies.ElementAt(centile).TotalMilliseconds;
        }

        public double AverageWriteLatency(double percentile)
        {
            WriteLatencies.OrderByDescending(ts => ts.TotalMilliseconds);
            var centile = Convert.ToInt32(Math.Round(0.90 * WriteLatencies.Count(), 1));
            return WriteLatencies.ElementAt(centile).TotalMilliseconds;
        }

        public int TotalWrites => WriteLatencies.Count();

        public int TotalReads => ReadLatencies.Count();

        public int TotalErrors { get; private set; }

        public double TotalTimeTaken => WriteLatencies.Concat(ReadLatencies).Sum(ts => ts.Milliseconds);

        private Stopwatch _statisticTimer { get; }

        private Collection<TimeSpan> WriteLatencies = new Collection<TimeSpan>();

        private Collection<TimeSpan> ReadLatencies = new Collection<TimeSpan>();

        private IReportGenerator _reportGenerator { get; }
        
        public TestRunStatistics(IReportGenerator reportGenerator)
        {
            _statisticTimer = new Stopwatch();
            _reportGenerator = reportGenerator;
        }

        public void StartCollecting()
        {
            _statisticTimer.Start();
        }

        public void StopCollection()
        {
            _statisticTimer.Stop();
        }

        public void AddWriteOperation(TimeSpan writeOperationLatency)
        {
            WriteLatencies.Add(writeOperationLatency);
        }

        public void AddReadOperation(TimeSpan readOperationLatency)
        {
            ReadLatencies.Add(readOperationLatency);
        }

        public void AddError()
        {
            TotalErrors++;
        }

        public void OutputReport()
        {
            _reportGenerator.Generate(this);
        }
    }
}