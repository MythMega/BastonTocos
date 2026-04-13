using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BastocosR2.Entity.Web
{
    public class ActiveUserWebData
    {
        public int Id { get; set; }
        public string Name { get; set; } = String.Empty;
        public DateTime DateInactive { get; set; } = DateTime.Now;
    }
}