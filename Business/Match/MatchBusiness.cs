using Bastocos.Business.Account;
using Bastocos.Business.Database.User;
using Bastocos.Entity;
using Bastocos.Entity.Admin;
using Bastocos.Entity.Admin.Settings;
using Bastocos.Entity.Match;
using Bastocos.Entity.Match.Assault;
using Bastocos.Entity.Match.Fight;
using Bastocos.Entity.Match.Request;
using Bastocos.Entity.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bastocos.Business.Match
{
    internal class MatchBusiness
    {
        private AccountManagement AccountManagement = new AccountManagement();
        private UserDatabaseConnection userDb = new UserDatabaseConnection();

        public void InitializeDuel()
        {
        }

        public void InitializeAssault(EnvItem envItem, FightAssaultRequest request)
        {
            request.UserItemB = AccountManagement.GetFromUsername(request.UserToDuel, envItem.Dbconnect);
            if (request.UserItemB == null)
            {
                throw new Exception(ErrorItem.ERROR_ACCOUN_USER_NOT_FOUND);
            }
            request.UserItemB = userDb.GetFromUserName(request.UserToDuel);
            envItem.FightQueue.Add(request);
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
            globalEnvironmentItem.CurrentMatch = new FightItem
            {
                FighterA = new Fighter
                {
                    User = futurematch.UserItemA,
                    HP_Current = HP,
                    HP_Max = HP,
                },
                FighterB = new Fighter
                {
                    User = futurematch.UserItemB,
                    HP_Current = HP,
                    HP_Max = HP,
                },
                FightStats = new FightStats
                {
                    LastAction = DateTime.Now,
                    StartTime = DateTime.Now,
                }
            };
        }
    }
}