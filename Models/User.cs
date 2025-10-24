using System;

namespace RTMAuthServer.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } = "user";

        public DateTime? LastLogin { get; set; }
        public int LoginCount { get; set; } = 0;

        public string? Token { get; set; }
    }
}
