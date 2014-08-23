using ConDep.Dsl.Builders;
using ConDep.Dsl.Operations;
using ConDep.Dsl.Operations.Application.Local;

namespace ConDep.Dsl
{
    /// <summary>
    /// Contains entry points for adding custom operations. Use this class from your extension methods that exposes your custom operations.
    /// </summary>
    public static class Configure
    {
        public static void LocalOperation(IOfferLocalOperations local, LocalOperation operation)
        {
            var seqContainer = local as LocalOperationsBuilder;
            seqContainer.AddOperation(operation);
        }

        public static void ExecuteOperation(IOfferRemoteExecution executor, IExecuteOnServer operation)
        {
            var seqContainer = executor as RemoteExecutionBuilder;
            seqContainer.AddOperation(operation);
        }

        public static void ExecuteOperation(IOfferRemoteExecution executor, RemoteCompositeOperation operation)
        {
            var seqContainer = executor as RemoteExecutionBuilder;
            seqContainer.AddOperation(operation);
        }

        public static void DeployOperation(IOfferRemoteDeployment deployment, RemoteCompositeOperation operation)
        {
            var seqContainer = deployment as RemoteDeploymentBuilder;
            seqContainer.AddOperation(operation);
        }

        public static void DeployOperation(IOfferRemoteDeployment deployment, IExecuteOnServer operation)
        {
            var seqContainer = deployment as RemoteDeploymentBuilder;
            seqContainer.AddOperation(operation);
        }

        public static void Remote(IOfferRemoteOperations remote, IExecuteOnServer operation)
        {
            var seqContainer = remote as RemoteOperationsBuilder;
            seqContainer.AddOperation(operation);
        }

        public static void InstallOperation(IOfferRemoteInstallation installation, IExecuteOnServer operation)
        {
            var seqContainer = installation as RemoteInstallationBuilder;
            seqContainer.AddOperation(operation);
        }

        public static void InstallOperation(IOfferRemoteInstallation installation, RemoteCompositeOperation operation)
        {
            var seqContainer = installation as RemoteInstallationBuilder;
            seqContainer.AddOperation(operation);
        }

        public static void ConfigureOperation(IOfferRemoteConfiguration remoteConfiguration, IExecuteOnServer operation)
        {
            var seqContainer = remoteConfiguration as RemoteConfigurationBuilder;
            seqContainer.AddOperation(operation);
        }

        public static void ConfigureOperation(IOfferRemoteConfiguration remoteConfiguration, RemoteCompositeOperation operation)
        {
            var seqContainer = remoteConfiguration as RemoteConfigurationBuilder;
            seqContainer.AddOperation(operation);
        }
    }
}