using ConDep.Dsl.Validation;

namespace ConDep.Dsl.Operations
{
    public abstract class RemoteCompositeOperationBase : IValidate
    {
        public abstract string Name { get; }
        public abstract bool IsValid(Notification notification);
    }
}