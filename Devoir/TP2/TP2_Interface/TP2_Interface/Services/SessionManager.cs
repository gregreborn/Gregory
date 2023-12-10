namespace TP2_Interface.Services
{
    public static class SessionManager
    {
        public static User CurrentUser { get; set; }

    }

    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public bool IsAdmin { get; set; }
        public string PostgresUsername { get; set; }
        public string PostgresPassword { get; set; }

    }
}