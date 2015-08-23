using System;
using ConDep.Dsl.Config;
using ConDep.Dsl.Sequence;

namespace ConDep.Dsl.Builders
{
    internal class RemoteDeploymentBuilder : IOfferRemoteDeployment, IConfigureRemoteDeployment
    {
        private readonly IOfferRemoteSequence _remoteSequence;

        public RemoteDeploymentBuilder(IOfferRemoteSequence remoteSequence)
        {
            _remoteSequence = remoteSequence;
        }

        public void AddOperation(IExecuteRemotely operation)
        {
            _remoteSequence.Add(operation);
        }

        public void AddOperation(RemoteCompositeOperation operation)
        {
            operation.Configure(new RemoteCompositeBuilder(_remoteSequence.NewCompositeSequence(operation)));
        }

        public IOfferRemoteDeployment OnlyIf(Predicate<ServerInfo> condition)
        {
            return new RemoteDeploymentBuilder(_remoteSequence.NewConditionalCompositeSequence(condition));
        }

        public IOfferRemoteDeployment OnlyIf(string conditionScript)
        {
            return new RemoteDeploymentBuilder(_remoteSequence.NewConditionalCompositeSequence(conditionScript));
        }
    }
}