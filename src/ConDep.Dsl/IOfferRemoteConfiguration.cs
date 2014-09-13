using System;
using ConDep.Dsl.Config;

namespace ConDep.Dsl
{
    public interface IOfferRemoteConfiguration
    {

        /// <summary>
        /// Server side condition. Any Operation followed by <see cref="OnlyIf"/> will only execute if the condition is met.
        /// </summary>
        /// <param name="condition">The condition that must be met</param>
        /// <returns></returns>
        IOfferRemoteConfiguration OnlyIf(Predicate<ServerInfo> condition);
    }
}