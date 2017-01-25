using ConDep.Dsl.Config;

namespace ConDep.Dsl
{
    /// <summary>
    /// A Runbook is where you define the execution sequence of all your operations in ConDep.
    /// </summary>
    public abstract class Runbook
    {
        public abstract void Execute(IOfferOperations dsl, ConDepSettings settings);

        /// <summary>
        /// Will execute a Runbook of given type T. Use this if you want to execute a Runbook from another Runbook.
        /// </summary>
        /// <typeparam name="T">Runbook type</typeparam>
        /// <param name="dsl">The ConDep DSL</param>
        /// <param name="settings">The ConDep settings</param>
        public static void Execute<T>(IOfferOperations dsl, ConDepSettings settings) where T : Runbook, new()
        {
            var runbook = new T();
            runbook.Execute(dsl, settings);
        }
    }
}