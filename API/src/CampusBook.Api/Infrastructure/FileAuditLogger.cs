namespace CampusBook.Api.Infrastructure;

public class FileAuditLogger
{
    public void Log(string action, int reservationId, int userId, string extra = "")
    {
        var line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "|" + action + "|" + reservationId + "|" + userId + "|" + extra;
        try
        {
            File.AppendAllText(AppConfig.Instance.AuditLogPath, line + Environment.NewLine);
        }
        catch
        {
        }
    }
}
