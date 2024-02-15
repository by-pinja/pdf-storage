namespace Pdf.Storage.Hangfire;

public class HangfireConfiguration
{
    public string? DashboardUser { get; set; }

    public string? DashboardPassword { get; set; }

    // ["*"] means all ip addresses are allowed.
    public string[] AllowedIpAddresses { get; set; } = [];
}
