using System;

namespace ConDep.Dsl
{
    public class TierAttribute : Attribute
    {
        public TierAttribute(string tierName)
        {
            TierName = tierName;
        }

        public TierAttribute(Tier tier)
        {
            TierName = GetTierName(tier);
        }

        public string TierName { get; }

        private string GetTierName(Tier tier)
        {
            switch (tier)
            {
                case Tier.Application:
                    return "application";
                case Tier.Database:
                    return "database";
                case Tier.Web:
                    return "web";
                default:
                    throw new ConDepInvalidEnumValueException(tier);
            }
        }
    }
}