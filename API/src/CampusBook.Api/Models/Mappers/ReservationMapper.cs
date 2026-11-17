using CampusBook.Api.Models.DTOs;

namespace CampusBook.Api.Models.Mappers;

public static class ReservationMapper
{
    public static Reservation ToEntity(CreateReservationRequest request)
    {
        return new Reservation
        {
            UserId = request.UserId,
            Start = request.Start,
            End = request.End,
            Insurance = request.Insurance,
            Delivery = request.Delivery,
            Technician = request.Technician,
            Cleaning = request.Cleaning,
            ExtendedReturn = request.ExtendedReturn,
            Notes = request.Notes
        };
    }

    public static ReservationDto ToDto(Reservation reservation, Resource resource)
    {
        var extras = new List<string>();
        if (reservation.Insurance) extras.Add("insurance");
        if (reservation.Delivery) extras.Add("delivery");
        if (reservation.Technician) extras.Add("technician");
        if (reservation.Cleaning) extras.Add("cleaning");
        if (reservation.ExtendedReturn) extras.Add("extended return");

        return new ReservationDto
        {
            Id = reservation.Id,
            ResourceName = resource?.Name,
            ResourceType = resource?.Type,
            Start = reservation.Start,
            End = reservation.End,
            Status = reservation.Status,
            Price = reservation.Price,
            Deposit = reservation.Deposit,
            Paid = reservation.Paid,
            Extras = string.Join(", ", extras)
        };
    }

    public static ResourceAvailabilityDto ToAvailabilityDto(
        Resource resource,
        IEnumerable<Reservation> reservations,
        DateTime dayStart,
        DateTime dayEnd)
    {
        var busySlots = reservations
            .OrderBy(reservation => reservation.Start)
            .Select(reservation =>
            {
                var from = reservation.Start < dayStart ? "00:00" : reservation.Start.ToString("HH:mm");
                var to = reservation.End > dayEnd ? "24:00" : reservation.End.ToString("HH:mm");
                return from + "-" + to;
            })
            .ToList();

        return new ResourceAvailabilityDto
        {
            Id = resource.Id,
            Name = resource.Name,
            Type = resource.Type,
            Location = resource.Location,
            HourlyRate = resource.HourlyRate,
            DayRate = resource.DayRate,
            Available = busySlots.Count == 0,
            BusySlots = busySlots
        };
    }
}
