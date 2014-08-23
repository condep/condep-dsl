using ConDep.Dsl.Operations;

namespace ConDep.Dsl
{
    /// <summary>
    /// Expose functionality for custom remote operations to be added to ConDep's execution sequence.
    /// </summary>
    public interface IConfigureRemoteDeployment
    {
        void AddOperation(RemoteCompositeOperation operation);
        void AddOperation(IExecuteOnServer operation);
    }
}