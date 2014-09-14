using System;
using ConDep.Dsl.Config;
using ConDep.Dsl.Operations;
using ConDep.Dsl.Sequence;

namespace ConDep.Dsl.Builders
{
    internal class RemoteConfigurationBuilder : IOfferRemoteConfiguration, IConfigureInfrastructure
    {
        private readonly IManageRemoteSequence _remoteSequence;

        public RemoteConfigurationBuilder(IManageRemoteSequence remoteSequences)
        {
            _remoteSequence = remoteSequences;
        }

        public IOfferRemoteConfiguration OnlyIf(Predicate<ServerInfo> condition)
        {
            return new RemoteConfigurationBuilder(_remoteSequence.NewConditionalCompositeSequence(condition));
        }

        public void AddOperation(RemoteCompositeOperation operation)
        {
            operation.Configure(new RemoteCompositeBuilder(_remoteSequence.NewCompositeSequence(operation)));
        }

        public void AddOperation(IExecuteRemotely operation)
        {
            _remoteSequence.Add(operation);
        }
    }
}