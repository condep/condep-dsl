using System.Threading;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Builders
{
    public class RemoteExecutionBuilder : RemoteBuilder, IOfferRemoteExecution
    {
        public RemoteExecutionBuilder(ServerConfig server, ConDepSettings settings, CancellationToken token) : base(server, settings, token)
        {
        }
    }
}