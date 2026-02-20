using DigitalniCjenik.Security;
using Microsoft.Extensions.Options;

namespace DigitalniCjenik.Services
{
    public class MockLdapService : ILdapService
    {
        private readonly ILogger<MockLdapService> _logger;
        private readonly LdapSettings _ldapSettings;

        // Mock baza korisnika
        private readonly Dictionary<string, (string Password, LdapUserInfo User, int UlogaID)> _mockUsers = new()
        {
            ["davor"] = (
                "test123",
                new LdapUserInfo
                {
                    Username = "davor",
                    Email = "davor@company.local",
                    ImePrezime = "Davor Horvat",
                    FirstName = "Davor",
                    LastName = "Horvat"
                },
                 3
            ),
            ["ivana"] = (
                "test456",
                new LdapUserInfo
                {
                    Username = "ivana",
                    Email = "ivana@company.local",
                    ImePrezime = "Ivana Kovač",
                    FirstName = "Ivana",
                    LastName = "Kovač"
                },
                3
            ),
            ["admin"] = (
                "admin123",
                new LdapUserInfo
                {
                    Username = "admin",
                    Email = "admin@company.local",
                    ImePrezime = "Admin User",
                    FirstName = "Admin",
                    LastName = "User"
                }, 1
            ),
            ["marko"] = (
                "marko123",
                new LdapUserInfo
                {
                    Username = "marko",
                    Email = "marko@company.local",
                    ImePrezime = "Marko Marić",
                    FirstName = "Marko",
                    LastName = "Marić"
                }, 2
            ),
            ["ana"] = (
                "ana123",
                new LdapUserInfo
                {
                    Username = "ana",
                    Email = "ana@company.local",
                    ImePrezime = "Ana Jurić",
                    FirstName = "Ana",
                    LastName = "Jurić"
                }, 3
            )
        };

        public MockLdapService(ILogger<MockLdapService> logger, IOptions<LdapSettings> ldapOptions)
        {
            _logger = logger;
            _ldapSettings = ldapOptions.Value;
        }

        public Task<LdapUserInfo?> AuthenticateAsync(string username, string password)
        {
            var cleanUsername = CleanUsername(username);
            _logger.LogInformation("Mock LDAP login attempt for {Username}", cleanUsername);

            // Provjeri da li korisnik postoji u mock bazi
            if (_mockUsers.TryGetValue(cleanUsername, out var mockUser))
            {
                // Provjeri lozinku
                if (mockUser.Password == password)
                {
                    _logger.LogInformation("Mock LDAP login successful for {Username}", cleanUsername);
                    return Task.FromResult<LdapUserInfo?>(mockUser.User);
                }
            }

            _logger.LogWarning(" Mock LDAP login failed for {Username}", cleanUsername);
            return Task.FromResult<LdapUserInfo?>(null);
        }

        public int? GetUlogaIdForUser(string username)
        {
            var cleanUsername = CleanUsername(username);
            if (_mockUsers.TryGetValue(cleanUsername, out var mockUser))
            {
                return mockUser.UlogaID;
            }
            return null;
        }

        public bool IsLdapUser(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;

            // Prepoznajemo LDAP korisnike po domeni @company.local
            return email.EndsWith($"@{_ldapSettings.Domain}") ||
                   email.Contains("\\");
        }

        private string CleanUsername(string username)
        {
            if (username.Contains('\\'))
                return username.Split('\\')[1];

            if (username.Contains('@'))
                return username.Split('@')[0];

            return username;
        }
    }
}
