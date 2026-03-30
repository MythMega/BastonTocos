using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bastocos.Entity.User
{
    /// <summary>
    /// Represents all tracked statistics for a player.
    /// </summary>
    public class StatisticsItem
    {
        /// <summary>Total number of victories (all modes combined).</summary>
        public int VictoryCount { get; set; }

        /// <summary>Total number of defeats (all modes combined).</summary>
        public int DefeatCount { get; set; }

        /// <summary>Total number of draws (all modes combined).</summary>
        public int DrawCount { get; set; }

        /// <summary>Total number of AFK occurrences (all modes combined).</summary>
        public int AfkCount { get; set; }

        /// <summary>Total number of attacks performed.</summary>
        public int AttackCount { get; set; }

        /// <summary>Total number of times the player has been looted.</summary>
        public int LootedCount { get; set; }

        /// <summary>Total amount of money currently owned.</summary>
        public int Money { get; set; }

        /// <summary>Maximum damage dealt in a single hit during a duel.</summary>
        public int MaxDamageSingleHitDuel { get; set; }

        /// <summary>Maximum damage dealt in a single hit during an assault.</summary>
        public int MaxDamageSingleHitAssault { get; set; }

        /// <summary>Maximum total damage dealt in a single duel.</summary>
        public int MaxDamageDuel { get; set; }

        /// <summary>Maximum total damage dealt in a single assault.</summary>
        public int MaxDamageAssault { get; set; }

        /// <summary>Total accumulated damage across all fights.</summary>
        public int TotalDamage { get; set; }

        /// <summary>Date of the player's first duel.</summary>
        public DateTime? FirstDuelDate { get; set; }

        // --- Economy ---

        /// <summary>Total number of purchases made (1 item per purchase).</summary>
        public int PurchaseCount { get; set; }

        /// <summary>Total amount of money spent.</summary>
        public int MoneySpent { get; set; }

        /// <summary>Total number of sales made.</summary>
        public int SaleCount { get; set; }

        /// <summary>Total amount of money earned from sales.</summary>
        public int MoneyEarned { get; set; }

        // --- Cards / Cardex ---

        /// <summary>Total number of cards looted.</summary>
        public int CardsLooted { get; set; }

        /// <summary>Total number of cards sold.</summary>
        public int CardsSold { get; set; }

        /// <summary>Percentage of cardex completion (0 to 100).</summary>
        public float CardexProgress { get; set; }

        // --- Activity ---

        /// <summary>Total number of active days.</summary>
        public int ActiveDays { get; set; }

        /// <summary>Total number of assaults launched.</summary>
        public int AssaultsLaunched { get; set; }

        /// <summary>Total number of assaults revived/resurrected.</summary>
        public int AssaultsResurrected { get; set; }

        /// <summary>Total number of duel participations.</summary>
        public int DuelParticipationCount { get; set; }

        // --- Assault Stats ---

        /// <summary>Total number of victories in assault mode.</summary>
        public int AssaultVictoryCount { get; set; }

        /// <summary>Total number of defeats in assault mode.</summary>
        public int AssaultDefeatCount { get; set; }

        /// <summary>Total number of draws in assault mode.</summary>
        public int AssaultDrawCount { get; set; }

        /// <summary>Total number of AFK occurrences in assault mode.</summary>
        public int AssaultAfkCount { get; set; }

        // --- Duel Stats ---

        /// <summary>Total number of victories in duel mode.</summary>
        public int DuelVictoryCount { get; set; }

        /// <summary>Total number of defeats in duel mode.</summary>
        public int DuelDefeatCount { get; set; }

        /// <summary>Total number of draws in duel mode.</summary>
        public int DuelDrawCount { get; set; }

        /// <summary>Total number of AFK occurrences in duel mode.</summary>
        public int DuelAfkCount { get; set; }

        // --- Misc ---

        /// <summary>Total number of commands executed.</summary>
        public int TotalCommandCount { get; set; }
    }
}