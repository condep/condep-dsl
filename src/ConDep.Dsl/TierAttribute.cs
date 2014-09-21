using System;

namespace ConDep.Dsl
{
    public class TierAttribute : Attribute
    {
        private readonly string _tierName;

        public TierAttribute(string tierName)
        {
            _tierName = tierName;
        }

        public TierAttribute(Tier tier)
        {
            _tierName = GetTierName(tier);
        }

        public string TierName { get { return _tierName; } }

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