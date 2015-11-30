using System;
using System.Threading.Tasks;
using ConDep.Dsl.Config;

namespace ConDep.Dsl
{
    public interface IOfferOperations
    {
        IOfferOperations Remote(Action<IOfferRemoteOperations> remote);
        IOfferOperations Remote(Action<IOfferRemoteOperations, ServerConfig> remote);
        IOfferOperations Remote(Action<IOfferRemoteOperations, ServerConfig, ConDepSettings> remote);
        Task<IOfferOperations> RemoteAsync(Action<IOfferRemoteOperations, ServerConfig> remote);
        Task<IOfferOperations> RemoteAsync(Action<IOfferRemoteOperations> remote);
        //IOfferOperations Remote(Tier tier, Action<IOfferRemoteOps> remote);
        //IOfferOperations Remote(string tier, Action<IOfferRemoteOps> remote);
        //IOfferOperations Remote(Tier tier, Action<IOfferRemoteOps, ServerConfig> remote);
        //IOfferOperations Remote(string tier, Action<IOfferRemoteOps, ServerConfig> remote);
        IOfferLocalOperations Local { get; }
    }
}