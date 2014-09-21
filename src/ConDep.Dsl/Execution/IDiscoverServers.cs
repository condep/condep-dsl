using System.Collections.Generic;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Execution
{
    public interface IDiscoverServers
    {
        IEnumerable<ServerConfig> GetServers(IProvideArtifact application, ConDepSettings settings);
    }
}