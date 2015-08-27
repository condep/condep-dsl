namespace ConDep.Dsl
{
    public interface IOfferRemoteOperations : IOfferOnlyIf<IOfferRemoteOperations>
    {
        /// <summary>
        /// Provide operations for remote deployment.
        /// </summary>
        IOfferRemoteDeployment Deploy { get; }

        /// <summary>
        /// Provide operations for remote execution.
        /// </summary>
        IOfferRemoteExecution Execute { get; }

        /// <summary>
        /// Provide operations for adding remote infrastructure
        /// </summary>
        IOfferRemoteConfiguration Configure { get; }

        /// <summary>
        /// Provide operations for remote installation
        /// </summary>
        IOfferRemoteInstallation Install { get; }
    }
}