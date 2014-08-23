using ConDep.Dsl.Config;

namespace ConDep.Dsl
{
    public class Artifact
    {
        public abstract class Local : IProvideArtifact
        {
            public abstract void Configure(IOfferLocalOperations onLocalMachine, ConDepSettings settings);
        }

        public abstract class Remote : IProvideArtifact
        {
            public abstract void Configure(IOfferRemoteOperations server, ConDepSettings settings);
        }
    }
}