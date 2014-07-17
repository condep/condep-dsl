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
    }
}