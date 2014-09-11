using System;
using System.Collections.Generic;
using System.Linq;
using ConDep.Dsl.Config;
using ConDep.Dsl.Sequence;

namespace ConDep.Dsl.Builders
{
    public class RemoteCompositeBuilder : IOfferRemoteComposition
    {
        private readonly List<CompositeSequence> _compositeSequences;

        public RemoteCompositeBuilder(IEnumerable<CompositeSequence> compositeSequences)
        {
            _compositeSequences = compositeSequences.ToList();
            Deploy = new RemoteDeploymentBuilder(_compositeSequences);
            Execute = new RemoteExecutionBuilder(_compositeSequences);
            Configure = new RemoteConfigurationBuilder(_compositeSequences);
            Install = new RemoteInstallationBuilder(_compositeSequences);
        }

        public IOfferRemoteDeployment Deploy { get; private set; }
        public IOfferRemoteExecution Execute { get; private set; }
        public IOfferRemoteConfiguration Configure { get; private set; }
        public IOfferRemoteInstallation Install { get; private set; }

        public IOfferRemoteComposition OnlyIf(Predicate<ServerInfo> condition)
        {
            var sequences = _compositeSequences.Select(sequence => sequence.NewConditionalCompositeSequence(condition));
            return new RemoteCompositeBuilder(sequences);
        }
    }
}