using ConDep.Dsl.Config;

namespace ConDep.Dsl.Execution
{
    public interface IResolveArtifactDependencies
    {
        void PopulateWithDependencies(IProvideArtifact artifact, ConDepSettings settings);
    }
}