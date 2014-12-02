using System;
using System.Collections.Generic;
using System.Linq;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Execution
{
    internal class ServerHandler : IDiscoverServers
    {
        public IEnumerable<ServerConfig> GetServers(IProvideArtifact application, ConDepSettings settings)
        {
            if (settings.Config.UsingTiers)
            {
                ValidateApplicationTier(application, settings);

                var tier = application.GetType().GetCustomAttributes(typeof(TierAttribute), false).Single() as TierAttribute;
                if (!settings.Config.Tiers.Exists(tier.TierName))
                {
                    throw new ConDepTierDoesNotExistInConfigException(string.Format("Tier {0} does not exist in {1}.env.config", tier.TierName, settings.Options.Environment));
                }
                return
                    settings.Config.Tiers.Single(x => x.Name.Equals(tier.TierName, StringComparison.OrdinalIgnoreCase))
                        .Servers;
            }
            return settings.Config.Servers;
        }

        private static void ValidateApplicationTier(IProvideArtifact application, ConDepSettings settings)
        {
            var hasTier = application.GetType().GetCustomAttributes(typeof(TierAttribute), false).Any();
            if (!hasTier) throw new ConDepNoArtifactTierDefinedException(application, settings);

            var hasSingleTier = application.GetType().GetCustomAttributes(typeof(TierAttribute), false).SingleOrDefault() != null;
            if (!hasSingleTier) throw new ConDepNoArtifactTierDefinedException(String.Format("Multiple tiers defined for {0}. Only one tier is allowed by Artifact.", application.GetType().Name));
        }
    }
}