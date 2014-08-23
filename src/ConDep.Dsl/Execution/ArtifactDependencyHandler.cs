using System.Collections.Generic;
using System.Linq;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Execution
{
    public class ArtifactDependencyHandler
    {
        private readonly IProvideArtifact _artifact;

        public ArtifactDependencyHandler(IProvideArtifact artifact)
        {
            _artifact = artifact;
        }

        public bool HasDependenciesDefined()
        {
            var typeName = typeof(IDependOn<>).Name;
            var interfaces = _artifact.GetType().GetInterfaces();
            return interfaces.Any(x => x.Name == typeName);
        }

        public IEnumerable<IProvideArtifact> GetDependeciesForArtifact(ConDepSettings settings)
        {
            var typeName = typeof(IDependOn<>).Name;
            var typeInterfaces = _artifact.GetType().GetInterfaces();

            var dependencies = typeInterfaces.Where(x => x.Name == typeName);
            var dependencyInstances = new List<IProvideArtifact>();

            foreach (var infraInterface in dependencies)
            {
                var dependencyType = infraInterface.GetGenericArguments().Single();

                var dependencyInstance = settings.Options.Assembly.CreateInstance(dependencyType.FullName) as IProvideArtifact;

                dependencyInstances.AddRange(new ArtifactDependencyHandler(dependencyInstance).GetDependeciesForArtifact(settings));
                dependencyInstances.Add(dependencyInstance);
            }
            return dependencyInstances;
        }
    }

}