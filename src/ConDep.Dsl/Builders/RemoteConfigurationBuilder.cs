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
        private readonly List<IManageRemoteSequence> _remoteSequences;

        public RemoteConfigurationBuilder(IEnumerable<IManageRemoteSequence> remoteSequences)
        {
            _remoteSequences = remoteSequences.ToList();
        }

        public IOfferRemoteConfiguration OnlyIf(Predicate<ServerInfo> condition)
        {
            var sequences = _remoteSequences.Select(sequence => sequence.NewConditionalCompositeSequence(condition));
            return new RemoteConfigurationBuilder(sequences);
        }

        public void AddOperation(RemoteCompositeOperation operation)
        {
            var sequences = _remoteSequences.Select(sequence => sequence.NewCompositeSequence(operation));
            operation.Configure(new RemoteCompositeBuilder(sequences));
        }

        public void AddOperation(IExecuteOnServer operation)
        {
            foreach (var sequence in _remoteSequences)
            {
                sequence.Add(operation);
            }
        }
    }
}