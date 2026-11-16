using CampusBook.Api.Infrastructure;
using CampusBook.Api.Models;
using CampusBook.Api.Services;

namespace CampusBook.Tests;

public class PricingTests
{
    private static User Student()
    {
        return new User { Id = 1, Name = "Test Student", Email = "test@student.campusbook.local", Role = "student" };
    }

    private static Resource Laptop()
    {
        return new Resource { Id = 3, Name = "Laptop L-01", Type = "laptop", HourlyRate = 4m, DayRate = 25m };
    }

    [Fact]
    public void Price_Student_Laptop_TwoHours()
    {
        var calc = new PriceCalculator();
        var start = new DateTime(2030, 1, 8, 10, 0, 0);

        var price = calc.Calculate(Laptop(), Student(), start, start.AddHours(2), false, false, false, false, false);

        Assert.Equal(8.00m, price);
    }

    [Fact]
    public void WeekendSurcharge_Applied()
    {
        var calc = new PriceCalculator();
        var start = DateTime.Now;

        var price = calc.Calculate(Laptop(), Student(), start, start.AddHours(2), false, false, false, false, false);

        Assert.Equal(10.00m, price);
    }

    [Fact]
    public void CampaignDiscount_Applied()
    {
        AppConfig.Instance.CurrentCampaignDiscount = 0.10m;
        AppConfig.Instance.CampaignEndsAt = DateTime.Now.AddDays(1);
        var calc = new PriceCalculator();
        var start = new DateTime(2030, 1, 8, 10, 0, 0);

        var price = calc.Calculate(Laptop(), Student(), start, start.AddHours(2), false, false, false, false, false);

        Assert.Equal(7.20m, price);
    }
}
