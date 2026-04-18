using Bastocos.Business.Match;
using Bastocos.Entity.Admin.Database;
using Bastocos.Entity.Cards;
using Bastocos.Entity.Match.Assault;
using Bastocos.Entity.Match.Request;
using Bastocos.Entity.Stuffs.Equipments.Armors;
using Bastocos.Entity.Stuffs.Equipments.Weapons;
using Bastocos.Entity.Stuffs.Trashs;
using Bastocos.Entity.User;
using BastocosR2.Business.Web;
using BastocosR2.Entity.Web;
using BastocosR2.Tools.Json;
using System.Text.Json;

namespace Bastocos.Entity.Admin
{
    public class EnvItem
    {
        private readonly WebExporterBusiness _webExporterBusiness = new WebExporterBusiness();
        public List<CardItem> CardItems { get; set; } = new List<CardItem>();

        public ItemSet Items { get; set; } = new ItemSet();

        public DatabaseConnection Dbconnect { get; set; } = new DatabaseConnection();

        public List<(UserItem Utilisateur, bool Modified, DateTime LastActivity)> Users { get; set; } = [];

        public List<FightRequest> FightQueue { get; set; } = new List<FightRequest>();

        public FightItem? CurrentMatch { get; set; }

        public DateTime LastMatchEnd { get; set; } = DateTime.Now;

        public Random RandomGenerator { get; set; } = new Random();

        public int SessionCommitCount { get; set; } = 1;
        public DateTime SessionStartDate { get; set; } = DateTime.Now;

        public void RefreshLastActivity(UserItem user)
        {
            int index = Users.FindIndex(u => u.Utilisateur.Id == user.Id);

            if (index >= 0)
            {
                var (Utilisateur, Modified, LastActivity) = Users[index];
                Users[index] = (Utilisateur, Modified, DateTime.Now);
            }
            else
            {
                Users.Add((user, true, DateTime.Now));
            }
        }

        internal bool IsStillActive(UserItem user)
        {
            int index = Users.FindIndex(u => u.Utilisateur.Id == user.Id);

            if (index < 0)
                return false;

            return Users[index].LastActivity.AddMinutes(45) > DateTime.Now;
        }

        public void LoadAllData()
        {
            LoadOneData("CARD");
            LoadOneData("TRASH");
            LoadOneData("ARMOR");
            LoadOneData("WEAPON");
            //Items.ArmorItems.ForEach(a => Console.WriteLine(a.Image.Split('/').Last()));
            //Items.WeaponItems.ForEach(a => Console.WriteLine(a.Image.Split('/').Last()));
            //Console.WriteLine("////////////////");
            //Items.Trashitems.ForEach(a => Console.WriteLine(a.Name));
            //CardItems.ForEach(a => Console.WriteLine(a.Name));
        }

        public void LoadOneData(string type)
        {
            Dictionary<string, string> item = new Dictionary<string, string>
            {
                { "CARD", "Carte" },
                { "TRASH", "Déchet" },
                { "ARMOR", "Armure" },
                { "WEAPON", "Arme" },
            };

            string folderPath = "";
            switch (type)
            {
                case "CARD":
                    folderPath = "./Elements/Cards/"; break;
                case "TRASH":
                    folderPath = "./Elements/Stuffs/Trashs/"; break;
                case "ARMOR":
                    folderPath = "./Elements/Stuffs/Defensives/"; break;
                case "WEAPON":
                    folderPath = "./Elements/Stuffs/Offensives/"; break;
            }

            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Console.WriteLine("Dossier introuvable : " + folderPath);
                    return;
                }

                var jsonFiles = Directory.GetFiles(folderPath, "*.json");

                foreach (var file in jsonFiles)
                {
                    try
                    {
                        string fileName = Path.GetFileName(file);
                        Console.WriteLine("Fichier " + item[type] + " détecté : " + fileName);

                        string json = File.ReadAllText(file);
                        int count = 0;

                        switch (type)
                        {
                            case "CARD":
                                List<CardItem> cards = JsonSerializer.Deserialize<List<CardItem>>(json);

                                if (cards == null || cards.Count == 0)
                                {
                                    Console.WriteLine("⚠️ Aucune carte valide dans " + fileName);
                                    continue;
                                }

                                count = cards.Count;
                                CardItems.AddRange(cards);
                                break;

                            case "TRASH":
                                List<Trashitem> trashs = JsonSerializer.Deserialize<List<Trashitem>>(json);

                                if (trashs == null || trashs.Count == 0)
                                {
                                    Console.WriteLine("⚠️ Aucun déchet valide dans " + fileName);
                                    continue;
                                }

                                count = trashs.Count;
                                Items.Trashitems.AddRange(trashs);
                                break;

                            case "WEAPON":
                                List<WeaponItem> weapons = JsonSerializer.Deserialize<List<WeaponItem>>(json);

                                if (weapons == null || weapons.Count == 0)
                                {
                                    Console.WriteLine("⚠️ Aucune arme valide dans " + fileName);
                                    continue;
                                }

                                count = weapons.Count;
                                Items.WeaponItems.AddRange(weapons);
                                break;

                            case "ARMOR":
                                List<ArmorItem> armors = JsonSerializer.Deserialize<List<ArmorItem>>(json);

                                if (armors == null || armors.Count == 0)
                                {
                                    Console.WriteLine("⚠️ Aucune armure valide dans " + fileName);
                                    continue;
                                }

                                count = armors.Count;
                                Items.ArmorItems.AddRange(armors);
                                break;
                        }

                        Console.WriteLine("Fichier " + item[type] + " chargé : " + fileName + " (" + count + " " + item[type] + ")");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("❌ Erreur lors du chargement du fichier " + file + " : " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Erreur critique dans LoadOneItem (" + item[type] + ") : " + ex.Message);
            }
        }

        internal void ExportWebData()
        {
            WebFullElementData datas = new WebFullElementData
            {
                Armors = this.Items.ArmorItems,
                Weapons = this.Items.WeaponItems,
                Trashs = this.Items.Trashitems,
                Cards = this.CardItems,
            };
            _webExporterBusiness.ExportFileAbsolute(JsonSerializer.Serialize(this), Path.Combine(Directory.GetCurrentDirectory(), "Web", "Data"), "fulldata.json");
            _webExporterBusiness.ExportFileAbsolute(JsonSerializer.Serialize(datas.Armors), Path.Combine(Directory.GetCurrentDirectory(), "Web", "Data"), "armors.json");
            _webExporterBusiness.ExportFileAbsolute(JsonSerializer.Serialize(datas.Weapons), Path.Combine(Directory.GetCurrentDirectory(), "Web", "Data"), "weapons.json");
            _webExporterBusiness.ExportFileAbsolute(JsonSerializer.Serialize(datas.Cards), Path.Combine(Directory.GetCurrentDirectory(), "Web", "Data"), "cards.json");
            _webExporterBusiness.ExportFileAbsolute(JsonSerializer.Serialize(datas.Trashs), Path.Combine(Directory.GetCurrentDirectory(), "Web", "Data"), "trashs.json");
        }

        internal string GetActivesUsers()
        {
            List<ActiveUserWebData> datas = [];
            Users.ForEach(a =>
            {
                datas.Add(new ActiveUserWebData
                {
                    DateInactive = a.LastActivity.AddMinutes(45),
                    Id = a.Utilisateur.Id,
                    Name = a.Utilisateur.Name,
                });
            });
            return JsonSerializer.Serialize(datas);
        }
    }
}