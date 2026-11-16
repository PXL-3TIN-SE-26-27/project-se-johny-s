using CampusBook.Api.Data;

namespace CampusBook.Api.Services;

public class StatsService
{
    private static readonly Dictionary<string, int> confirmations = new Dictionary<string, int>();
    private static decimal confirmedTotal = 0;

    private readonly IReservationRepository _repo;

    public StatsService(IReservationRepository repo)
    {
        _repo = repo;
    }

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
        var thisMonth = _repo.GetByDateRange(monthStart, monthStart.AddMonths(1));

        return new
        {
            byStatus = _repo.CountByStatus(),
            reservationsThisMonth = thisMonth.Count,
            revenueThisMonth = thisMonth.Where(r => r.Status != "Cancelled").Sum(r => r.Price),
            confirmationsSinceStartup = confirmations,
            confirmedAmountSinceStartup = confirmedTotal
        };
    }
}
