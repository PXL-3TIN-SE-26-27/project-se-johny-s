using System.Globalization;
using System.Text;
using CampusBook.Api.Data;

namespace CampusBook.Api.Services;

public class ExportService(IReservationRepository repo)
{
    public string ToCsv(int userId)
    {
        var sb = new StringBuilder();
        sb.AppendLine("id;resource;start;end;status;price");
        foreach (var r in repo.GetByUser(userId))
        {
            sb.AppendLine(string.Join(";",
                r.Id,
                Escape(r.Resource?.Name),
                r.Start.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                r.End.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                r.Status,
                r.Price.ToString(CultureInfo.InvariantCulture)));
        }
        return sb.ToString();
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(';') || value.Contains('"'))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
        return value;
    }
}
