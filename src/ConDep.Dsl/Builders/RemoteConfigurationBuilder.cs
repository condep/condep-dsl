using System.Threading;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Builders
{
    public class RemoteConfigurationBuilder : RemoteBuilder, IOfferRemoteConfiguration
    {
        public RemoteConfigurationBuilder(ServerConfig server, ConDepSettings settings, CancellationToken token) : base(server, settings, token)
        {
        }
    }
}