using System.Text.Json;

namespace CampusBook.Api.Infrastructure;

public class AppConfig
{
    private static AppConfig _instance;

    public static AppConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Load();
            }
            return _instance;
        }
    }

    public string SmtpFrom { get; set; } = "noreply@campusbook.local";
    public string MailPickupDirectory { get; set; } = "mail-out";
    public string AuditLogPath { get; set; } = "audit.log";
    public string MerchantId { get; set; } = "CAMPUS-0042";
    public decimal CurrentCampaignDiscount { get; set; } = 0m;
    public DateTime CampaignEndsAt { get; set; } = DateTime.MinValue;
    public decimal WeekendSurcharge { get; set; } = 0.25m;
    public int MaxHours { get; set; } = 72;
    public string AdminEmail { get; set; } = "facilities@campusbook.local";

    private static AppConfig Load()
    {
        var path = "campus.config.json";
        if (!File.Exists(path))
        {
            path = Path.Combine(AppContext.BaseDirectory, "campus.config.json");
        }
        if (File.Exists(path))
        {
            try
            {
                var cfg = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(path));
                if (cfg != null) return cfg;
            }
            catch
            {
            }
        }
        return new AppConfig();
    }

    public void Reload()
    {
        _instance = Load();
    }
}
