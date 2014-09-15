namespace ConDep.Dsl
{
    public abstract class RemoteCompositeOperation : RemoteCompositeOperationBase
    {
        public abstract void Configure(IOfferRemoteComposition server);
    }
}