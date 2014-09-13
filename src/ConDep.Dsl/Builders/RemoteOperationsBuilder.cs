using System;
using System.Collections.Generic;
using System.Linq;
using ConDep.Dsl.Config;
using ConDep.Dsl.SemanticModel;
using ConDep.Dsl.Sequence;

namespace ConDep.Dsl.Builders
{
    public class RemoteOperationsBuilder : IOfferRemoteOperations
    {
        private readonly IManageRemoteSequence _remoteSequence;

        public RemoteOperationsBuilder(IManageRemoteSequence remoteSequence)
        {
            _remoteSequence = remoteSequence;
        }

        public IOfferRemoteDeployment Deploy { get { return new RemoteDeploymentBuilder(_remoteSequence); } }
        public IOfferRemoteExecution Execute { get { return new RemoteExecutionBuilder(_remoteSequence); } }
        public IOfferRemoteConfiguration Configure { get { return new RemoteConfigurationBuilder(_remoteSequence);  } }
        public IOfferRemoteInstallation Install { get { return new RemoteInstallationBuilder(_remoteSequence); } }

        public IOfferRemoteComposition OnlyIf(Predicate<ServerInfo> condition)
        {
            return new RemoteCompositeBuilder(_remoteSequence.NewConditionalCompositeSequence(condition));
        }

        public void AddOperation(IExecuteRemotely operation)
        {
            _remoteSequence.Add(operation);
        }
    }
}