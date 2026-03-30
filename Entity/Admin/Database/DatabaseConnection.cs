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
        public string ConnectionString { get; set; } = "./database.sqlite";

        public DatabaseConnection()
        {
            if()
        }

        internal void Account_Create(UserItem account)
        {
            throw new NotImplementedException();
        }

        internal object Account_Exist(int accountId)
        {
            throw new NotImplementedException();
        }
    }
}