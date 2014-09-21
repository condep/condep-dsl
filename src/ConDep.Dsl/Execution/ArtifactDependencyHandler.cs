using System.Collections.Generic;
using System.Linq;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Execution
{
    internal class ArtifactDependencyHandler : IResolveArtifactDependencies
    {
        public bool HasDependenciesDefined(IProvideArtifact artifact)
        {
            var typeName = typeof(IDependOn<>).Name;
            var interfaces = artifact.GetType().GetInterfaces();
            return interfaces.Any(x => x.Name == typeName);
        }

        public void PopulateWithDependencies(IProvideArtifact artifact, ConDepSettings settings)
        {
            if (!HasDependenciesDefined(artifact)) return;

            artifact.Dependencies = GetDependeciesForArtifact(artifact, settings);
        }

        private IEnumerable<IProvideArtifact> GetDependeciesForArtifact(IProvideArtifact artifact, ConDepSettings settings)
        {
            var typeName = typeof(IDependOn<>).Name;
            var typeInterfaces = artifact.GetType().GetInterfaces();

            var dependencies = typeInterfaces.Where(x => x.Name == typeName);
            var dependencyInstances = new List<IProvideArtifact>();

            foreach (var infraInterface in dependencies)
            {
                var dependencyType = infraInterface.GetGenericArguments().Single();

                var dependencyInstance = settings.Options.Assembly.CreateInstance(dependencyType.FullName) as IProvideArtifact;

                dependencyInstances.AddRange(new ArtifactDependencyHandler().GetDependeciesForArtifact(dependencyInstance, settings));
                dependencyInstances.Add(dependencyInstance);
            }
            return dependencyInstances;
        }
    }
}