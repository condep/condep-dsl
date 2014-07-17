using System;
using ConDep.Dsl.Config;

namespace ConDep.Dsl
{
    public interface IOfferInfrastructure
    {

        /// <summary>
        /// Server side condition. Any Operation followed by <see cref="OnlyIf"/> will only execute if the condition is met.
        /// </summary>
        /// <param name="condition">The condition that must be met</param>
        /// <returns></returns>
        IOfferInfrastructure OnlyIf(Predicate<ServerInfo> condition);
    }
}