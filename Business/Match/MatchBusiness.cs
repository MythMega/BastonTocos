using Bastocos.Business.Database.User;
using Bastocos.Entity;
using Bastocos.Entity.Admin;
using Bastocos.Entity.Admin.Settings;
using Bastocos.Entity.Match;
using Bastocos.Entity.Match.Assault;
using Bastocos.Entity.Match.Fight;
using Bastocos.Entity.Match.Request;
using Bastocos.Entity.User;
using BastocosR2.Entity.Match.Request;
using BastocosR2.Entity.Web;
using BastocosR2.Tools;
using System.Text.Json;

namespace Bastocos.Business.Match
{
    internal class MatchBusiness
    {
        private UserDatabaseConnection userDb = new UserDatabaseConnection();
        private DefaultElements _defaultElements = new DefaultElements();
        private AttackBusiness _attackBusiness = new AttackBusiness();
        private LootBusiness _lootBusiness = new LootBusiness();

        public string isMatchValid(EnvItem envItem, FightRequest request)
        {
            envItem.RefreshLastActivity(request.UserItemA);
            request.UserToDuel = request.UserToDuel.Replace("@", "");
            if (request.UserToDuel == request.UserItemA.Name)
            {
                return "Tu ne peux pas t'assaut toi même gros con";
            }
            request.UserItemB = userDb.GetFromUserName(request.UserToDuel);
            if (request.UserItemB == null)
            {
                return "Utilisateur non trouvé";
            }
            if (!envItem.IsStillActive(request.UserItemB))
            {
                return $"L'utilisateur {request.UserItemB.Name} ne semble pas être actif.";
            }
            if (envItem.FightQueue.Any(a =>
            (a.RequestStatut == RequestStatut.Running || a.RequestStatut == RequestStatut.In_Queue) &&
            a.UserItemA.Id == request.UserItemA.Id))
            {
                return request.UserItemA.Name + " est déjà en attente d'un combat / en combat";
            }
            if (envItem.FightQueue.Any(a =>
            (a.RequestStatut == RequestStatut.Running || a.RequestStatut == RequestStatut.In_Queue) &&
            a.UserItemB!.Name.Equals(request.UserToDuel, StringComparison.CurrentCultureIgnoreCase)))
            {
                return request.UserItemB.Name + " est déjà en attente d'un combat / en combat";
            }
            return String.Empty;
        }

        public string InitializeAssault(EnvItem envItem, FightAssaultRequest request)
        {
            string result = isMatchValid(envItem, request);
            if (result == null || result.Trim() == "")
            {
                request.RequestStatut = RequestStatut.In_Queue;
                FightRequest fightRequest = request;
                fightRequest.FightType = FightType.Assault;
                envItem.FightQueue.Add(request);
                return $"Combat ajouté a la liste d'attente {envItem.FightQueue.Where(f => f.RequestStatut == RequestStatut.In_Queue).Count()}";
            }
            else
                return result;
        }

        internal string InitializeDuelRequest(EnvItem env, FightDuelRequest duelrequest)
        {
            string result = isMatchValid(env, duelrequest);
            if (result == null)
            {
                duelrequest.RequestStatut = RequestStatut.Pending_Answer;
                FightRequest fightRequest = duelrequest;
                fightRequest.FightType = FightType.Duel;
                env.FightQueue.Add(duelrequest);
                return $"{duelrequest.UserToDuel} a été duel par {duelrequest.UserItemA.Name}. @{duelrequest.UserToDuel}, pour accepter, faire \"!yduel {duelrequest.UserItemA.Name}\" ; pour refuser faire \"!nduel {duelrequest.UserItemA.Name}\"";
            }
            else
                return result;
        }

        internal string AnswerDuelRequest(EnvItem env, FightDuelRequestAnswer answer)
        {
            if (answer.UserItem.Name.ToLower() == answer.Requester.ToLower())
            {
                return "Tu ne peux pas accepter un duel de toi meme";
            }
            if (!env.FightQueue.Any(w => w.FightType == FightType.Duel && w.RequestStatut == RequestStatut.Pending_Answer))
            {
                return "Tu n'as aucune requête de duel en attente";
            }
            if (!env.FightQueue.Any(w => w.FightType == FightType.Duel &&
            w.RequestStatut == RequestStatut.Pending_Answer &&
            w.UserItemA.Name.ToLower() == answer.Requester.ToLower() &&
            w.UserItemB!.Name.ToLower() == answer.Requester.ToLower()))
            {
                return $"Tu n'as pas de requête de {answer.Requester} en attente.";
            }

            // si aucun soucis : alors on prend en compte la requete :
            FightRequest? req = env.FightQueue.FirstOrDefault(w => w.FightType == FightType.Duel &&
                w.RequestStatut == RequestStatut.Pending_Answer &&
                w.UserItemA.Name.ToLower() == answer.Requester.ToLower() &&
                w.UserItemB!.Name.ToLower() == answer.Requester.ToLower());

            if (req is null)
            {
                return "answered match not found after check";
            }
            else
            {
                if (answer.Accepted)
                {
                    req.RequestStatut = RequestStatut.In_Queue;
                    req.DateRequest = DateTime.Now;
                    return "La requête a bien été acceptée.";
                }
                else
                {
                    req.RequestStatut = RequestStatut.Duel_Denied;
                    return "La requête a bien été refusée.";
                }
            }
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
            if (futurematch.UserItemB is null)
                throw new Exception(ErrorItem.ERROR_ACCOUNT_ALREADY_EXIST);
            globalEnvironmentItem.CurrentMatch = new FightItem
            {
                FighterA = new Fighter
                {
                    User = futurematch.UserItemA,
                    HP_Current = HP,
                    HP_Max = HP,
                    ArmorItem = _defaultElements.DefaultArmorItem,
                    WeaponItem = _defaultElements.DefaultWeaponItem,
                },
                FighterB = new Fighter
                {
                    User = futurematch.UserItemB,
                    HP_Current = HP,
                    HP_Max = HP,
                    ArmorItem = _defaultElements.DefaultArmorItem,
                    WeaponItem = _defaultElements.DefaultWeaponItem,
                },
                FightStats = new FightStats
                {
                    LastAction = DateTime.Now,
                    StartTime = DateTime.Now,
                }
            };
        }

        internal string SearchForLoot(EnvItem globalEnvironmentItem, SettingsItem globalSettingsItems, UserItem userWhoLoot)
        {
            return _lootBusiness.SearchForLoot(globalEnvironmentItem, globalSettingsItems, userWhoLoot);
        }

        internal string Attack(EnvItem env, SettingsItem settings, UserItem attacker)
        {
            return _attackBusiness.Attack(env, settings, attacker);
        }

        internal string GetQueue(EnvItem globalEnvironmentItem)
        {
            List<FightRequest> datas = globalEnvironmentItem.FightQueue.Where(a => a.RequestStatut == RequestStatut.In_Queue).ToList();
            List<WebQueueWebData> result = [];
            datas.ForEach(a =>
            {
                result.Add(new WebQueueWebData
                {
                    MatchType = a.FightType == FightType.Assault ? "Assault" : "Duel",
                    RequestTime = a.DateRequest,
                    UsernameA = a.UserItemA.Name,
                    UsernameB = a.UserItemB is null ? a.UserToDuel : a.UserItemB.Name,
                });
            });
            return JsonSerializer.Serialize(result);
        }

        public string RemoveQueueItem(EnvItem env, WebQueueWebData itemToRemove)
        {
            List<FightRequest> queue = [.. env.FightQueue.Where(fq => fq.UserItemA.Name == itemToRemove.UsernameA &&
            fq.UserItemB is not null && fq.UserItemB.Name == itemToRemove.UsernameB)];

            if (queue.Count == 1)
            {
                queue.First().RequestStatut = RequestStatut.Canceled_By_Admin;
                return "Item annulé avec succés.";
            }
            else
            {
                if(queue.Count == 0)
                {
                    return "Aucun item correspondant dans la liste.";
                }
                else
                {
                    return "Plusieurs items correspondant a la liste";
                }
            }
        }
    }
}