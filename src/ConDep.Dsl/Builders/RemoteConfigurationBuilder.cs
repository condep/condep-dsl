using System;
using System.Collections.Generic;
using System.Linq;
using ConDep.Dsl.Config;
using ConDep.Dsl.Operations;
using ConDep.Dsl.SemanticModel;

namespace ConDep.Dsl.Builders
{
    public class RemoteConfigurationBuilder : IOfferRemoteConfiguration, IConfigureInfrastructure
    {
        private readonly IManageRemoteSequence _remoteSequence;

        public RemoteConfigurationBuilder(IManageRemoteSequence remoteSequences)
        {
            _remoteSequence = remoteSequences;
        }

        public IOfferRemoteConfiguration OnlyIf(Predicate<ServerInfo> condition)
        {
            return new RemoteConfigurationBuilder(_remoteSequence);
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