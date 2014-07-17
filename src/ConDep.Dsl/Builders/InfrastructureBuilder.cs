using System;
using ConDep.Dsl.Config;
using ConDep.Dsl.Operations;
using ConDep.Dsl.SemanticModel;

namespace ConDep.Dsl.Builders
{
    public class InfrastructureBuilder : IOfferInfrastructure, IConfigureInfrastructure
    {
        private readonly IManageRemoteSequence _remoteSequence;

        public InfrastructureBuilder(IManageRemoteSequence remoteSequence)
        {
            _remoteSequence = remoteSequence;
        }

        public IOfferInfrastructure OnlyIf(Predicate<ServerInfo> condition)
        {
            return new InfrastructureBuilder(_remoteSequence.NewConditionalCompositeSequence(condition));
        }

        public void AddOperation(RemoteCompositeOperation operation)
        {
            operation.Configure(new RemoteCompositeBuilder(_remoteSequence.NewCompositeSequence(operation)));
        }
    }
}