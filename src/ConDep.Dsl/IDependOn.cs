namespace ConDep.Dsl
{
    public interface IDependOn<T> where T : IProvideArtifact
    {
         
    }

    public interface IdependOnSequence
    {
        void Configure(IOfferArtifactExecutionOrder sequence);
    }
}