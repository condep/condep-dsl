using ConDep.Dsl.Validation;

namespace ConDep.Dsl
{
    public abstract class RemoteCompositeOperationBase : IValidate
    {
        public abstract string Name { get; }
        public abstract bool IsValid(Notification notification);
    }
}