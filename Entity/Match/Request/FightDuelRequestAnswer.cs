using Bastocos.Entity.User;

namespace BastocosR2.Entity.Match.Request
{
    internal class FightDuelRequestAnswer
    {
        public UserItem UserItem { get; set; } = new UserItem();
        public string Requester { get; set; } = String.Empty;
        public bool Accepted { get; set; }
    }
}