using Bastocos.Business.Database.User;
using Bastocos.Entity.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bastocos.Entity.Admin.Database
{
    public class DatabaseConnection
    {
        public UserDatabaseConnection UserDatabaseConnection { get; set; } = new UserDatabaseConnection();

        public DatabaseConnection()
        {
        }

        internal void Account_Create(UserItem account)
        {
            throw new NotImplementedException();
        }

        internal bool Account_Exist(int accountId)
        {
            throw new NotImplementedException();
        }

        internal UserItem GetFromUserName(string user)
        {
            return UserDatabaseConnection.GetFromUserName(user);
        }
    }
}