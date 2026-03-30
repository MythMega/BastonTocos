namespace Bastocos.Entity.Stuffs
{
    public enum StuffRarity
    {
        Common = 1,
        Uncommon = 2,
        Rare = 3,
        Epic = 4,
        Legendary = 5,
    }

    public static class RarityBinding
    {
        public static string COMMON_RARITY = "COMMON";
        public static string UNCOMMON_RARITY = "UNCOMMON";
        public static string RARE_RARITY = "RARE";
        public static string EPIC_RARITY = "EPIC";
        public static string LEGENDARY_RARITY = "LEGENDARY";
        public static string INVALID_RARITY = "INVALID";

        /// <summary>
        /// Converts an enum value to its string label.
        /// </summary>
        public static string ValueToLabel(StuffRarity value)
        {
            switch (value)
            {
                case StuffRarity.Common:
                    return COMMON_RARITY;

                case StuffRarity.Uncommon:
                    return UNCOMMON_RARITY;

                case StuffRarity.Rare:
                    return RARE_RARITY;

                case StuffRarity.Epic:
                    return EPIC_RARITY;

                case StuffRarity.Legendary:
                    return LEGENDARY_RARITY;

                default:
                    return INVALID_RARITY;
            }
        }

        /// <summary>
        /// Converts a string label to its enum value.
        /// Returns null if the label is invalid.
        /// </summary>
        public static StuffRarity? LabelToValue(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
                return null;

            switch (label.Trim().ToUpperInvariant())
            {
                case "COMMON":
                    return StuffRarity.Common;

                case "UNCOMMON":
                    return StuffRarity.Uncommon;

                case "RARE":
                    return StuffRarity.Rare;

                case "EPIC":
                    return StuffRarity.Epic;

                case "LEGENDARY":
                    return StuffRarity.Legendary;

                default:
                    return null;
            }
        }
    }
}