using System;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Sequence;

namespace ConDep.Dsl.Builders
{
    public class RemoteConfigurationBuilder : RemoteBuilder, IOfferRemoteConfiguration
    {
        public RemoteConfigurationBuilder(IOfferRemoteOperations dsl, ServerConfig server, ConDepSettings settings, CancellationToken token) : base(server, settings, token)
        {
            Dsl = dsl;
        }

        public override IOfferRemoteOperations Dsl { get; }
    }
}