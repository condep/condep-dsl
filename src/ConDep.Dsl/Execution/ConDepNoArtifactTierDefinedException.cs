using System;
using System.Linq;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Execution
{
    public class ConDepNoArtifactTierDefinedException : Exception
    {
        public ConDepNoArtifactTierDefinedException(string message) : base(message)
        {
        }

        public ConDepNoArtifactTierDefinedException(IProvideArtifact application, ConDepSettings settings)
            : base(string.Format("No Tiers defined for application {0}. You need to specify a tier using the {1} attribute on the {0} class. Tiers available in your configuration are {2}.",
                application.GetType().Name, 
                typeof(TierAttribute).Name, 
                string.Join(", ", settings.Config.Tiers.Select(x => x.Name))))
        {
        }
    }
}