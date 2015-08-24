using ConDep.Dsl.Config;

namespace ConDep.Dsl.Harvesters
{
    internal interface IHarvestServerInfo
    {
        void Harvest(ServerConfig server);
    }
}