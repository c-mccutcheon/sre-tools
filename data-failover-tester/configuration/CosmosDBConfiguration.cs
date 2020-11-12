using System.Security;

namespace Contino.SRE.Tools.CosmosDB.Configuration
{
    public class CosmosDBConfiguration
    {
        public string DatabaseServer { get; set; }
        public string DatabaseName { get; set; }
        public string DatabaseUser { get; set; }
        public string DatabasePassword { get; set; }
    }
}