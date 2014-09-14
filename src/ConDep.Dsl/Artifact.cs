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
        /// 
        /// <example>
        /// <code>
        /// public class TransformConfigForMyApplication : Artifact.Local
        /// {
        ///     public override void Configure(IOfferLocalOperations onLocalMachine, ConDepSettings settings)
        ///     {
        ///         //Your implementation
        ///     }
        /// }
        /// </code>
        /// </example>
        /// </summary>
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