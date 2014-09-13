using ConDep.Dsl.Operations;

namespace ConDep.Dsl
{
    public interface IConfigureRemoteInstallation
    {
        void AddOperation(RemoteCompositeOperation operation);
        void AddOperation(IExecuteRemotely operation);
    }
}