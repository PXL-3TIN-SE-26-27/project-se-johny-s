using CampusBook.Api.Data;

namespace CampusBook.Api.Services;

public class StatsService(IReservationRepository repo)
{
    private static readonly Dictionary<string, int> confirmations = new Dictionary<string, int>();
    private static decimal confirmedTotal = 0;

    public static void RegisterConfirmation(string type, decimal amount)
    {
        if (!confirmations.ContainsKey(type))
        {
            confirmations[type] = 0;
        }
        confirmations[type]++;
        confirmedTotal += amount;
    }

    public object GetDashboard()
    {
        var now = DateTime.Now;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var thisMonth = repo.GetByDateRange(monthStart, monthStart.AddMonths(1));

        return new
        {
            byStatus = repo.CountByStatus(),
            reservationsThisMonth = thisMonth.Count,
            revenueThisMonth = thisMonth.Where(r => r.Status != "Cancelled").Sum(r => r.Price),
            confirmationsSinceStartup = confirmations,
            confirmedAmountSinceStartup = confirmedTotal
        };
    }
}
