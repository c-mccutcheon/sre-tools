using System;
using System.Diagnostics;
using System.Security.Authentication;
using System.Threading.Tasks;
using Contino.SRE.Tools.CosmosDB.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace Contino.SRE.Tools.Data
{
    public sealed class MongoDocumentInserter : IDataInserter
    {
        private Guid _session = Guid.NewGuid();

        private int _sessionCount = 1;

        public async Task<InsertOperationStatistic> InsertData(IMongoCollection<BsonDocument> collection)
        {
            var document = new BsonDocument
                {
                    { "id", CombGuidGenerator.Instance.NewCombGuid(Guid.NewGuid(), DateTime.UtcNow) },
                    { "name", "Audit Entry" },
                    { "type", "Audit Object" },
                    { "session_count", _sessionCount },
                    { "info", new BsonDocument
                        {
                            { "generating_threadId", Environment.CurrentManagedThreadId },
                            { "generated_dateTime", DateTime.UtcNow.ToString() }
                        }}
                };

                var insertStopwatch = new Stopwatch();
                try 
                {
                    insertStopwatch.Start();
                    await collection.InsertOneAsync(document);
                    insertStopwatch.Stop();
                }
                catch (Exception)
                {
                    throw new MongoDocumentInsertException(this._session, this._sessionCount);
                }
                
            return new InsertOperationStatistic { Counter = _sessionCount, ElapsedTime = insertStopwatch.Elapsed, Session = _session };
        }
    }

    public class MongoDocumentInsertException : Exception
    {
        public Guid Session { get; private set; }

        public int SessionCounter {get; private set;}
        public MongoDocumentInsertException(Guid session, int sessionCounter) : base()
        {
            this.Session = session;
            this.SessionCounter = sessionCounter;
        }
    }
}