using System.Collections.Generic;
using System.Linq;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Execution
{
    public class ApplicationDependencyHandler
    {
        private readonly ApplicationArtifact _artifact;

        public ApplicationDependencyHandler(ApplicationArtifact artifact)
        {
            _artifact = artifact;
        }

        public bool HasDependenciesDefined()
        {
            var typeName = typeof(IDependOn<>).Name;
            var interfaces = _artifact.GetType().GetInterfaces();
            return interfaces.Any(x => x.Name == typeName);
        }

        public IEnumerable<ApplicationArtifact> GetDependeciesForApplication(ConDepSettings settings)
        {
            var typeName = typeof(IDependOn<>).Name;
            var typeInterfaces = _artifact.GetType().GetInterfaces();

            var dependencies = typeInterfaces.Where(x => x.Name == typeName);
            var dependencyInstances = new List<ApplicationArtifact>();

            foreach (var infraInterface in dependencies)
            {
                var dependencyType = infraInterface.GetGenericArguments().Single();

                var dependencyInstance = settings.Options.Assembly.CreateInstance(dependencyType.FullName) as ApplicationArtifact;

                dependencyInstances.AddRange(new ApplicationDependencyHandler(dependencyInstance).GetDependeciesForApplication(settings));
                dependencyInstances.Add(dependencyInstance);
            }
            return dependencyInstances;
        }
    }
}