using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Contino.SRE.Tools.Data.Clients.Mongo;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Azure.Cosmos;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace Contino.SRE.Tools.Data.Failover.Commands
{
    [Command("sql")]
    public class SqlFailoverTestCommand
    {
        [Argument(0, name: "dbname", description: "The database name to generate for the test.")]
        public static string DatabaseName { get; set; }

        [Argument(1, name: "dbtablename", description: "The table name to generate for the test.")]
        public static string DatabaseCollectionName { get; set; }

        [Argument(2, name: "delete", description: "Deletes all data resources after the test run.")]
        public static bool DeleteData { get; set; }

        private object _syncLock = new Object();

        public DataFailoverTestCommand Parent { get; set; }

        private TimeSpan _defaultTimeout = new TimeSpan(0, 0, 30);

        private readonly Barrier _threadBarrier;

        public SqlFailoverTestCommand()
        {
            _threadBarrier = new Barrier(2, (barrier) =>
            {
                /* Write out final statistics to a file or console. */
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Beep();
                Console.WriteLine("Hit end of thread barrier or default timeout.");
                Environment.Exit(0);
            });
        }

        private Task OnExecuteAsync(CommandLineApplication application)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Sql provider is not yet supported.");
            /* Return a never ending task as we wish to wait and let thread barriers takeover */
            return Task.FromResult(1);
        }

    }
}