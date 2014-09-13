using System;

namespace ConDep.Dsl
{
    public interface IOfferLocalOperations
    {
        /// <summary>
        /// Provide operations to perform on remote servers
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        IOfferLocalOperations ToEachServer(Action<IOfferRemoteOperations> action);

        /// <summary>
        /// Provide operations to perform on remote server in paralell. Note: Since this runs 
        /// in paralell, any form of integration with any load balancer will be skipped.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        IOfferLocalOperations ToEachServerInParalell(Action<IOfferRemoteOperations> action);
    }
}