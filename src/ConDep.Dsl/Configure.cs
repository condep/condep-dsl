using System;
using ConDep.Dsl.Builders;

namespace ConDep.Dsl
{
    /// <summary>
    /// Contains entry points for adding custom operations. Use this class from your extension methods that exposes your custom operations.
    /// </summary>
    public static class Configure
    {
        public static void Operation(IOfferLocalOperations local, LocalOperation operation)
        {
            var seqContainer = local as LocalOperationsBuilder;
            seqContainer.AddOperation(operation);
        }

        public static void Operation(IOfferRemoteExecution executor, IExecuteRemotely operation)
        {
            var seqContainer = executor as RemoteExecutionBuilder;
            seqContainer.AddOperation(operation);
        }

        public static void Operation(IOfferRemoteExecution executor, RemoteCompositeOperation operation)
        {
            var seqContainer = executor as RemoteExecutionBuilder;
            seqContainer.AddOperation(operation);
        }

        public static void Operation(IOfferRemoteDeployment deployment, RemoteCompositeOperation operation)
        {
            var seqContainer = deployment as RemoteDeploymentBuilder;
            seqContainer.AddOperation(operation);
        }

        public static void Operation(IOfferRemoteDeployment deployment, IExecuteRemotely operation)
        {
            var seqContainer = deployment as RemoteDeploymentBuilder;
            seqContainer.AddOperation(operation);
        }

        public static void Operation(IOfferRemoteOperations remote, IExecuteRemotely operation)
        {
            if (remote is RemoteOperationsBuilder)
            {
                var seqContainer = remote as RemoteOperationsBuilder;
                seqContainer.AddOperation(operation);
                return;
            }
            if (remote is RemoteCompositeBuilder)
            {
                var seqContainer = remote as RemoteCompositeBuilder;
                seqContainer.CompositeSequence.Add(operation);
                return;
            }
            throw new Exception(string.Format("Type {0} not currently supported.", remote.GetType().Name));
        }


        public static void Operation(IOfferRemoteInstallation installation, IExecuteRemotely operation)
        {
            var seqContainer = installation as RemoteInstallationBuilder;
            seqContainer.AddOperation(operation);
        }

        public static void Operation(IOfferRemoteInstallation installation, RemoteCompositeOperation operation)
        {
            var seqContainer = installation as RemoteInstallationBuilder;
            seqContainer.AddOperation(operation);
        }

        public static void Operation(IOfferRemoteConfiguration remoteConfiguration, IExecuteRemotely operation)
        {
            var seqContainer = remoteConfiguration as RemoteConfigurationBuilder;
            seqContainer.AddOperation(operation);
        }

        public static void Operation(IOfferRemoteConfiguration remoteConfiguration, RemoteCompositeOperation operation)
        {
            var seqContainer = remoteConfiguration as RemoteConfigurationBuilder;
            seqContainer.AddOperation(operation);
        }
    }
}