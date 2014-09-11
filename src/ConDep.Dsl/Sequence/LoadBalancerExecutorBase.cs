using System.Collections.Generic;
using System.Threading;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.SemanticModel;

namespace ConDep.Dsl.Sequence
{
    public abstract class LoadBalancerExecutorBase
    {
        public abstract void Execute(IReportStatus status, ConDepSettings settings, CancellationToken token);

        protected void ExecuteOnServer(ServerConfig server, IReportStatus status, ConDepSettings settings, ILoadBalance loadBalancer, bool bringServerOfflineBeforeExecution, bool bringServerOnlineAfterExecution, CancellationToken token)
        {
            var errorDuringLoadBalancing = false;

            Logger.WithLogSection(server.Name, () =>
                {
                    try
                    {
                        if (bringServerOfflineBeforeExecution)
                        {
                            Logger.Info(string.Format("Taking server [{0}] offline in load balancer.", server.Name));
                            loadBalancer.BringOffline(server.Name, server.LoadBalancerFarm,
                                                      LoadBalancerSuspendMethod.Suspend, status);
                        }

                        //ExecuteOnServer(server, status, settings, token);
                    }
                    catch
                    {
                        errorDuringLoadBalancing = true;
                        throw;
                    }
                    finally
                    {
                        //&& !status.HasErrors
                        if (bringServerOnlineAfterExecution && !errorDuringLoadBalancing)
                        {
                            Logger.Info(string.Format("Taking server [{0}] online in load balancer.", server.Name));
                            loadBalancer.BringOnline(server.Name, server.LoadBalancerFarm, status);
                        }
                    }
                });

        }

        public void DryRun()
        {
            //foreach (var item in _sequence)
            //{
            //    Logger.WithLogSection(item.Name, () => { item.DryRun(); });
            //}
        }
    }
}