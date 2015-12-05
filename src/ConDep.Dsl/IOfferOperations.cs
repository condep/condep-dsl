using System;
using System.Threading.Tasks;
using ConDep.Dsl.Config;

namespace ConDep.Dsl
{
    public interface IOfferOperations
    {
        /// <summary>
        /// Will make sure to take servers offline from load balancer before executing 
        /// operations on it. This requires load balancer configuration in your env.json config. 
        /// </summary>
        /// <param name="mode">Which load balancer mode to use during execution</param>
        /// <param name="farm"></param>
        /// <param name="dsl"></param>
        /// <returns></returns>
        IOfferOperations LoadBalance(LoadBalancerMode mode, string farm, Action<IOfferRemoteOperations> remote);

        IOfferOperations LoadBalance(LoadBalancerMode mode, string farm, Action<IOfferRemoteOperations, ServerConfig> remote);

        IOfferOperations LoadBalance(LoadBalancerMode mode, Tier tier, string farm, Action<IOfferRemoteOperations> remote);

        IOfferOperations LoadBalance(LoadBalancerMode mode, Tier tier, string farm, Action<IOfferRemoteOperations, ServerConfig> remote);

        IOfferOperations LoadBalance(LoadBalancerMode mode, string tier, string farm, Action<IOfferRemoteOperations> remote);

        IOfferOperations LoadBalance(LoadBalancerMode mode, string tier, string farm, Action<IOfferRemoteOperations, ServerConfig> remote);

        /// <summary>
        /// Allows you to synchroniously execute opertions on remote servers
        /// </summary>
        /// <param name="remote">A DSL with all available remote operations</param>
        /// <returns></returns>
        IOfferOperations Remote(Action<IOfferRemoteOperations> remote);

        /// <summary>
        /// Allows you to synchroniously execute opertions on remote servers, with access
        /// to the <see cref="ServerConfig"/> object.
        /// </summary>
        /// <param name="remote">A DSL with all available remote operations</param>
        /// <returns></returns>
        IOfferOperations Remote(Action<IOfferRemoteOperations, ServerConfig> remote);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="remote"></param>
        /// <returns></returns>
        Task<IOfferOperations> RemoteAsync(Action<IOfferRemoteOperations, ServerConfig> remote);

        /// <summary>
        /// Allows you to asynchroniously execute opertions on remote servers. Will return a <see cref="Task"/>
        /// object you need to handle.
        /// </summary>
        /// <param name="remote">A DSL with all available remote operations</param>
        /// <returns></returns>
        Task<IOfferOperations> RemoteAsync(Action<IOfferRemoteOperations> remote);

        /// <summary>
        /// Allows you to synchroniously execute opertions on remote servers located in a specific <see cref="Tier"/>
        /// </summary>
        /// <param name="tier"></param>
        /// <param name="remote">A DSL with all available remote operations</param>
        /// <returns></returns>
        IOfferOperations Remote(Tier tier, Action<IOfferRemoteOperations> remote);

        /// <summary>
        /// Allows you to synchroniously execute opertions on remote servers located in a specific <see cref="Tier"/>, 
        /// with access to the <see cref="ServerConfig"/> object.
        /// </summary>
        /// <param name="tier">The <see cref="Tier"/> to execute on (a collection of servers)</param>
        /// <param name="remote">A DSL with all available remote operations</param>
        /// <returns></returns>
        IOfferOperations Remote(Tier tier, Action<IOfferRemoteOperations, ServerConfig> remote);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tier"></param>
        /// <param name="remote"></param>
        /// <returns></returns>
        IOfferOperations Remote(string tier, Action<IOfferRemoteOperations> remote);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tier"></param>
        /// <param name="remote"></param>
        /// <returns></returns>
        IOfferOperations Remote(string tier, Action<IOfferRemoteOperations, ServerConfig> remote);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tier"></param>
        /// <param name="remote"></param>
        /// <returns></returns>
        Task<IOfferOperations> RemoteAsync(Tier tier, Action<IOfferRemoteOperations> remote);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tier"></param>
        /// <param name="remote"></param>
        /// <returns></returns>
        Task<IOfferOperations> RemoteAsync(string tier, Action<IOfferRemoteOperations> remote);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tier"></param>
        /// <param name="remote"></param>
        /// <returns></returns>
        Task<IOfferOperations> RemoteAsync(Tier tier, Action<IOfferRemoteOperations, ServerConfig> remote);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tier"></param>
        /// <param name="remote"></param>
        /// <returns></returns>
        Task<IOfferOperations> RemoteAsync(string tier, Action<IOfferRemoteOperations, ServerConfig> remote);

        /// <summary>
        /// 
        /// </summary>
        IOfferLocalOperations Local { get; }
    }
}