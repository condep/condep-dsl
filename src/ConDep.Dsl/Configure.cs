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
        public static void Local(IOfferLocalOperations local, LocalOperation operation)
        {
            var seqContainer = local as LocalOperationsBuilder;
            seqContainer.AddOperation(operation);
        }

        public static void Execution(IOfferRemoteExecution executor, ForEachServerOperation operation)
        {
            var seqContainer = executor as RemoteExecutionBuilder;
            seqContainer.AddOperation(operation);
        }

        public static void Execution(IOfferRemoteExecution executor, RemoteCompositeOperation operation)
        {
            var seqContainer = executor as RemoteExecutionBuilder;
            seqContainer.AddOperation(operation);
        }

        public static void Deployment(IOfferRemoteDeployment deployment, RemoteCompositeOperation operation)
        {
            var seqContainer = deployment as RemoteDeploymentBuilder;
            seqContainer.AddOperation(operation);
        }

        public static void Deployment(IOfferRemoteDeployment deployment, IOperateRemote operation)
        {
            var seqContainer = deployment as RemoteDeploymentBuilder;
            seqContainer.AddOperation(operation);
        }

        public static void Remote(IOfferRemoteOperations remote, ForEachServerOperation operation)
        {
            var seqContainer = remote as RemoteOperationsBuilder;
            seqContainer.AddOperation(operation);
        }

        public static void Remote(IOfferRemoteOperations remote, RemoteServerOperation operation)
        {
            var seqContainer = remote as RemoteOperationsBuilder;
            seqContainer.AddOperation(operation);
        }

        public static void Installation(IOfferRemoteInstallation installation, RemoteCompositeOperation operation)
        {
            var seqContainer = installation as RemoteInstallationBuilder;
            seqContainer.AddOperation(operation);
        }

        public static void Infrastructure(IOfferInfrastructure infrastructure, RemoteCompositeOperation operation)
        {
            var seqContainer = infrastructure as InfrastructureBuilder;
            seqContainer.AddOperation(operation);
        }
    }
}