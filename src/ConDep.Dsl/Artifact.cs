using System.Collections.Generic;
using ConDep.Dsl.Config;

namespace ConDep.Dsl
{
    /// <summary>
    /// Container class for <see cref="Artifact.Local"/> and <see cref="Artifact.Remote"/>.
    /// </summary>
    public class Artifact
    {
        /// <summary>
        /// Use this Artifact to get access to ConDep's Local Operations DSL and perform local operations. 
        /// If you need access to the remote DSL, use the ToEachServer() method.
        /// </summary>
        public abstract class Local : IProvideArtifact
        {
            public abstract void Configure(IOfferLocalOperations onLocalMachine, ConDepSettings settings);
            public IEnumerable<IProvideArtifact> Dependencies { get; set; }
        }

        /// <summary>
        /// Use this Artifact to get access to ConDep's Remote Operations DSL and perform remote operations. 
        /// If you need access to the local DSL, use the <see cref="Artifact.Local"/> class instead. This 
        /// will give you access to both the local and remote DSL.
        /// </summary>
        public abstract class Remote : IProvideArtifact
        {
            public abstract void Configure(IOfferRemoteOperations server, ConDepSettings settings);
            public IEnumerable<IProvideArtifact> Dependencies { get; set; }
        }
    }
}