using ConDep.Dsl.Validation;

namespace ConDep.Dsl
{
    public abstract class RemoteCompositeOperation : IValidate
    {
        public abstract void Configure(IOfferRemoteComposition server);
        public abstract string Name { get; }
        public abstract bool IsValid(Notification notification);
    }
}