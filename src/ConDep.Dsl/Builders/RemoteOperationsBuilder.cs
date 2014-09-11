using System;
using System.Collections.Generic;
using System.Linq;
using ConDep.Dsl.Config;
using ConDep.Dsl.SemanticModel;

namespace ConDep.Dsl.Builders
{
    public class RemoteOperationsBuilder : IOfferRemoteOperations
    {
        private readonly IEnumerable<IManageRemoteSequence> _remoteSequences;

        public RemoteOperationsBuilder(IEnumerable<IManageRemoteSequence> remoteSequences)
        {
            _remoteSequences = remoteSequences;
        }

        public IOfferRemoteDeployment Deploy { get { return new RemoteDeploymentBuilder(_remoteSequences); } }
        public IOfferRemoteExecution Execute { get { return new RemoteExecutionBuilder(_remoteSequences); } }
        public IOfferRemoteConfiguration Configure { get { return new RemoteConfigurationBuilder(_remoteSequences);  } }
        public IOfferRemoteInstallation Install { get { return new RemoteInstallationBuilder(_remoteSequences); } }

        public IOfferRemoteComposition OnlyIf(Predicate<ServerInfo> condition)
        {
            var sequences = _remoteSequences.Select(sequence => sequence.NewConditionalCompositeSequence(condition));
            return new RemoteCompositeBuilder(sequences);
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