using System;
using System.Threading;
using System.Threading.Tasks;
using ConDep.Dsl.Config;
using ConDep.Dsl.Harvesters;
using ConDep.Dsl.LoadBalancer;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Remote;
using ConDep.Dsl.Sequence;
using ConDep.Dsl.Validation;

namespace ConDep.Dsl.Execution
{
    public class ConDepConfigurationExecutor
    {
        private bool _cancelled;
        private bool _serverNodeInstalled;

        public static ConDepExecutionResult ExecuteFromAssembly(ConDepSettings conDepSettings, CancellationToken token)
        {
            try
            {
                if (conDepSettings.Options.Assembly == null) throw new ArgumentException("assembly");

                var clientValidator = new ClientValidator();

                var serverInfoHarvester = HarvesterFactory.GetHarvester(conDepSettings);
                var serverValidator = new RemoteServerValidator(conDepSettings.Config.Servers,
                                                                serverInfoHarvester);

                var lbLookup = new LoadBalancerLookup(conDepSettings.Config.LoadBalancer);
                var sequenceManager = new ExecutionSequenceManager(conDepSettings.Config.Servers, lbLookup.GetLoadBalancer());

                ArtifactHandler.PopulateExecutionSequence(conDepSettings, sequenceManager);

                if (conDepSettings.Options.DryRun)
                {
                    Logger.Warn("Showing execution sequence from dry run:");
                    sequenceManager.DryRun(conDepSettings);
                    return new ConDepExecutionResult(true);
                }

                return new ConDepConfigurationExecutor().Execute(conDepSettings, clientValidator,
                                                                        serverValidator, sequenceManager, token);
            }
            catch (Exception ex)
            {
                Logger.Error("An error sneaked by.", ex);
                throw;
            }
        }

        public static Task<ConDepExecutionResult> ExecuteFromAssemblyAsync(ConDepSettings conDepSettings, CancellationToken token)
        {
            return Task.Factory.StartNew(() => ExecuteFromAssembly(conDepSettings, token), token);
        }

        internal ConDepExecutionResult Execute(ConDepSettings settings, IValidateClient clientValidator, IValidateServer serverValidator, ExecutionSequenceManager execManager, CancellationToken token)
        {
            if (settings == null) { throw new ArgumentException("settings"); }
            if (settings.Config == null) { throw new ArgumentException("settings.Config"); }
            if (settings.Options == null) { throw new ArgumentException("settings.Options"); }
            if (clientValidator == null) { throw new ArgumentException("clientValidator"); }
            if (serverValidator == null) { throw new ArgumentException("serverValidator"); }
            if (execManager == null) { throw new ArgumentException("execManager"); }

            var status = new StatusReporter();

            try
            {
                Validate(clientValidator, serverValidator);

                ExecutePreOps(settings, status, token);
                _serverNodeInstalled = true;

                //Todo: Result of merge. Not sure if this is correct.
                token.Register(() => Cancel(settings, status, token));

                var notification = new Notification();
                if (!execManager.IsValid(notification))
                {
                    notification.Throw();
                }

                execManager.Execute(status, settings, token);
                return new ConDepExecutionResult(true);
            }
            catch (OperationCanceledException)
            {
                Cancel(settings, status, token);
                return new ConDepExecutionResult(false) { Cancelled = true };
            }
            catch (AggregateException aggEx)
            {
                var result = new ConDepExecutionResult(false);
                aggEx.Handle(inner =>
                {
                    if (inner is OperationCanceledException)
                    {
                        Cancel(settings, status, token);
                        result.Cancelled = true;
                        Logger.Warn("ConDep execution cancelled.");
                    }
                    else
                    {
                        result.AddException(inner);
                        Logger.Error("ConDep execution failed.", inner);
                    }

                    return true;
                });
                return result;
            }
            catch (Exception ex)
            {
                var result = new ConDepExecutionResult(false);
                result.AddException(ex);
                Logger.Error("ConDep execution failed.", ex);
                return result;
            }
            finally
            {
                if (!_cancelled) ExecutePostOps(settings, status, token);
            }
        }

        private void Cancel(ConDepSettings settings, StatusReporter status, CancellationToken token)
        {
            Logger.WithLogSection("Cancellation", () =>
            {
                try
                {
                    var tokenSource = new CancellationTokenSource();
                    Logger.Warn("Cancelling execution gracefully!");
                    _cancelled = true;
                    if (_serverNodeInstalled) ExecutePostOps(settings, status, tokenSource.Token);
                }
                catch (AggregateException aggEx)
                {
                    foreach (var ex in aggEx.InnerExceptions)
                    {
                        Logger.Error("Failure during cancellation", ex);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Failure during cancellation", ex);
                }
            });
        }

        private static void Validate(IValidateClient clientValidator, IValidateServer serverValidator)
        {
            clientValidator.Validate();

            if (!serverValidator.IsValid())
            {
                throw new ConDepValidationException("Not all servers fulfill ConDep's requirements. Aborting execution.");
            }
        }

        public static void ExecutePreOps(ConDepSettings conDepSettings, IReportStatus status, CancellationToken token)
        {
            Logger.WithLogSection("Executing pre-operations", () =>
            {
                foreach (var server in conDepSettings.Config.Servers)
                {
                    Logger.WithLogSection(server.Name, () =>
                    {
                        //Todo: This will not work with ConDep server. After first run, this key will always exist.
                        if (!ConDepGlobals.ServersWithPreOps.ContainsKey(server.Name))
                        {
                            var remotePreOps = new PreRemoteOps();
                            remotePreOps.Execute(server, status, conDepSettings, token);
                            ConDepGlobals.ServersWithPreOps.Add(server.Name, server);
                        }
                    });
                }
            });
        }

        private static void ExecutePostOps(ConDepSettings conDepSettings, IReportStatus status, CancellationToken token)
        {
            foreach (var server in conDepSettings.Config.Servers)
            {
                //Todo: This will not work with ConDep server. After first run, this key will always exist.
                if (ConDepGlobals.ServersWithPreOps.ContainsKey(server.Name))
                {
                    var remotePostOps = new PostRemoteOps();
                    remotePostOps.Execute(server, status, conDepSettings, token);
                    ConDepGlobals.ServersWithPreOps.Remove(server.Name);
                }
            }
        }
    }
}