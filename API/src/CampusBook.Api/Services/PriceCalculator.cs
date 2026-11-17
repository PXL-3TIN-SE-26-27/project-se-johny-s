using CampusBook.Api.Infrastructure;
using CampusBook.Api.Models;

namespace CampusBook.Api.Services;

public class PriceCalculator
{

    public decimal Calculate(
        Resource resource,
        User user,
        DateTime start,
        DateTime end,
        bool insurance,
        bool delivery,
        bool technician,
        bool cleaning,
        bool extendedReturn)
    {
        decimal hours = (decimal)Math.Ceiling((end - start).TotalHours);
        decimal days = (decimal)Math.Ceiling((end - start).TotalDays);

        if (hours <= 0)
        {
            throw new ArgumentException("Invalid period");
        }

        if (user.UserRole == UserRole.Staff)
        {
            return 0;
        }

        decimal price = user.UserRole switch
        {
            UserRole.Student => CalculateStudentPrice(resource, hours, days),
            _ => user.UserRole == UserRole.Association                ? CalculateAssociationPrice(resource, start, hours, days)
                : throw new InvalidOperationException($"Unknown role {user.UserRole}")
        };

        price = ApplyWeekendSurcharge(price, end);
        price = ApplyInsurance(price, resource, insurance);
        price = ApplyDelivery(price, resource, delivery, insurance);
        price = ApplyAdditionalServices(price, resource, hours, technician, cleaning);
        price = ApplyExtendedReturn(price, delivery, technician, extendedReturn);
        price = ApplyCampaignDiscount(price);

        return Math.Round(price, 2);
    }

    private decimal CalculateStudentPrice(Resource resource, decimal hours, decimal days)
    {
        return resource.Type switch
        {
            ResourceTypes.Laptop => CalculateHourlyOrDailyPrice(resource, hours, days, 24),
            ResourceTypes.Camera => CalculateHourlyOrDailyPrice(resource, hours, days, 8),
            ResourceTypes.Printer => hours * resource.HourlyRate + 2.50m,
            ResourceTypes.Room => hours * resource.HourlyRate,
            _ => throw new InvalidOperationException($"Unknown resource type {resource.Type}")
        };
    }

    private decimal CalculateAssociationPrice(
        Resource resource,
        DateTime start,
        decimal hours,
        decimal days)
    {
        return resource.Type switch
        {
            ResourceTypes.Room => CalculateAssociationRoomPrice(resource, start, days),
            ResourceTypes.Laptop or ResourceTypes.Camera => days * resource.DayRate,
            ResourceTypes.Printer => hours * resource.HourlyRate + 2.50m,
            _ => throw new InvalidOperationException($"Unknown resource type {resource.Type}")
        };
    }

    private decimal CalculateHourlyOrDailyPrice(
        Resource resource,
        decimal hours,
        decimal days,
        decimal dailyRateThreshold)
    {
        decimal price = hours * resource.HourlyRate;

        if (hours > dailyRateThreshold)
        {
            price = Math.Min(price, days * resource.DayRate);
        }

        return price;
    }

    private decimal CalculateAssociationRoomPrice(
        Resource resource,
        DateTime start,
        decimal days)
    {
        decimal price = days * resource.DayRate;

        bool isWeekday = start.DayOfWeek is not DayOfWeek.Saturday
            and not DayOfWeek.Sunday;

        return isWeekday ? price * 0.8m : price;
    }

    private decimal ApplyWeekendSurcharge(decimal price, DateTime end)
    {
        bool isWeekend = end.DayOfWeek is DayOfWeek.Saturday
            or DayOfWeek.Sunday;

        if (isWeekend)
        {
            price += price * AppConfig.Instance.WeekendSurcharge;
        }

        return price;
    }

    private decimal ApplyInsurance(decimal price, Resource resource, bool insurance)
    {
        if (!insurance)
        {
            return price;
        }

        bool isInsurableResource = resource.Type is ResourceTypes.Camera
            or ResourceTypes.Laptop;

        return isInsurableResource
            ? price * 1.10m
            : price + 5m;
    }

    private decimal ApplyDelivery(
        decimal price,
        Resource resource,
        bool delivery,
        bool insurance)
    {
        if (!delivery)
        {
            return price;
        }

        if (resource.Type == ResourceTypes.Room)
        {
            throw new InvalidOperationException("Rooms cannot be delivered");
        }

        return price + (insurance ? 10m : 15m);
    }

    private decimal ApplyAdditionalServices(
        decimal price,
        Resource resource,
        decimal hours,
        bool technician,
        bool cleaning)
    {
        if (technician)
        {
            price += 30m * hours;
        }

        if (cleaning)
        {
            price += resource.Type == ResourceTypes.Room ? 25m : 8m;
        }

        return price;
    }

    private decimal ApplyExtendedReturn(
        decimal price,
        bool delivery,
        bool technician,
        bool extendedReturn)
    {
        if (!extendedReturn)
        {
            return price;
        }

        if (delivery)
        {
            return price + 5m;
        }

        if (technician)
        {
            return price;
        }

        return price + 7.5m;
    }

    private decimal ApplyCampaignDiscount(decimal price)
    {
        var config = AppConfig.Instance;

        if (config.CurrentCampaignDiscount > 0 &&
            DateTime.Now < config.CampaignEndsAt)
        {
            price -= price * config.CurrentCampaignDiscount;
        }

        return price;
    }


    public decimal CancellationFee(Reservation reservation)
    {
        if (reservation.Status == ReservationStatus.Confirmed &&
            reservation.Start - DateTime.Now < TimeSpan.FromHours(24))
        {
            return Math.Round(reservation.Price * 0.5m, 2);
        }

        return 0;
    }

}
