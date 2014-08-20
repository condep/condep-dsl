using System;
using ConDep.Dsl.Config;
using ConDep.Dsl.Operations;
using ConDep.Dsl.Sequence;

namespace ConDep.Dsl.Builders
{
    public class RemoteOperationsBuilder : IOfferRemoteOperations
    {
        private readonly RemoteSequence _remoteSequence;

        public RemoteOperationsBuilder(RemoteSequence remoteSequence)
        {
            _remoteSequence = remoteSequence;
        }

        public IOfferRemoteDeployment Deploy { get { return new RemoteDeploymentBuilder(_remoteSequence); } }
        public IOfferRemoteExecution ExecuteRemote { get { return new RemoteExecutionBuilder(_remoteSequence); } }
        public IOfferInfrastructure Require { get { return new InfrastructureBuilder(_remoteSequence);  } }
        public IOfferRemoteInstallation Install { get { return new RemoteInstallationBuilder(_remoteSequence); } }

        public IOfferRemoteComposition OnlyIf(Predicate<ServerInfo> condition)
        {
            return new RemoteCompositeBuilder(_remoteSequence.NewConditionalCompositeSequence(condition));
        }

        public void AddOperation(IOperateRemote operation)
        {
            _remoteSequence.Add(operation);
        }

        public void AddOperation(RealRemoteOperation operation)
        {
            _remoteSequence.Add(operation);
        }

    }
}