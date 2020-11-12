using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Contino.SRE.Tools.CosmosDB.Configuration;
using Contino.SRE.Tools.Data.Clients.Mongo;
using Contino.SRE.Tools.Data.Failover.Statistics;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Azure.Cosmos;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace Contino.SRE.Tools.Data.Failover.Commands
{
    [Command("cosmosdb")]
    public class CosmosFailoverTestCommand
    {
        #region Command Parameters

        [Argument(0, name: "dbname", description: "The database name to generate for the test.")]
        public static string CommandDatabaseName { get; set; }

        [Argument(1, name: "dbcollectionname", description: "The collection name to generate for the test.")]
        public static string CommandDatabaseCollectionName { get; set; }

        [Argument(2, name: "dbserver", description: "The database server. Should be a URI and will use port 10255 by default.")]
        public static string CommandDatabaseServerAddress { get; set; }

        [Argument(3, name: "dbidentity", description: "The database server identity. Must have read/write access.")]
        public static string CommandDatabaseServerIdentity { get; set; }

        [Argument(4, name: "dbpassword", description: "The database server password. This should be the read/write access key.")]
        public static string CommandDatabaseServerPassword { get; set; }

        [Argument(5, name: "delete", description: "Deletes all data resources before the test run.")]
        public static bool CommandDeleteData { get; set; }

        [Argument(6, name: "timeout", description: "The time (in seconds) to run the test for. -1 will run indefinitely.")]
        public static int CommandTimeout { get; set; }

        #endregion

        #region Members

        private static MongoCosmosClient _cosmosClient;

        private readonly Lazy<IMongoDatabase> Database = new Lazy<IMongoDatabase>(() => 
        {
            return _cosmosClient.Client.GetDatabase(CommandDatabaseName);
        });

        private object _syncLock = new Object();

        private Func<TimeSpan> _defaultTimeout = () => {
            if(CommandTimeout == -1)
                return new TimeSpan(Int32.MaxValue);
            return new TimeSpan(0, 0, CommandTimeout);
        };

        private ConsoleColor _defaultColor = ConsoleColor.Green;

        private readonly Barrier _threadBarrier;

        private TestRunStatistics _testRunStatistics = new TestRunStatistics(new ConsoleReportGenerator());

        #endregion

        public CosmosFailoverTestCommand()
        {
            Console.ForegroundColor = _defaultColor;
            _threadBarrier = new Barrier(2, (barrier) =>
            {
                DeleteDatabaseCollectionAsync(CommandDatabaseCollectionName);
                ConsoleWrite(ConsoleColor.Red, "End of thread barrier or default timeout. Cleaned up resources.");
                _testRunStatistics.StopCollection();
                _testRunStatistics.OutputReport();
                Environment.Exit(0);
            });
        }

        private Task OnExecuteAsync(CommandLineApplication application)
        {
            var cosmosConfiguration = new CosmosDBConfiguration {
                DatabaseServer = CommandDatabaseServerAddress,
                DatabaseName = CommandDatabaseName,
                DatabaseUser = CommandDatabaseServerIdentity,
                DatabasePassword = CommandDatabaseServerPassword
            };

            _cosmosClient = new MongoCosmosClient(cosmosConfiguration);

            try
            {
                Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}]Beginning operations...\n");
                Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}]#### --- Settings --- ####");
                Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}]Timeout(s) :: {_defaultTimeout().TotalSeconds}");
                Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}]Delete data:: {CommandDeleteData}");
                
                _testRunStatistics.StartCollecting();
                new Thread(InsertDocuments).Start();
                new Thread(ConsumeDocuments).Start();
            }
            catch (CosmosException de)
            {
                Exception baseException = de.GetBaseException();
                ConsoleWrite(ConsoleColor.Red, $"{de.StatusCode} error occurred: {de}");
                return Task.FromResult(1);
            }
            catch (Exception e)
            {
                ConsoleWrite(ConsoleColor.Red, $"Error occurred: {e}");
                return Task.FromResult(1);
            }
            
            return Task.Delay(-1);
        }

        private void ConsoleWrite(ConsoleColor color, string message, bool resetColor = false)
        {
            lock(_syncLock)
            {
                Console.ForegroundColor = color;
                Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}]"+ message);
                if(resetColor)
                    Console.ForegroundColor = _defaultColor;
            }
        }

        private IMongoCollection<BsonDocument> GetDatabaseCollectionAsync(string collectionName)
        {
            var cosmosCollection = Database.Value.GetCollection<BsonDocument>(collectionName);
            ConsoleWrite(ConsoleColor.Blue, $"Created or Fetched Database: {CommandDatabaseName}/{collectionName}\n", true);
            return cosmosCollection;
        }

        private void DeleteDatabaseCollectionAsync(string collectionName)
        {
            Database.Value.DropCollectionAsync(collectionName);
            ConsoleWrite(ConsoleColor.Red, $"Dropped Database Collection: {collectionName}\n", true);
        }

        private async void InsertDocuments()
        {
            if(CommandDeleteData)
                DeleteDatabaseCollectionAsync(CommandDatabaseCollectionName);

            var cosmosCollection = GetDatabaseCollectionAsync(CommandDatabaseCollectionName);

            Stopwatch timer = new Stopwatch();
            timer.Start();

            while(!(timer.Elapsed > (_defaultTimeout() * 2))) 
            {
                try 
                {
                    var insertOperation = await new MongoDocumentInserter().InsertData(cosmosCollection);
                    _testRunStatistics.AddWriteOperation(insertOperation.ElapsedTime);
                    ConsoleWrite(ConsoleColor.Green, $"Data producer thread... :: session {insertOperation.Session.ToString()} inserted document count {insertOperation.Counter} - time elapsed {insertOperation.ElapsedTime.TotalMilliseconds}ms");
                    Thread.Sleep(200);
                }
                catch(MongoDocumentInsertException ie) 
                {
                    _testRunStatistics.AddError();
                    ConsoleWrite(ConsoleColor.Red, $"Data producer thread... :: exception on {ie.Session} insert {ie.SessionCounter} {ie.Message}");
                }
            }   

            _threadBarrier.SignalAndWait(-1);
        }

        private async void ConsumeDocuments()
        {
            var cosmosCollection = GetDatabaseCollectionAsync(CommandDatabaseCollectionName);
            Stopwatch timer = new Stopwatch();
            timer.Start();
            var consumeTime = (_defaultTimeout() * 2);
            while(!(timer.Elapsed > consumeTime)) 
            {
                var readStopwatch = new Stopwatch();
                readStopwatch.Start();
                var count = await cosmosCollection.CountDocumentsAsync(new BsonDocument());
                readStopwatch.Stop();
                _testRunStatistics.AddReadOperation(readStopwatch.Elapsed);

                ConsoleWrite(ConsoleColor.Magenta, $"Data consumer thread... :: read document count {count}", true);
                Thread.Sleep(500);
            }

            timer.Stop();
            _threadBarrier.SignalAndWait(-1);
        }
    }
}