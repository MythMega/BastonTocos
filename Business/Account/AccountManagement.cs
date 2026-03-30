using Bastocos.Entity;
using Bastocos.Entity.Admin.Database;
using Bastocos.Entity.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bastocos.Business.Account
{
    public static class AccountManagement
    {
        public static bool AccountExist(int accountId, DatabaseConnection dbconnect)
        {
            return dbconnect.Account_Exist(accountId) != null;
        }

        public static void CreateAccount(UserItem account, DatabaseConnection dbconnect)
        {
            if (!AccountExist(account.Id, dbconnect))
            {
                dbconnect.Account_Create(account);
            }
            else
            {
                throw new Exception(ErrorItem.ERROR_ACCOUNT_ALREADY_EXIST);
            }
        }
    }
}