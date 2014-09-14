namespace ConDep.Dsl
{
    public interface IOfferArtifactExecutionOrder
    {
        IOfferArtifactFollowedBy StartWith<T>();
        IOfferArtifactPrecededBy EndWith<T>();
    }
}