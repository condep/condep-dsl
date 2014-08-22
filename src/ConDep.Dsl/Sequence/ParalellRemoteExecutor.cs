using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;

namespace ConDep.Dsl.Sequence
{
    public class ParalellRemoteExecutor : LoadBalancerExecutorBase
    {
        private readonly IEnumerable<ServerConfig> _servers;

        public ParalellRemoteExecutor(List<IExecuteOnServer> sequence, IEnumerable<ServerConfig> servers)
            : base(sequence)
        {
            _servers = servers;
        }

        public override void Execute(IReportStatus status, ConDepSettings settings, CancellationToken token)
        {
            Logger
                .WithLogSection("Paralell Remote Operations (weard logging will occour!)", 
                () => Parallel.ForEach(_servers, server => 
                    ExecuteOnServer(server, status, settings, token)
                )
            );
        }
    }
}