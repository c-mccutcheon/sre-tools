using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contino.SRE.Tools.Data.Clients.Mongo;
using Contino.SRE.Tools.Data.Failover.Commands;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace Contino.SRE.Tools.Data.Failover
{
    class Program
    {
        
        private static object _syncLock = new object();
        static int Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();
                
            var app = new CommandLineApplication<DataFailoverTestCommand>();

            app.Conventions
                .UseDefaultConventions();

            return app.Execute(args);
        }
        
    }
}
