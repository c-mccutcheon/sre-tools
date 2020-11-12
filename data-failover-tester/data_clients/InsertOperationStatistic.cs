using System;

namespace Contino.SRE.Tools.Data
{
    public struct InsertOperationStatistic
    {
        public Guid Session {get; set;}

        public int Counter {get; set;}

        public TimeSpan ElapsedTime {get; set;}
    }
}