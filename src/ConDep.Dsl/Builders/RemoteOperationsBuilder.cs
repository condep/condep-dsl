using System.Threading;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Builders
{
    public class RemoteOperationsBuilder : RemoteBuilder, IOfferRemoteOperations
    {
        public IOfferRemoteDeployment Deploy { get; private set; }
        public IOfferRemoteExecution Execute { get; private set; }
        public IOfferRemoteConfiguration Configure { get; private set; }
        public IOfferRemoteInstallation Install { get; private set; }

        public RemoteOperationsBuilder(ServerConfig server, ConDepSettings settings, CancellationToken token) : base(server, settings, token)
        {
            Deploy = new RemoteDeploymentBuilder(this, server, settings, token);
            Execute = new RemoteExecutionBuilder(this, server, settings, token);
            Configure = new RemoteConfigurationBuilder(this, server, settings, token);
            Install = new RemoteInstallationBuilder(this, server, settings, token);
        }

        public override IOfferRemoteOperations Dsl => this;
    }
}