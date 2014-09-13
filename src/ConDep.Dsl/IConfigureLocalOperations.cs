using ConDep.Dsl.Operations;

namespace ConDep.Dsl
{
    /// <summary>
    /// Expose functionality for custom local operations to be added to ConDep's execution sequence.
    /// </summary>
    public interface IConfigureLocalOperations
    {
        void AddOperation(LocalOperation operation);
    }
}