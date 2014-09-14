using System;
using System.Linq;
using ConDep.Dsl.Builders;
using ConDep.Dsl.Config;
using ConDep.Dsl.Sequence;

namespace ConDep.Dsl.Execution
{
    internal class ArtifactHandler
    {
        public static void PopulateExecutionSequence(ConDepSettings conDepSettings, ExecutionSequenceManager sequenceManager)
        {
            CreateApplicationArtifacts(conDepSettings, sequenceManager);
        }

        private static void CreateApplicationArtifacts(ConDepSettings settings, ExecutionSequenceManager sequenceManager)
        {
            var assembly = settings.Options.Assembly;
            if (settings.Options.HasApplicationDefined())
            {
                var type = assembly.GetTypes().SingleOrDefault(t => typeof(IProvideArtifact).IsAssignableFrom(t) && t.Name == settings.Options.Application);
                if (type == null)
                {
                    throw new ConDepConfigurationTypeNotFoundException(string.Format("A class inheriting from [{0}] or [{1}] must be present in assembly [{2}] for ConDep to work. No calss with name [{3}] found in assembly. ", typeof(Artifact.Local).FullName, typeof(Artifact.Remote).FullName, assembly.FullName, settings.Options.Application));
                }
                CreateApplicationArtifact(type, settings, sequenceManager);
            }
            else
            {
                var types = assembly.GetTypes().Where(t => typeof(IProvideArtifact).IsAssignableFrom(t));
                foreach (var type in types)
                {
                    CreateApplicationArtifact(type, settings, sequenceManager);
                }
            }
        }

        private static void CreateApplicationArtifact(Type type, ConDepSettings settings, ExecutionSequenceManager sequenceManager)
        {
            var application = Activator.CreateInstance(type) as IProvideArtifact;
            if (application == null) throw new NullReferenceException(string.Format("Instance of application class [{0}] not found.", type.FullName));

            PopulateDependencies(settings, application, sequenceManager);
            ConfigureArtifact(settings, sequenceManager, application);
        }

        private static void PopulateDependencies(ConDepSettings conDepSettings, IProvideArtifact application, ExecutionSequenceManager sequenceManager)
        {
            var dependencyHandler = new ArtifactDependencyHandler(application);
            if (dependencyHandler.HasDependenciesDefined())
            {
                var dependencies = dependencyHandler.GetDependeciesForArtifact(conDepSettings);
                foreach (var dependency in dependencies)
                {
                    ConfigureArtifact(conDepSettings, sequenceManager, dependency);
                }
            }
        }

        private static void ConfigureArtifact(ConDepSettings conDepSettings, ExecutionSequenceManager sequenceManager, IProvideArtifact dependency)
        {
            if (dependency is Artifact.Local)
            {
                var localSequence = sequenceManager.NewLocalSequence(dependency.GetType().Name);
                var localBuilder = new LocalOperationsBuilder(localSequence);
                ((Artifact.Local)dependency).Configure(localBuilder, conDepSettings);
            }
            else if (dependency is Artifact.Remote)
            {
                var remoteSequence = sequenceManager.NewRemoteSequence(dependency.GetType().Name);
                var remoteBuilder = new RemoteOperationsBuilder(remoteSequence);
                ((Artifact.Remote)dependency).Configure(remoteBuilder, conDepSettings);
            }
        }
    }
}