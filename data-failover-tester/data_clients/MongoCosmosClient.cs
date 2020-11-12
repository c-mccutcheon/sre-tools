using System.Security.Authentication;
using Contino.SRE.Tools.CosmosDB.Configuration;
using MongoDB.Driver;

namespace Contino.SRE.Tools.Data.Clients.Mongo
{
    public sealed class MongoCosmosClient
    {
        private MongoClient _mongoClient;
        public MongoClient Client => _mongoClient;

        public MongoCosmosClient(CosmosDBConfiguration config)
        {
            MongoClientSettings settings = new MongoClientSettings();
            settings.Server = new MongoServerAddress(config.DatabaseServer, 10255);
            settings.UseTls = true;
            settings.SslSettings = new SslSettings();
            settings.SslSettings.EnabledSslProtocols = SslProtocols.Tls12;
            settings.RetryWrites = false;

            MongoIdentity identity = new MongoInternalIdentity(config.DatabaseName, config.DatabaseUser);
            MongoIdentityEvidence evidence = new PasswordEvidence(config.DatabasePassword);

            settings.Credential = new MongoCredential("SCRAM-SHA-1", identity, evidence);

            _mongoClient = new MongoClient(settings);
        }
    }
}