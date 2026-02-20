namespace DigitalniCjenik.Security
{
    public class LdapSettings
    {
        public string Domain { get; set; } = "company.local";
        public bool EnableLdap { get; set; } = true;
        public bool UseMock { get; set; } = true; 
    }
}
