using System;
using System.Diagnostics;

namespace Contino.SRE.Tools.Data.Failover.Statistics
{
    public interface IReportGenerator 
    {
        void Generate(TestRunStatistics statistics);
    }
}