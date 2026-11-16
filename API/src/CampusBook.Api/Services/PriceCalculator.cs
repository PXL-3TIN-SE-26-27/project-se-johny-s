using CampusBook.Api.Infrastructure;
using CampusBook.Api.Models;

namespace CampusBook.Api.Services;

public class PriceCalculator
{
    public decimal Calculate(Resource resource, User user, DateTime start, DateTime end,
        bool insurance, bool delivery, bool technician, bool cleaning, bool extendedReturn)
    {
        decimal price = 0;
        var hours = (decimal)Math.Ceiling((end - start).TotalHours);
        var days = (decimal)Math.Ceiling((end - start).TotalDays);

        if (hours <= 0)
        {
            throw new ArgumentException("Invalid period");
        }

        if (user.Role == "staff")
        {
            return 0;
        }
        else if (user.Role == "student")
        {
            switch (resource.Type)
            {
                case "laptop":
                    price = hours * resource.HourlyRate;
                    if (hours > 24)
                    {
                        price = Math.Min(price, days * resource.DayRate);
                    }
                    break;
                case "camera":
                    price = hours * resource.HourlyRate;
                    if (hours > 8)
                    {
                        price = Math.Min(price, days * resource.DayRate);
                    }
                    break;
                case "printer":
                    price = hours * resource.HourlyRate + 2.50m;
                    break;
                case "room":
                    price = hours * resource.HourlyRate;
                    break;
                default:
                    throw new InvalidOperationException("Unknown resource type " + resource.Type);
            }
        }
        else if (user.Role == "association")
        {
            switch (resource.Type)
            {
                case "room":
                    price = days * resource.DayRate;
                    if (start.DayOfWeek != DayOfWeek.Saturday && start.DayOfWeek != DayOfWeek.Sunday)
                    {
                        price = price * 0.8m;
                    }
                    break;
                case "laptop":
                case "camera":
                    price = days * resource.DayRate;
                    break;
                case "printer":
                    price = hours * resource.HourlyRate + 2.50m;
                    break;
                default:
                    throw new InvalidOperationException("Unknown resource type " + resource.Type);
            }
        }
        else
        {
            throw new InvalidOperationException("Unknown role " + user.Role);
        }

        if (end.DayOfWeek == DayOfWeek.Saturday || end.DayOfWeek == DayOfWeek.Sunday)
        {
            price = price + price * AppConfig.Instance.WeekendSurcharge;
        }

        if (insurance)
        {
            if (resource.Type == "camera" || resource.Type == "laptop")
            {
                price = price + price * 0.10m;
            }
            else
            {
                price = price + 5m;
            }
        }

        if (delivery && !insurance)
        {
            if (resource.Type == "room")
            {
                throw new InvalidOperationException("Rooms cannot be delivered");
            }
            price = price + 15m;
        }
        else if (delivery && insurance)
        {
            if (resource.Type == "room")
            {
                throw new InvalidOperationException("Rooms cannot be delivered");
            }
            price = price + 10m;
        }

        if (technician && cleaning)
        {
            price = price + 30m * hours;
        }
        else if (technician)
        {
            price = price + 30m * hours;
        }
        else if (cleaning)
        {
            if (resource.Type == "room")
            {
                price = price + 25m;
            }
            else
            {
                price = price + 8m;
            }
        }

        if (extendedReturn)
        {
            if (delivery)
            {
                price = price + 5m;
            }
            else if (technician)
            {
                price = price + 0m;
            }
            else
            {
                price = price + 7.5m;
            }
        }

        if (AppConfig.Instance.CurrentCampaignDiscount > 0 && DateTime.Now < AppConfig.Instance.CampaignEndsAt)
        {
            price = price - price * AppConfig.Instance.CurrentCampaignDiscount;
        }

        return Math.Round(price, 2);
    }

    public decimal CancellationFee(Reservation r)
    {
        if (r.Status == "Confirmed" && r.Start - DateTime.Now < TimeSpan.FromHours(24))
        {
            return Math.Round(r.Price * 0.5m, 2);
        }
        return 0;
    }
}
