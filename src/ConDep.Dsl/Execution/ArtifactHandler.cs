using System;
using System.Linq;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Execution
{
    internal class ArtifactHandler : IDiscoverArtifacts
    {
        public IProvideArtifact GetArtifact(ConDepSettings settings)
        {
            if (!settings.Options.HasApplicationDefined()) throw new ConDepNoArtifactDefinedException();

            var assembly = settings.Options.Assembly;

            var type = assembly.GetTypes().SingleOrDefault(t => typeof(IProvideArtifact).IsAssignableFrom(t) && t.Name == settings.Options.Application);
            if (type == null)
            {
                throw new ConDepConfigurationTypeNotFoundException(string.Format("A class inheriting from [{0}] or [{1}] must be present in assembly [{2}] for ConDep to work. No calss with name [{3}] found in assembly. ", typeof(Artifact.Local).FullName, typeof(Artifact.Remote).FullName, assembly.FullName, settings.Options.Application));
            }
            return CreateApplicationArtifact(type);
        }

        private static IProvideArtifact CreateApplicationArtifact(Type type)
        {
            var application = Activator.CreateInstance(type) as IProvideArtifact;
            if (application == null) throw new NullReferenceException(string.Format("Instance of application class [{0}] not found.", type.FullName));

            return application;
        }
    }
}