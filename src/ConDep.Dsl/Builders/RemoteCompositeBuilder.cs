using System;
using System.Collections.Generic;
using System.Linq;
using ConDep.Dsl.Config;
using ConDep.Dsl.Sequence;

namespace ConDep.Dsl.Builders
{
    public class RemoteCompositeBuilder : IOfferRemoteComposition
    {
        private readonly CompositeSequence _compositeSequence;

        public RemoteCompositeBuilder(CompositeSequence compositeSequence)
        {
            _compositeSequence = compositeSequence;
            Deploy = new RemoteDeploymentBuilder(_compositeSequence);
            Execute = new RemoteExecutionBuilder(_compositeSequence);
            Configure = new RemoteConfigurationBuilder(_compositeSequence);
            Install = new RemoteInstallationBuilder(_compositeSequence);
        }

        public IOfferRemoteDeployment Deploy { get; private set; }
        public IOfferRemoteExecution Execute { get; private set; }
        public IOfferRemoteConfiguration Configure { get; private set; }
        public IOfferRemoteInstallation Install { get; private set; }

        public IOfferRemoteComposition OnlyIf(Predicate<ServerInfo> condition)
        {
            return new RemoteCompositeBuilder(_compositeSequence.NewConditionalCompositeSequence(condition));
        }
    }
}