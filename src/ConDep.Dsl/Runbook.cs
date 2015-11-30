using ConDep.Dsl.Config;

namespace ConDep.Dsl
{
    /// <summary>
    /// A Runbook is where you define the execution sequence of all your operations in ConDep.
    /// </summary>
    public abstract class Runbook
    {
        public abstract void Execute(IOfferOperations dsl, ConDepSettings settings);
    }
}