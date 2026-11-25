using System.Globalization;
using CampusBook.Api.Data;
using CampusBook.Api.Infrastructure;
using CampusBook.Api.Models;
using CampusBook.Api.Models.DTOs;
using CampusBook.Api.Models.Mappers;

namespace CampusBook.Api.Services;

public class ReservationService(IReservationRepository repo, PriceCalculator calc)
{
    public List<ResourceAvailabilityDto> GetAvailability(DateTime date)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);

        var resources = repo.GetAllResources();
        var reservations = repo.GetByDateRange(dayStart, dayEnd)
            .Where(r => r.Status != ReservationStatus.Cancelled && r.Status != ReservationStatus.Returned)
            .ToList();

        return resources
            .Select(resource => ReservationMapper.ToAvailabilityDto(
                resource,
                reservations.Where(r => r.ResourceId == resource.Id),
                dayStart,
                dayEnd))
            .ToList();
    }

    public ReservationDto Create(CreateReservationRequest req)
    {
        if (req == null)
            throw new ArgumentException("Request is required");

        if (req.End <= req.Start)
            throw new ArgumentException("End must be after start");

        if (req.Start < DateTime.Now.AddMinutes(-5))
            throw new ArgumentException("Start is in the past");

        if ((req.End - req.Start).TotalHours > AppConfig.Instance.MaxHours)
            throw new ArgumentException(
                "Reservation too long, max " + AppConfig.Instance.MaxHours + " hours");

        var user = repo.GetUser(req.UserId)
            ?? throw new KeyNotFoundException("User " + req.UserId + " not found");

        var resource = repo.GetResource(req.ResourceId)
            ?? throw new KeyNotFoundException("Resource " + req.ResourceId + " not found");

        if (user.UserRole == UserRole.Student &&
            resource.Type == ResourceTypes.Room &&
            (req.End - req.Start).TotalHours > 4)
        {
            throw new ArgumentException(
                "Students can book a room for at most 4 hours");
        }

        if (req.Delivery && !resource.CanBeDelivered())
        {
            throw new ArgumentException("This resource cannot be delivered");
        }

        if (repo.HasOverlap(resource.Id, req.Start, req.End))
        {
            throw new InvalidOperationException(
                "Resource is already booked in this period");
        }

        if (user.UserRole == UserRole.Student)
        {
            var openReservations = repo.GetByUser(user.Id)
                .Count(r =>
                    r.Status == ReservationStatus.Pending ||
                    r.Status == ReservationStatus.Confirmed ||
                    r.Status == ReservationStatus.CheckedOut);

            if (openReservations >= 3)
            {
                throw new InvalidOperationException(
                    "Students can have at most 3 open reservations");
            }
        }

        var reservation = ReservationMapper.ToEntity(req);

        reservation.UserId = user.Id;
        reservation.Status = ReservationStatus.Pending;
        reservation.CreatedAt = DateTime.Now;

        resource.Reserve(reservation);

        reservation.Price = calc.Calculate(
            resource,
            user,
            reservation.Start,
            reservation.End,
            reservation.Insurance,
            reservation.Delivery,
            reservation.Technician,
            reservation.Cleaning,
            reservation.ExtendedReturn);

        reservation.Deposit = CalculateDeposit(user, resource, reservation);

        repo.Add(reservation);
        repo.SaveChanges();

        var mailer = new SmtpMailer("smtp.campus.local");

        mailer.Send(
            user.Email,
            "CampusBook: reservation received",
            "Hi " + user.Name + ",\n\nWe received your reservation #" +
            reservation.Id + " for " + resource.Name +
            " on " + reservation.Start.ToString("dd/MM/yyyy HH:mm") +
            ".\nPrice: EUR " + reservation.Price +
            (reservation.Deposit > 0
                ? "\nDeposit: EUR " + reservation.Deposit
                : "") +
            "\n\nCampusBook");

        new FileAuditLogger().Log(
            "CREATE", reservation.Id, user.Id, resource.Name);

        return ToDto(reservation, resource);
    }

    private decimal CalculateDeposit(
        User user,
        Resource resource,
        Reservation reservation)
    {
        if (user.UserRole != UserRole.Student)
            return 0m;

        return resource.Type switch
        {
            ResourceTypes.Camera => 150m,
            ResourceTypes.Laptop when reservation.Delivery => 50m,
            _ => 0m
        };
    }

    public Reservation Confirm(int id)
    {
        var reservation = repo.GetById(id)
            ?? throw new KeyNotFoundException("Reservation " + id + " not found");

        if (reservation.Status != ReservationStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending reservations can be confirmed");
        }

        var user = repo.GetUser(reservation.UserId);

        if (user.UserRole == UserRole.Student &&
            !reservation.Paid &&
            reservation.Price + reservation.Deposit > 0)
        {
            throw new InvalidOperationException(
                "Reservation must be paid before it can be confirmed");
        }

        reservation.Status = ReservationStatus.Confirmed;

        repo.Update(reservation);
        repo.SaveChanges();

        return reservation;
    }

    public Reservation CheckOut(int id)
    {
        var reservation = repo.GetById(id)
            ?? throw new KeyNotFoundException("Reservation " + id + " not found");

        if (reservation.Status == ReservationStatus.Pending)
        {
            throw new InvalidOperationException(
                "Reservation is not confirmed yet");
        }

        if (reservation.Status != ReservationStatus.Confirmed)
        {
            throw new InvalidOperationException(
                "Cannot check out a reservation with status " + reservation.Status);
        }

        if (DateTime.Now < reservation.Start.AddMinutes(-30))
        {
            throw new InvalidOperationException(
                "Too early, check out is possible 30 minutes before start");
        }

        if (DateTime.Now > reservation.End)
        {
            throw new InvalidOperationException("Reservation period is over");
        }

        if (reservation.Deposit > 0 && !reservation.Paid)
        {
            throw new InvalidOperationException("Deposit not paid");
        }

        reservation.Status = ReservationStatus.CheckedOut;
        reservation.CheckedOutAt = DateTime.Now;

        repo.Update(reservation);
        repo.SaveChanges();

        var user = repo.GetUser(reservation.UserId);

        var mailer = new SmtpMailer("smtp.campus.local");

        mailer.Send(
            user.Email,
            "CampusBook: " + reservation.Resource.Name + " picked up",
            "Please return it before " +
            (reservation.ExtendedReturn
                ? reservation.End.AddHours(12)
                : reservation.End).ToString("dd/MM/yyyy HH:mm") + ".");

        new FileAuditLogger().Log("CHECKOUT", reservation.Id, user.Id);

        return reservation;
    }

    public Reservation Return(int id)
    {
        var reservation = repo.GetById(id)
            ?? throw new KeyNotFoundException("Reservation " + id + " not found");

        if (reservation.Status != ReservationStatus.CheckedOut)
        {
            throw new InvalidOperationException(
                "Only checked out reservations can be returned");
        }

        reservation.Status = ReservationStatus.Returned;
        reservation.ReturnedAt = DateTime.Now;

        var allowedReturnTime = reservation.ExtendedReturn
            ? reservation.End.AddHours(12)
            : reservation.End.AddMinutes(15);

        if (reservation.ReturnedAt > allowedReturnTime)
        {
            var lateHours = (decimal)Math.Ceiling(
                (reservation.ReturnedAt.Value - allowedReturnTime).TotalHours);

            reservation.Price += lateHours * 5m;

            reservation.Notes = (reservation.Notes ?? "") +
                                " [late " + lateHours + "h]";
        }

        if (reservation.Deposit > 0 && reservation.Paid)
        {
            var gateway = new LegacyPayGateway();

            var amount = ((int)(reservation.Deposit * 100))
                .ToString(CultureInfo.InvariantCulture);

            var code = gateway.Refund(
                AppConfig.Instance.MerchantId,
                reservation.PaymentRef,
                amount);

            if (code != LegacyPayGateway.OK)
            {
                reservation.Notes = (reservation.Notes ?? "") +
                    " [deposit refund failed: " + gateway.LastError + "]";
            }
        }

        repo.Update(reservation);
        repo.SaveChanges();

        var user = repo.GetUser(reservation.UserId);

        var mailer = new SmtpMailer("smtp.campus.local");

        mailer.Send(
            user.Email,
            "CampusBook: thanks for returning " + reservation.Resource.Name,
            "Your reservation #" + reservation.Id + " is closed." +
            (reservation.Deposit > 0
                ? " Your deposit will be refunded."
                : ""));

        new FileAuditLogger().Log(
            "RETURN", reservation.Id, user.Id, reservation.Notes);

        return reservation;
    }

    public Reservation Cancel(int id)
    {
        var reservation = repo.GetById(id)
            ?? throw new KeyNotFoundException("Reservation " + id + " not found");

        if (reservation.Status == ReservationStatus.Cancelled)
        {
            throw new InvalidOperationException("Reservation is already cancelled");
        }

        if (reservation.Status == ReservationStatus.CheckedOut)
        {
            throw new InvalidOperationException("Item is checked out, return it first");
        }

        var fee = calc.CancellationFee(reservation);

        if (reservation.Paid && reservation.PaymentRef != "FREE")
        {
            var refund = reservation.Price + reservation.Deposit - fee;

            if (refund > 0)
            {
                var gateway = new LegacyPayGateway();

                var cents = ((int)(refund * 100))
                    .ToString(CultureInfo.InvariantCulture);

                var code = gateway.Refund(
                    AppConfig.Instance.MerchantId,
                    reservation.PaymentRef,
                    cents);

                if (code != LegacyPayGateway.OK)
                {
                    throw new InvalidOperationException(
                        "Refund failed: " + gateway.LastError);
                }
            }
        }

        reservation.Status = ReservationStatus.Cancelled;
        reservation.CancelledAt = DateTime.Now;

        if (fee > 0)
        {
            reservation.Notes = (reservation.Notes ?? "") +
                                " [cancellation fee " + fee + "]";
        }

        repo.Update(reservation);
        repo.SaveChanges();

        var user = repo.GetUser(reservation.UserId);

        var mailer = new SmtpMailer("smtp.campus.local");

        mailer.Send(
            user.Email,
            "CampusBook: reservation cancelled",
            "Your reservation #" + reservation.Id + " has been cancelled." +
            (fee > 0 ? " A fee of EUR " + fee + " was charged." : ""));

        new FileAuditLogger().Log(
            "CANCEL",
            reservation.Id,
            user.Id,
            fee.ToString(CultureInfo.InvariantCulture));

        return reservation;
    }

    public Reservation Pay(int reservationID)
    {
        var reservation = repo.GetById(reservationID)
            ?? throw new KeyNotFoundException(
                "Reservation " + reservationID + " not found");

        if (reservation.Paid)
        {
            throw new InvalidOperationException("Reservation is already paid");
        }

        if (reservation.Status is ReservationStatus.Cancelled
            or ReservationStatus.Returned)
        {
            throw new InvalidOperationException(
                "Cannot pay a reservation with status " + reservation.Status);
        }

        var total = reservation.Price + reservation.Deposit;

        if (total == 0)
        {
            reservation.Paid = true;
            reservation.PaymentRef = "FREE";

            repo.Update(reservation);
            repo.SaveChanges();

            return reservation;
        }

        var gateway = new LegacyPayGateway();

        var cents = ((int)(total * 100))
            .ToString(CultureInfo.InvariantCulture);

        int code;

        try
        {
            code = gateway.Charge(
                AppConfig.Instance.MerchantId,
                cents,
                "R" + reservation.Id);
        }
        catch (TimeoutException)
        {
            throw new InvalidOperationException(
                "Payment provider did not answer, try again");
        }

        if (code == LegacyPayGateway.OK)
        {
            reservation.Paid = true;
            reservation.PaymentRef = gateway.LastTransactionId;
        }
        else if (code == LegacyPayGateway.DECLINED)
        {
            throw new InvalidOperationException("Payment declined");
        }
        else if (code == LegacyPayGateway.BAD_AMOUNT)
        {
            throw new ArgumentException("Invalid amount");
        }
        else
        {
            throw new InvalidOperationException(
                "Payment failed: " + gateway.LastError);
        }

        repo.Update(reservation);
        repo.SaveChanges();

        var user = repo.GetUser(reservation.UserId);

        var mailer = new SmtpMailer("smtp.campus.local");

        mailer.Send(
            user.Email,
            "CampusBook: payment received",
            "We received EUR " + total + " for reservation #" +
            reservation.Id + ". Reference: " + reservation.PaymentRef);

        new FileAuditLogger().Log(
            "PAY", reservation.Id, user.Id, reservation.PaymentRef);

        return reservation;
    }

    public List<ReservationDto> GetForUser(int userId)
    {
        return repo.GetByUser(userId)
            .Select(r => ToDto(r, r.Resource))
            .ToList();
    }

    public List<ReservationDto> GetOverdue()
    {
        return repo.GetOverdue(DateTime.Now)
            .Select(r => ToDto(r, r.Resource))
            .ToList();
    }

    public int SendReminders()
    {
        var tomorrow = DateTime.Today.AddDays(1);

        var reservations = repo.GetByDateRange(tomorrow, tomorrow.AddDays(1))
            .Where(r =>
                r.Status == ReservationStatus.Confirmed &&
                r.Start >= tomorrow)
            .ToList();

        int count = 0;

        foreach (var reservation in reservations)
        {
            var user = repo.GetUser(reservation.UserId);
            var resource = repo.GetResource(reservation.ResourceId);

            var mailer = new SmtpMailer("smtp.campus.local");

            mailer.Send(
                user.Email,
                "CampusBook: reminder",
                "Tomorrow at " + reservation.Start.ToString("HH:mm") +
                " you can pick up " + resource.Name +
                " at " + resource.Location + ".");

            count++;
        }

        new FileAuditLogger().Log(
            "REMINDERS", 0, 0, count.ToString());

        return count;
    }

    private ReservationDto ToDto(Reservation reservation, Resource resource)
    {
        return ReservationMapper.ToDto(reservation, resource);
    }
}
