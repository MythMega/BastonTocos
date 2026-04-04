using Bastocos.Entity;
using Bastocos.Entity.Admin;
using Bastocos.Entity.Admin.Database;
using Bastocos.Entity.User;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Bastocos.Business.Account
{
    public class AccountManagement
    {
        public bool AccountExist(int accountId, DatabaseConnection dbconnect)
        {
            return dbconnect.Account_Exist(accountId);
        }

        public string CreateAccount(UserItem account, EnvItem Env)
        {
            if (account == null)
            {
                return "Invalid data sent";
            }
            if (Env.Users.Any(acc => acc.Utilisateur.Id == account.Id))
            {
                return "account already created and in data";
            }
            DatabaseConnection dbconnect = Env.Dbconnect;
            if (!AccountExist(account.Id, dbconnect))
            {
                dbconnect.Account_Create(account);
                return $"Account {account.Name} registered!";
            }
            else
            {
                throw new Exception(ErrorItem.ERROR_ACCOUNT_ALREADY_EXIST);
            }
        }

        internal UserItem GetFromUsername(string user, DatabaseConnection dbconnect)
        {
            return dbconnect.GetFromUserName(user);
        }
    }
}