using Bastocos.Business.Database.User;
using Bastocos.Entity.Admin;
using Bastocos.Entity.Admin.Settings;
using Bastocos.Entity.Cards;
using Bastocos.Entity.Stuffs.Equipments.Armors;
using Bastocos.Entity.Stuffs.Equipments.Weapons;
using Bastocos.Entity.Stuffs.Trashs;
using Bastocos.Entity.User;
using BastocosR2.Business.Database.Entities;
using BastocosR2.Entity.Database;
using BastocosR2.Entity.Web;
using BastocosR2.Entity.Web.UserWebExportAlbum;
using System.Text.Json;

namespace BastocosR2.Business.Web
{
    public class UserDataExporter
    {
        private UserDatabaseConnection _userDB = new UserDatabaseConnection();
        private CardDatabaseConnection _cardDB = new CardDatabaseConnection();
        private ItemsDatabaseConnection _itemDB = new ItemsDatabaseConnection();
        private WebExporterBusiness _webExporter = new WebExporterBusiness();

        public void ExportAllUserDataJson(EnvItem env, SettingsItem settings)
        {
            List<UserItem> users = _userDB.GetAllFullUserData();
            List<CardEntrie> cards = _cardDB.GetAllItems();
            List<ItemEntrie> items = _itemDB.GetAllItems();

            List<UserWebData> userWebDatas = [];

            List<string> names = [];

            // UsersData/user.json

            users.ForEach(user =>
            {
                userWebDatas.Add(new UserWebData()
                {
                    User = user,
                    Cards = CardEntriesToCardItems(env, cards.Where(a => a.Id == user.Id).ToList()),
                    Armors = ArmorEntriesToArmorItems(env, items.Where(a => a.Id == user.Id).ToList()),
                    Weapons = WeaponEntriesToWeaponItems(env, items.Where(a => a.Id == user.Id).ToList()),
                    Trashs = TrashEntriesToTrashItems(env, items.Where(a => a.Id == user.Id).ToList()),
                });
                names.Add(user.Name);
            });

            userWebDatas.ForEach(userWebData =>
            {
                _webExporter.ExportFileAbsolute(JsonSerializer.Serialize(userWebData), Path.Combine(Directory.GetCurrentDirectory(), "Web", "UsersData"), $"{userWebData.User.Name}.json");
            });

            // users.json

            _webExporter.ExportFileAbsolute(JsonSerializer.Serialize(names), Path.Combine(Directory.GetCurrentDirectory(), "Web"), $"users.json");

            // globaldata.json
            _webExporter.ExportFileAbsolute(JsonSerializer.Serialize(new GlobalDataWebData
            {
                ArmorCount = env.Items.ArmorItems.Count,
                TrashCount = env.Items.Trashitems.Count,
                WeaponCount = env.Items.WeaponItems.Count,
                CardCount = env.CardItems.Count
            }
            ), Path.Combine(Directory.GetCurrentDirectory(), "Web"), $"globaldata.json");
        }

        private List<ArmorAlbumEntrie> ArmorEntriesToArmorItems(EnvItem env, List<ItemEntrie> armorEntrie)
        {
            List<ArmorAlbumEntrie> result = [];
            foreach (var item in armorEntrie)
            {
                ArmorItem? corresp = env.Items.ArmorItems.FirstOrDefault(a => a.Name.ToLower() == item.Item.ToLower());
                if (corresp != null)
                {
                    result.Add(new ArmorAlbumEntrie()
                    {
                        Name = item.Item,
                        Count = item.Count,
                        Description = corresp.Description,
                        Image = corresp.Image,
                        Defense = corresp.Defense
                    });
                }
            }
            return result;
        }

        private List<WeaponAlbumEntrie> WeaponEntriesToWeaponItems(EnvItem env, List<ItemEntrie> weaponEntrie)
        {
            List<WeaponAlbumEntrie> result = [];
            foreach (var item in weaponEntrie)
            {
                WeaponItem? corresp = env.Items.WeaponItems.FirstOrDefault(a => a.Name.ToLower() == item.Item.ToLower());
                if (corresp != null)
                {
                    result.Add(new WeaponAlbumEntrie()
                    {
                        Name = item.Item,
                        Count = item.Count,
                        Description = corresp.Description,
                        Image = corresp.Image,
                        Attack = corresp.Attack
                    });
                }
            }
            return result;
        }

        private List<TrashAlbumEntrie> TrashEntriesToTrashItems(EnvItem env, List<ItemEntrie> trashEntrie)
        {
            List<TrashAlbumEntrie> result = [];
            foreach (var item in trashEntrie)
            {
                Trashitem? corresp = env.Items.Trashitems.FirstOrDefault(a => a.Name.ToLower() == item.Item.ToLower());
                if (corresp != null)
                {
                    result.Add(new TrashAlbumEntrie()
                    {
                        Name = item.Item,
                        Count = item.Count,
                        Description = corresp.Description,
                        Image = corresp.Image,
                        SellValue = corresp.SellValue
                    });
                }
            }
            return result;
        }

        private List<CardAlbumEntrie> CardEntriesToCardItems(EnvItem env, List<CardEntrie> cardEntries)
        {
            List<CardAlbumEntrie> result = [];
            foreach (var item in cardEntries)
            {
                CardItem? corresp = env.CardItems.FirstOrDefault(a => a.Name.ToLower() == item.Item.ToLower());
                if (corresp != null)
                {
                    result.Add(new CardAlbumEntrie()
                    {
                        BuyValue = corresp.BuyValue,
                        Name = item.Item,
                        CardRarity = corresp.CardRarity,
                        Count = item.Count,
                        Description = corresp.Description,
                        Image = corresp.Image,
                        SellValue = corresp.SellValue
                    });
                }
            }
            return result;
        }
    }
}