using Bastocos.Tools;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BastocosR2.Business.Database.Entities
{
    public static class BastoDB
    {
        public static SqliteConnection GetConnection()
        {
            string a = GlobalVar.ConnectionString;
            var conn = new SqliteConnection(a);
            conn.Open();
            return conn;
        }
    }
}