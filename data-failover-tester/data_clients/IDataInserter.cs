using System.Security.Authentication;
using System.Threading.Tasks;
using Contino.SRE.Tools.CosmosDB.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Contino.SRE.Tools.Data
{
    public interface IDataInserter
    {
        Task<InsertOperationStatistic> InsertData(IMongoCollection<BsonDocument> collection);
    }
}