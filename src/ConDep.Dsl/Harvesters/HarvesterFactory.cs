using ConDep.Dsl.Config;

namespace ConDep.Dsl.Harvesters
{
    public class HarvesterFactory
    {
        public static ServerInfoHarvester GetHarvester(ConDepSettings settings)
        {
            return new ServerInfoHarvester(settings, new[] { typeof(OperatingSystemHarvester), typeof(DotNetFrameworkHarvester) });
        } 
    }
}