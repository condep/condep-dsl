using System;
using ConDep.Dsl.Config;

namespace ConDep.Dsl
{
    /// <summary>
    /// Inherit this class to configure deployment for your application
    /// </summary>
    [Obsolete("ApplicationArtifact is deprecated. Use Artifact.Local or Artifact.Remote instead.")]
    public abstract class ApplicationArtifact : IProvideArtifact
    {
        public abstract void Configure(IOfferLocalOperations onLocalMachine, ConDepSettings settings);
    }

    public interface IProvideArtifact
    {
        
    }

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