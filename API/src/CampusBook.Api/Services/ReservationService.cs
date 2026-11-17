using System.Globalization;
using CampusBook.Api.Data;
using CampusBook.Api.Infrastructure;
using CampusBook.Api.Models;
using CampusBook.Api.Models.DTOs;
using CampusBook.Api.Models.Mappers;

namespace CampusBook.Api.Services;

public class ReservationService(CampusDbContext db, IReservationRepository repo, PriceCalculator calc)
{
    public List<ResourceAvailabilityDto> GetAvailability(DateTime date)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);

        var resources = db.Resources.ToList();
        var reservations = db.Reservations
            .Where(r => r.Start < dayEnd && r.End > dayStart && r.Status != "Cancelled" && r.Status != "Returned")
            .ToList();

        return resources
            .Select(resource => ReservationMapper.ToAvailabilityDto(
                resource,
                reservations.Where(reservation => reservation.ResourceId == resource.Id),
                dayStart,
                dayEnd))
            .ToList();
    }

    public ReservationDto Create(CreateReservationRequest req)
    {
        if (req == null) throw new ArgumentException("Request is required");
        if (req.End <= req.Start) throw new ArgumentException("End must be after start");
        if (req.Start < DateTime.Now.AddMinutes(-5)) throw new ArgumentException("Start is in the past");
        if ((req.End - req.Start).TotalHours > AppConfig.Instance.MaxHours)
            throw new ArgumentException("Reservation too long, max " + AppConfig.Instance.MaxHours + " hours");

        var user = repo.GetUser(req.UserId);
        if (user == null) throw new KeyNotFoundException("User " + req.UserId + " not found");

        var resource = repo.GetResource(req.ResourceId);
        if (resource == null) throw new KeyNotFoundException("Resource " + req.ResourceId + " not found");

        if (user.Role == "student" && resource.Type == "room" && (req.End - req.Start).TotalHours > 4)
        {
            throw new ArgumentException("Students can book a room for at most 4 hours");
        }

        if (req.Delivery && !resource.CanBeDelivered())
        {
            throw new ArgumentException("This resource cannot be delivered");
        }

        if (repo.HasOverlap(resource.Id, req.Start, req.End))
        {
            throw new InvalidOperationException("Resource is already booked in this period");
        }

        if (user.Role == "student")
        {
            var open = repo.GetByUser(user.Id)
                .Count(x => x.Status == "Pending" || x.Status == "Confirmed" || x.Status == "CheckedOut");
            if (open >= 3)
            {
                throw new InvalidOperationException("Students can have at most 3 open reservations");
            }
        }

        var r = ReservationMapper.ToEntity(req);
        r.UserId = user.Id;
        r.Status = "Pending";
        r.CreatedAt = DateTime.Now;

        resource.Reserve(r);

        r.Price = calc.Calculate(resource, user, r.Start, r.End, r.Insurance, r.Delivery, r.Technician, r.Cleaning, r.ExtendedReturn);

        r.Deposit = 0;
        if (user.Role == "student")
        {
            if (resource.Type == "camera")
            {
                r.Deposit = 150m;
            }
            else if (resource.Type == "laptop" && r.Delivery)
            {
                r.Deposit = 50m;
            }
        }

        repo.Add(r);
        repo.SaveChanges();

        var mailer = new SmtpMailer("smtp.campus.local");
        mailer.Send(user.Email, "CampusBook: reservation received",
            "Hi " + user.Name + ",\n\nWe received your reservation #" + r.Id + " for " + resource.Name +
            " on " + r.Start.ToString("dd/MM/yyyy HH:mm") + ".\nPrice: EUR " + r.Price +
            (r.Deposit > 0 ? "\nDeposit: EUR " + r.Deposit : "") + "\n\nCampusBook");

        new FileAuditLogger().Log("CREATE", r.Id, user.Id, resource.Name);

        return ToDto(r, resource);
    }

    public Reservation Confirm(int id)
    {
        var r = repo.GetById(id);
        if (r == null) throw new KeyNotFoundException("Reservation " + id + " not found");

        if (r.Status != "Pending")
        {
            throw new InvalidOperationException("Only pending reservations can be confirmed");
        }

        var user = repo.GetUser(r.UserId);
        if (user.Role == "student" && !r.Paid && r.Price + r.Deposit > 0)
        {
            throw new InvalidOperationException("Reservation must be paid before it can be confirmed");
        }

        r.Status = "Confirmed";
        repo.Update(r);
        repo.SaveChanges();
        return r;
    }

    public Reservation CheckOut(int id)
    {
        var rsv = repo.GetById(id);
        if (rsv == null) throw new KeyNotFoundException("Reservation " + id + " not found");

        if (rsv.Status == "Confirmed")
        {
            if (DateTime.Now < rsv.Start.AddMinutes(-30))
            {
                throw new InvalidOperationException("Too early, check out is possible 30 minutes before start");
            }
            if (DateTime.Now > rsv.End)
            {
                throw new InvalidOperationException("Reservation period is over");
            }
            if (rsv.Deposit > 0 && !rsv.Paid)
            {
                throw new InvalidOperationException("Deposit not paid");
            }

            rsv.Status = "CheckedOut";
            rsv.CheckedOutAt = DateTime.Now;
            repo.Update(rsv);
            repo.SaveChanges();

            var u = repo.GetUser(rsv.UserId);
            var mailer = new SmtpMailer("smtp.campus.local");
            mailer.Send(u.Email, "CampusBook: " + rsv.Resource.Name + " picked up",
                "Please return it before " + (rsv.ExtendedReturn ? rsv.End.AddHours(12) : rsv.End).ToString("dd/MM/yyyy HH:mm") + ".");
            new FileAuditLogger().Log("CHECKOUT", rsv.Id, u.Id);
        }
        else if (rsv.Status == "Pending")
        {
            throw new InvalidOperationException("Reservation is not confirmed yet");
        }
        else
        {
            throw new InvalidOperationException("Cannot check out a reservation with status " + rsv.Status);
        }
        return rsv;
    }

    public Reservation Return(int id)
    {
        var r = repo.GetById(id);
        if (r == null) throw new KeyNotFoundException("Reservation " + id + " not found");

        if (r.Status != "CheckedOut")
        {
            throw new InvalidOperationException("Only checked out reservations can be returned");
        }

        r.Status = "Returned";
        r.ReturnedAt = DateTime.Now;

        var allowed = r.ExtendedReturn ? r.End.AddHours(12) : r.End.AddMinutes(15);
        if (r.ReturnedAt > allowed)
        {
            var lateHours = (decimal)Math.Ceiling((r.ReturnedAt.Value - allowed).TotalHours);
            r.Price = r.Price + lateHours * 5m;
            r.Notes = (r.Notes ?? "") + " [late " + lateHours + "h]";
        }

        if (r.Deposit > 0 && r.Paid)
        {
            var gateway = new LegacyPayGateway();
            var amount = ((int)(r.Deposit * 100)).ToString(CultureInfo.InvariantCulture);
            var code = gateway.Refund(AppConfig.Instance.MerchantId, r.PaymentRef, amount);
            if (code != LegacyPayGateway.OK)
            {
                r.Notes = (r.Notes ?? "") + " [deposit refund failed: " + gateway.LastError + "]";
            }
        }

        repo.Update(r);
        repo.SaveChanges();

        var user = repo.GetUser(r.UserId);
        var mailer = new SmtpMailer("smtp.campus.local");
        mailer.Send(user.Email, "CampusBook: thanks for returning " + r.Resource.Name,
            "Your reservation #" + r.Id + " is closed." + (r.Deposit > 0 ? " Your deposit will be refunded." : ""));
        new FileAuditLogger().Log("RETURN", r.Id, user.Id, r.Notes);

        return r;
    }

    public Reservation Cancel(int id)
    {
        var r = repo.GetById(id);
        if (r == null) throw new KeyNotFoundException("Reservation " + id + " not found");

        if (r.Status == "Cancelled")
        {
            throw new InvalidOperationException("Reservation is already cancelled");
        }
        if (r.Status == "CheckedOut")
        {
            throw new InvalidOperationException("Item is checked out, return it first");
        }

        var fee = calc.CancellationFee(r);

        if (r.Paid && r.PaymentRef != "FREE")
        {
            var refund = r.Price + r.Deposit - fee;
            if (refund > 0)
            {
                var gateway = new LegacyPayGateway();
                var cents = ((int)(refund * 100)).ToString(CultureInfo.InvariantCulture);
                var code = gateway.Refund(AppConfig.Instance.MerchantId, r.PaymentRef, cents);
                if (code != LegacyPayGateway.OK)
                {
                    throw new InvalidOperationException("Refund failed: " + gateway.LastError);
                }
            }
        }

        r.Status = "Cancelled";
        r.CancelledAt = DateTime.Now;
        if (fee > 0)
        {
            r.Notes = (r.Notes ?? "") + " [cancellation fee " + fee + "]";
        }
        repo.Update(r);
        repo.SaveChanges();

        var user = repo.GetUser(r.UserId);
        var mailer = new SmtpMailer("smtp.campus.local");
        mailer.Send(user.Email, "CampusBook: reservation cancelled",
            "Your reservation #" + r.Id + " has been cancelled." + (fee > 0 ? " A fee of EUR " + fee + " was charged." : ""));
        new FileAuditLogger().Log("CANCEL", r.Id, user.Id, fee.ToString(CultureInfo.InvariantCulture));

        return r;
    }

    public Reservation Pay(int reservationID)
    {
        var r = repo.GetById(reservationID);
        if (r == null) throw new KeyNotFoundException("Reservation " + reservationID + " not found");

        if (r.Paid)
        {
            throw new InvalidOperationException("Reservation is already paid");
        }
        if (r.Status == "Cancelled" || r.Status == "Returned")
        {
            throw new InvalidOperationException("Cannot pay a reservation with status " + r.Status);
        }

        var total = r.Price + r.Deposit;
        if (total == 0)
        {
            r.Paid = true;
            r.PaymentRef = "FREE";
            repo.Update(r);
            repo.SaveChanges();
            return r;
        }

        var gateway = new LegacyPayGateway();
        var cents = ((int)(total * 100)).ToString(CultureInfo.InvariantCulture);
        int code;
        try
        {
            code = gateway.Charge(AppConfig.Instance.MerchantId, cents, "R" + r.Id);
        }
        catch (TimeoutException)
        {
            throw new InvalidOperationException("Payment provider did not answer, try again");
        }

        if (code == LegacyPayGateway.OK)
        {
            r.Paid = true;
            r.PaymentRef = gateway.LastTransactionId;
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
            throw new InvalidOperationException("Payment failed: " + gateway.LastError);
        }

        repo.Update(r);
        repo.SaveChanges();

        var user = repo.GetUser(r.UserId);
        var mailer = new SmtpMailer("smtp.campus.local");
        mailer.Send(user.Email, "CampusBook: payment received",
            "We received EUR " + total + " for reservation #" + r.Id + ". Reference: " + r.PaymentRef);
        new FileAuditLogger().Log("PAY", r.Id, user.Id, r.PaymentRef);

        return r;
    }

    public List<ReservationDto> GetForUser(int userId)
    {
        var list = repo.GetByUser(userId);
        var result = new List<ReservationDto>();
        foreach (var r in list)
        {
            result.Add(ToDto(r, r.Resource));
        }
        return result;
    }

    public List<ReservationDto> GetOverdue()
    {
        return repo.GetOverdue(DateTime.Now).Select(r => ToDto(r, r.Resource)).ToList();
    }

    public int SendReminders()
    {
        var tomorrow = DateTime.Today.AddDays(1);
        var list = repo.GetByDateRange(tomorrow, tomorrow.AddDays(1))
            .Where(r => r.Status == "Confirmed" && r.Start >= tomorrow)
            .ToList();

        int count = 0;
        foreach (var r in list)
        {
            var user = repo.GetUser(r.UserId);
            var resource = repo.GetResource(r.ResourceId);
            var mailer = new SmtpMailer("smtp.campus.local");
            mailer.Send(user.Email, "CampusBook: reminder",
                "Tomorrow at " + r.Start.ToString("HH:mm") + " you can pick up " + resource.Name + " at " + resource.Location + ".");
            count++;
        }

        //foreach (var r in repo.GetUnpaid())
        //{
        //    var user = repo.GetUser(r.UserId);
        //    if (user.Role == "association" && r.Start < DateTime.Now.AddDays(7))
        //    {
        //        new SmtpMailer("smtp.campus.local").Send(user.Email, "CampusBook: payment due", "...");
        //    }
        //}

        new FileAuditLogger().Log("REMINDERS", 0, 0, count.ToString());
        return count;
    }

    private ReservationDto ToDto(Reservation r, Resource resource)
    {
        return ReservationMapper.ToDto(r, resource);
    }
}
