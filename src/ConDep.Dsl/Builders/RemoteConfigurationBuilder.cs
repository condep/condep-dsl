using System;
using ConDep.Dsl.Config;
using ConDep.Dsl.Operations;
using ConDep.Dsl.SemanticModel;

namespace ConDep.Dsl.Builders
{
    public class RemoteConfigurationBuilder : IOfferRemoteConfiguration, IConfigureInfrastructure
    {
        private readonly IManageRemoteSequence _remoteSequence;

        public RemoteConfigurationBuilder(IManageRemoteSequence remoteSequence)
        {
            _remoteSequence = remoteSequence;
        }

        public IOfferRemoteConfiguration OnlyIf(Predicate<ServerInfo> condition)
        {
            return new RemoteConfigurationBuilder(_remoteSequence.NewConditionalCompositeSequence(condition));
        }

        public void AddOperation(RemoteCompositeOperation operation)
        {
            operation.Configure(new RemoteCompositeBuilder(_remoteSequence.NewCompositeSequence(operation)));
        }

        public void AddOperation(IExecuteOnServer operation)
        {
            _remoteSequence.Add(operation);
        }
    }
}