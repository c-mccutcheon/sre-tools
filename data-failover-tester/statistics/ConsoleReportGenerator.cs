using System;
using System.Diagnostics;

namespace Contino.SRE.Tools.Data.Failover.Statistics
{
    public class ConsoleReportGenerator : IReportGenerator
    {
        public void Generate(TestRunStatistics statistics)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("#### --- Test Statistics --- #####");
            Console.WriteLine("Total Writes         :: " + statistics.TotalWrites);
            Console.WriteLine("Total Reads          :: " + statistics.TotalReads);
            Console.WriteLine("Total Errors         :: " + statistics.TotalErrors);
            Console.WriteLine("Total Time Taken     :: " + statistics.TotalTimeTaken + "ms");
            Console.WriteLine("Longest Read Latency :: " + statistics.LongestReadLatency + "ms");
            Console.WriteLine("Average Read Latency :: " + statistics.AverageReadLatency(0.90) + "ms");
            Console.WriteLine("Longest Write Latency:: " + statistics.LongestWriteLatency + "ms");
            Console.WriteLine("Average Write Latency :: " + statistics.AverageWriteLatency(0.90) + "ms");
        }
    }
}