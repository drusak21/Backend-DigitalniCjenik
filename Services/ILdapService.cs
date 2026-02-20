namespace DigitalniCjenik.Services
{
    public interface ILdapService
    {
        Task<LdapUserInfo?> AuthenticateAsync(string username, string password);
        bool IsLdapUser(string email);
        int? GetUlogaIdForUser(string username);
    }

    public class LdapUserInfo
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? ImePrezime { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
