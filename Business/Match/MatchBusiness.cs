using Bastocos.Business.Account;
using Bastocos.Business.Database.User;
using Bastocos.Entity;
using Bastocos.Entity.Admin;
using Bastocos.Entity.Admin.Settings;
using Bastocos.Entity.Cards;
using Bastocos.Entity.Match;
using Bastocos.Entity.Match.Assault;
using Bastocos.Entity.Match.Fight;
using Bastocos.Entity.Match.Request;
using Bastocos.Entity.Stuffs.Equipments;
using Bastocos.Entity.Stuffs.Equipments.Armors;
using Bastocos.Entity.Stuffs.Equipments.Weapons;
using Bastocos.Entity.Stuffs.Trashs;
using Bastocos.Entity.User;
using BastocosR2.Business.Database.Entities;
using BastocosR2.Entity.Database;
using BastocosR2.Tools;
using System.Text;

namespace Bastocos.Business.Match
{
    internal class MatchBusiness
    {
        private AccountManagement AccountManagement = new AccountManagement();
        private UserDatabaseConnection userDb = new UserDatabaseConnection();
        private DefaultElements _defaultElements = new DefaultElements();
        private AttackBusiness _attackBusiness = new AttackBusiness();
        private LootBusiness _lootBusiness = new LootBusiness();

        public void InitializeDuel()
        {
        }

        public string InitializeAssault(EnvItem envItem, FightAssaultRequest request)
        {
            envItem.RefreshLastActivity(request.UserItemA);
            request.UserToDuel = request.UserToDuel.Replace("@", "");
            if (request.UserToDuel == request.UserItemA.Name)
            {
                return "Tu ne peux pas te duel toi même gros con";
            }
            request.UserItemB = AccountManagement.GetFromUsername(request.UserToDuel, envItem.Dbconnect);
            if (request.UserItemB == null)
            {
                return "Utilisateur non trouvé";
            }
            if (!envItem.IsStillActive(request.UserItemB))
            {
                return $"L'utilisateur {request.UserItemB.Name} ne semble pas être actif.";
            }
            if (envItem.FightQueue.Any(a =>
            (a.RequestStatut == RequestStatut.Running || a.RequestStatut == RequestStatut.In_Queue) &&
            a.UserItemA.Id == request.UserItemA.Id))
            {
                return request.UserItemA.Name + " est déjà en attente d'un combat / en combat";
            }
            if (envItem.FightQueue.Any(a =>
            (a.RequestStatut == RequestStatut.Running || a.RequestStatut == RequestStatut.In_Queue) &&
            a.UserItemB!.Name.Equals(request.UserToDuel, StringComparison.CurrentCultureIgnoreCase)))
            {
                return request.UserItemB.Name + " est déjà en attente d'un combat / en combat";
            }
            request.UserItemB = userDb.GetFromUserName(request.UserToDuel);
            request.RequestStatut = RequestStatut.In_Queue;
            FightRequest fightRequest = request;
            fightRequest.FightType = FightType.Assault;
            envItem.FightQueue.Add(request);
            return $"Combat ajouté a la liste d'attente {envItem.FightQueue.Where(f => f.RequestStatut == RequestStatut.In_Queue).Count()}";
        }

        public void Start(EnvItem globalEnvironmentItem, MatchSettings matchSettings)
        {
            FightRequest futurematch = globalEnvironmentItem.FightQueue.OrderBy(o => o.DateRequest).First(w => w.RequestStatut == RequestStatut.In_Queue);
            globalEnvironmentItem.FightQueue.OrderBy(o => o.DateRequest).First(w => w.RequestStatut == RequestStatut.In_Queue).RequestStatut = RequestStatut.Running;
            if (futurematch == null)
            {
                return;
            }
            int HP = matchSettings.DefaultsMatchSettings.DefaultHP;
            if (futurematch.UserItemB is null)
                throw new Exception(ErrorItem.ERROR_ACCOUNT_ALREADY_EXIST);
            globalEnvironmentItem.CurrentMatch = new FightItem
            {
                FighterA = new Fighter
                {
                    User = futurematch.UserItemA,
                    HP_Current = HP,
                    HP_Max = HP,
                    ArmorItem = _defaultElements.DefaultArmorItem,
                    WeaponItem = _defaultElements.DefaultWeaponItem,
                },
                FighterB = new Fighter
                {
                    User = futurematch.UserItemB,
                    HP_Current = HP,
                    HP_Max = HP,
                    ArmorItem = _defaultElements.DefaultArmorItem,
                    WeaponItem = _defaultElements.DefaultWeaponItem,
                },
                FightStats = new FightStats
                {
                    LastAction = DateTime.Now,
                    StartTime = DateTime.Now,
                }
            };
        }

        internal string SearchForLoot(EnvItem globalEnvironmentItem, SettingsItem globalSettingsItems, UserItem userWhoLoot)
        {
            return _lootBusiness.SearchForLoot(globalEnvironmentItem, globalSettingsItems, userWhoLoot);
        }

        internal string Attack(EnvItem env, SettingsItem settings, UserItem attacker)
        {
            return _attackBusiness.Attack(env, settings, attacker);
        }
    }
}