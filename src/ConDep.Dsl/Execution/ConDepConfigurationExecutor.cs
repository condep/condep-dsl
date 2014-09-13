using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ConDep.Dsl.Builders;
using ConDep.Dsl.Config;
using ConDep.Dsl.Harvesters;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Operations;
using ConDep.Dsl.Operations.LoadBalancer;
using ConDep.Dsl.Remote;
using ConDep.Dsl.SemanticModel;
using ConDep.Dsl.Sequence;
using ConDep.Dsl.Validation;

namespace ConDep.Dsl.Execution
{
    //Todo: Screaming for refac! 
    public class ConDepConfigurationExecutor
    {
        private bool _cancelled = false;
        private bool _serverNodeInstalled = false;

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

                PopulateExecutionSequence(conDepSettings, sequenceManager);

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

        public ConDepExecutionResult Execute(ConDepSettings settings, IValidateClient clientValidator, IValidateServer serverValidator, ExecutionSequenceManager execManager, CancellationToken token)
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
                //new PostOpsSequence().Execute(status, settings);
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

            //var serverInfoHarvester = new ServerInfoHarvester(conDepSettings);
            //var serverValidator = new RemoteServerValidator(conDepSettings.Config.Servers, serverInfoHarvester);
            if (!serverValidator.IsValid())
            {
                throw new ConDepValidationException("Not all servers fulfill ConDep's requirements. Aborting execution.");
            }
        }

        private static void PopulateExecutionSequence(ConDepSettings conDepSettings, ExecutionSequenceManager sequenceManager)
        {
            var artifacts = CreateApplicationArtifacts(conDepSettings);
            foreach (var artifact in artifacts)
            {
                PopulateDependencies(conDepSettings, artifact, sequenceManager);
                ConfigureArtifact(conDepSettings, sequenceManager, artifact);
            }
        }

        private static void PopulateDependencies(ConDepSettings conDepSettings, IProvideArtifact application, ExecutionSequenceManager sequenceManager)
        {
            var dependencyHandler = new ArtifactDependencyHandler(application);
            if (dependencyHandler.HasDependenciesDefined())
            {
                var dependencies = dependencyHandler.GetDependeciesForArtifact(conDepSettings);
                foreach (var dependency in dependencies)
                {
                    ConfigureArtifact(conDepSettings, sequenceManager, dependency);
                }
            }
        }

        private static void ConfigureArtifact(ConDepSettings conDepSettings, ExecutionSequenceManager sequenceManager, IProvideArtifact dependency)
        {
            if (dependency is Artifact.Local)
            {
                var localSequence = sequenceManager.NewLocalSequence(dependency.GetType().Name);
                var localBuilder = new LocalOperationsBuilder(localSequence);
                ((Artifact.Local)dependency).Configure(localBuilder, conDepSettings);
            }
            else if (dependency is Artifact.Remote)
            {
                var remoteSequence = sequenceManager.NewRemoteSequence(dependency.GetType().Name);
                var remoteBuilder = new RemoteOperationsBuilder(remoteSequence);
                ((Artifact.Remote)dependency).Configure(remoteBuilder, conDepSettings);
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

        private static IEnumerable<IProvideArtifact> CreateApplicationArtifacts(ConDepSettings settings)
        {
            var assembly = settings.Options.Assembly;
            if (settings.Options.HasApplicationDefined())
            {
                var type = assembly.GetTypes().SingleOrDefault(t => typeof(IProvideArtifact).IsAssignableFrom(t) && t.Name == settings.Options.Application);
                if (type == null)
                {
                    throw new ConDepConfigurationTypeNotFoundException(string.Format("A class inheriting from [{0}] or [{1}] must be present in assembly [{2}] for ConDep to work. No calss with name [{3}] found in assembly. ", typeof(Artifact.Local).FullName, typeof(Artifact.Remote).FullName, assembly.FullName, settings.Options.Application));
                }
                yield return CreateApplicationArtifact(type);
            }
            else
            {
                var types = assembly.GetTypes().Where(t => typeof(IProvideArtifact).IsAssignableFrom(t));
                foreach (var type in types)
                {
                    yield return CreateApplicationArtifact(type);
                }
            }
        }

        private static IProvideArtifact CreateApplicationArtifact(Type type)
        {
            var application = Activator.CreateInstance(type) as IProvideArtifact;
            if (application == null) throw new NullReferenceException(string.Format("Instance of application class [{0}] not found.", type.FullName));
            return application;
        }
    }
}