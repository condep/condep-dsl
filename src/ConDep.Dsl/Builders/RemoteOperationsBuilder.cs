using System;
using ConDep.Dsl.Config;
using ConDep.Dsl.Sequence;

namespace ConDep.Dsl.Builders
{
    public class RemoteOperationsBuilderNew : IOfferRemoteOperations
    {
        private readonly ServerConfig _server;

        public RemoteOperationsBuilderNew(ServerConfig server)
        {
            _server = server;
        }

        public IOfferRemoteOperations OnlyIf(Predicate<ServerInfo> condition)
        {
            throw new NotImplementedException();
        }

        public IOfferRemoteOperations OnlyIf(string conditionScript)
        {
            throw new NotImplementedException();
        }

        public IOfferRemoteDeployment Deploy { get; }
        public IOfferRemoteExecution Execute { get; }
        public IOfferRemoteConfiguration Configure { get; }
        public IOfferRemoteInstallation Install { get; }

        public Result Result { get; set; }
    }

    public class RemoteOperationsBuilder : IOfferRemoteOperations
    {
        private readonly IOfferRemoteSequence _remoteSequence;

        public RemoteOperationsBuilder(IOfferRemoteSequence remoteSequence)
        {
            _remoteSequence = remoteSequence;
        }

        public IOfferRemoteDeployment Deploy { get { return new RemoteDeploymentBuilder(_remoteSequence); } }
        public IOfferRemoteExecution Execute { get { return new RemoteExecutionBuilder(_remoteSequence); } }
        public IOfferRemoteConfiguration Configure { get { return new RemoteConfigurationBuilder(_remoteSequence);  } }
        public IOfferRemoteInstallation Install { get { return new RemoteInstallationBuilder(_remoteSequence); } }

        public IOfferRemoteOperations OnlyIf(Predicate<ServerInfo> condition)
        {
            return new RemoteCompositeBuilder(_remoteSequence.NewConditionalCompositeSequence(condition));
        }

        public IOfferRemoteOperations OnlyIf(string conditionScript)
        {
            return new RemoteCompositeBuilder(_remoteSequence.NewConditionalCompositeSequence(conditionScript));            
        }

        public void AddOperation(IExecuteRemotely operation)
        {
            _remoteSequence.Add(operation);
        }
    }
}