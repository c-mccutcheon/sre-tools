using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Contino.SRE.Tools.Data.Failover.Commands
{
    [Subcommand(typeof(CosmosFailoverTestCommand), (typeof(SqlFailoverTestCommand)))]
    public class DataFailoverTestCommand: IDisposable
    {

        public DataFailoverTestCommand()
        {
        }

        private Task<int> OnExecuteAsync(CommandLineApplication application)
        {
            application.ShowHelp();
            return Task.FromResult(0);
        }

        public void Dispose()
        {
        }
    }
}
