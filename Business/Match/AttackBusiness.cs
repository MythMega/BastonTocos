using Bastocos.Business.Database.User;
using Bastocos.Entity.Admin;
using Bastocos.Entity.Admin.Settings;
using Bastocos.Entity.Match;
using Bastocos.Entity.Match.Assault;
using Bastocos.Entity.Match.Request;
using Bastocos.Entity.User;
using BastocosR2.Business.Database.Entities;
using BastocosR2.Business.Match;
using BastocosR2.Entity.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bastocos.Business.Match
{
    internal class AttackBusiness
    {
        private EndMatchBusiness _EndMatchBusiness = new EndMatchBusiness();

        internal string Attack(EnvItem env, SettingsItem settings, UserItem attacker)
        {
            var match = env.CurrentMatch;
            if (match is null)
                return "y a pas de match gros tocos";
            if (env.FightQueue.Any(a => a.RequestStatut == RequestStatut.Finishing))
                return "Le match est terminé, un suivant va démarrer prochainement si il y en a en queue";
            if (match.Finished)
                return "C'est bon le combat est finit stop les damages";

            match.FightStats.LastAction = DateTime.Now;

            // Détermine si l'utilisateur est FighterA ou FighterB
            bool isA = match.FighterA.User.Id == attacker.Id;
            bool isB = match.FighterB.User.Id == attacker.Id;

            if (!isA && !isB)
                return "t'es pas dans le match gros tocos";

            // Sélectionne automatiquement l'attaquant et la cible
            var attackerFighter = isA ? match.FighterA : match.FighterB;
            var defenderFighter = isA ? match.FighterB : match.FighterA;

            attackerFighter.FighterStatistic.AttackCount++;
            attackerFighter.FighterStatistic.TotalCommandCount++;

            // Calcul des dégâts
            int fulldamage = attackerFighter.WeaponItem.Attack - defenderFighter.ArmorItem.Defense;
            int blockedDamages = 0;
            if (defenderFighter.ArmorItem.Defense > attackerFighter.WeaponItem.Attack)
            {
                blockedDamages = attackerFighter.WeaponItem.Attack;
            }
            else
            {
                blockedDamages = defenderFighter.ArmorItem.Defense;
            }
            decimal blockedDamageValues = blockedDamages / 2;
            if (blockedDamages > 0)
            {
                fulldamage += (int)Math.Round(blockedDamageValues, 0);
            }
            int damageDone = Math.Max(fulldamage, 0);

            // Applique les dégâts
            defenderFighter.HP_Current -= damageDone;

            // applique les statistiques
            if (env.CurrentMatch.MatchType == FightType.Assault)
            {
                attackerFighter.FighterStatistic.MaxDamageSingleHitAssault = damageDone > attackerFighter.FighterStatistic.MaxDamageSingleHitAssault ? damageDone : attackerFighter.FighterStatistic.MaxDamageSingleHitAssault;
            }
            else
            {
                attackerFighter.FighterStatistic.MaxDamageSingleHitDuel = damageDone > attackerFighter.FighterStatistic.MaxDamageSingleHitDuel ? damageDone : attackerFighter.FighterStatistic.MaxDamageSingleHitDuel;
            }
            attackerFighter.FighterStatistic.TotalDamage += damageDone;

            bool matchfinito = attackerFighter.HP_Current <= 0 || defenderFighter.HP_Current <= 0;
            // minimise les PV a 0
            env.CurrentMatch!.FighterB.HP_Current = env.CurrentMatch.FighterB.HP_Current < 0 ? 0 : env.CurrentMatch.FighterB.HP_Current;
            env.CurrentMatch.FighterA.HP_Current = env.CurrentMatch.FighterA.HP_Current < 0 ? 0 : env.CurrentMatch.FighterA.HP_Current;
            // Message final
            if (matchfinito)
            {
                StringBuilder res = new();
                res.Append("Match Terminé ! ");
                if (attackerFighter.HP_Current <= 0 && defenderFighter.HP_Current <= 0)
                {
                    res.Append("Match nul ! Les deux sont dead, kaput!");
                }
                else
                {
                    List<Fighter> fighters = [attackerFighter, defenderFighter];
                    Fighter winner = fighters.OrderByDescending(f => f.HP_Current).First();
                    res.Append($"Gagnant : {winner.User.Name}, avec {winner.HP_Current} PV restant.");
                    _EndMatchBusiness.EndMatch(env, settings);
                }
                return res.ToString();
            }
            else
            {
                env.CurrentMatch!.WebHitDatas.Add(new BastocosR2.Tools.Json.WebHitData
                {
                    // on envoie sur le perso qui réçoit les dégats, donc pas celui qui attaque
                    Perso = attacker.Id == env.CurrentMatch.FighterA.User.Id ? "B" : "A",

                    // on affiche les dégats inversé (pour 6 damages, on affiche -6)
                    PVEdit = (damageDone * -1).ToString(),
                    Displayed = false,
                });
                return damageDone > 0
                    ? $"{attackerFighter.User.Name} a infligé {damageDone} à {defenderFighter.User.Name}."
                    : $"Dégâts de {attackerFighter.User.Name} totalement bloqués par {defenderFighter.User.Name}.";
            }
        }
    }
}