namespace Bastocos.Entity.User
{
    public class LoginStatsItem
    {
        public DateTime FirstLogin { get; set; } = DateTime.Now;
        public DateTime LastLogin { get; set; } = DateTime.Now;
    }
}