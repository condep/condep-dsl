using ConDep.Dsl.Builders;
using ConDep.Dsl.Config;
using ConDep.Dsl.Sequence;

namespace ConDep.Dsl.Execution
{
    internal class ArtifactConfigurationHandler
    {
        private readonly IDiscoverArtifacts _artifactHandler;
        private readonly IResolveArtifactDependencies _artifactDependencyHandler;
        private readonly IDiscoverServers _serverHandler;
        private readonly ILoadBalance _loadBalancer;

        public ArtifactConfigurationHandler(IDiscoverArtifacts artifactHandler, IResolveArtifactDependencies artifactDependencyHandler, IDiscoverServers serverHandler, ILoadBalance loadBalancer)
        {
            _artifactHandler = artifactHandler;
            _artifactDependencyHandler = artifactDependencyHandler;
            _serverHandler = serverHandler;
            _loadBalancer = loadBalancer;
        }

        public ExecutionSequenceManager CreateExecutionSequence(ConDepSettings settings)
        {
            var artifact = _artifactHandler.GetArtifact(settings);
            _artifactDependencyHandler.PopulateWithDependencies(artifact, settings);

            var servers = _serverHandler.GetServers(artifact, settings);
            var sequenceManager = new ExecutionSequenceManager(servers, _loadBalancer);

            if (artifact.Dependencies != null)
            {
                foreach (var dependency in artifact.Dependencies)
                {
                    ConfigureAtrifact(dependency, sequenceManager, settings);
                }
            }

            ConfigureAtrifact(artifact, sequenceManager, settings);
            return sequenceManager;
        }

        private void ConfigureAtrifact(IProvideArtifact artifact, ExecutionSequenceManager sequenceManager, ConDepSettings settings)
        {
            if (artifact is Artifact.Local)
            {
                var localSequence = sequenceManager.NewLocalSequence(artifact.GetType().Name);
                var localBuilder = new LocalOperationsBuilder(localSequence);
                ((Artifact.Local)artifact).Configure(localBuilder, settings);
            }
            else if (artifact is Artifact.Remote)
            {
                var remoteSequence = sequenceManager.NewRemoteSequence(artifact.GetType().Name);
                var remoteBuilder = new RemoteOperationsBuilder(remoteSequence);
                ((Artifact.Remote)artifact).Configure(remoteBuilder, settings);
            }
        }
    }
}