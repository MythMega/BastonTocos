using Bastocos.Business.Database.User;
using Bastocos.Entity.Admin;
using Bastocos.Entity.Stuffs.Trashs;
using Bastocos.Entity.User;
using BastocosR2.Business.Database.Entities;
using BastocosR2.Entity.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BastocosR2.Business.User
{
    public class UserBusiness
    {
        private UserDatabaseConnection _userConnection = new();
        private ItemsDatabaseConnection _itemsConnection = new();

        public string SellGoods(UserItem seller, EnvItem env)
        {
            string result = String.Empty;
            UserItem FullUser = _userConnection.GetFullUserData(seller.Id);
            List<ItemEntrie> items = _itemsConnection.GetAllItemsFromUser(seller);
            List<ItemEntrie> toDelete = [];
            int countMoney = FullUser.StatisticsItem.Money;
            int count = 0;
            foreach (ItemEntrie item in items)
            {
                Trashitem? trash = env.Items.Trashitems.FirstOrDefault(a => a.Name == item.Item);
                if (trash is not null)
                {
                    toDelete.Add(item);
                    FullUser.StatisticsItem.Money += trash.SellValue * item.Count;
                    FullUser.StatisticsItem.MoneyEarned += trash.SellValue * item.Count;
                    count++;
                }
            }
            countMoney = FullUser.StatisticsItem.Money - countMoney;
            _itemsConnection.RemoveEntries(toDelete);
            _userConnection.SetFullUserData(FullUser);

            result = count > 0 ? $"{FullUser.Name} a vendu {count} items pour {countMoney}." : $"{FullUser.Name} n'a rien a vendre.";
            return result;
        }
    }
}