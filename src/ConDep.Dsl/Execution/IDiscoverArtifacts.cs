using ConDep.Dsl.Config;

namespace ConDep.Dsl.Execution
{
    public interface IDiscoverArtifacts
    {
        IProvideArtifact GetArtifact(ConDepSettings settings);
    }
}