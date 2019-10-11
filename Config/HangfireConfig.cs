namespace Pdf.Storage.Config
{
    public class HangfireConfig
    {
        // If empty, only localhost is allowed.
        // To allow any ip addreess add "*".
        public string[] AllowedIpAddresses { get; set; } = new string[] {};

        // If username and password is set, then authentication for hangfire is enabled.
        public string DashboardUser { get; set; }
        public string DashboardPassword { get; set; }
    }
}