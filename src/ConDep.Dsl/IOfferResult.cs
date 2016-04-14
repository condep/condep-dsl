namespace ConDep.Dsl
{
    public interface IOfferResult
    {
        /// <summary>
        /// Result of the previous executed operation
        /// </summary>
        Result Result { get; set; }
    }
}