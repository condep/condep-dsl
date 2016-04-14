using System.Threading;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Builders
{
    public class RemoteOperationsBuilder : RemoteBuilder, IOfferRemoteOperations
    {
        public RemoteOperationsBuilder(ServerConfig server, ConDepSettings settings, CancellationToken token) : base(server, settings, token)
        {
            Deploy = new RemoteDeploymentBuilder(server, settings, token);
            Execute = new RemoteExecutionBuilder(server, settings, token);
            Configure = new RemoteConfigurationBuilder(server, settings, token);
            Install = new RemoteInstallationBuilder(server, settings, token);
        }

        public IOfferRemoteDeployment Deploy { get; }
        public IOfferRemoteExecution Execute { get; }
        public IOfferRemoteConfiguration Configure { get; }
        public IOfferRemoteInstallation Install { get; }
    }
}