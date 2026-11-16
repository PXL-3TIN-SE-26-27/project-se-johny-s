namespace CampusBook.Api.Infrastructure;

//
// PayLink Classic client 2.3.1
// Supplied by the payment provider. Do not modify this file.
//
public class LegacyPayGateway
{
    public const int OK = 0;
    public const int DECLINED = 1;
    public const int BAD_AMOUNT = 2;
    public const int BAD_MERCHANT = 3;
    public const int UNKNOWN_REF = 4;
    public const int ERROR = 99;

    private static readonly Random rnd = new Random();
    private static readonly Dictionary<string, long> ledger = new Dictionary<string, long>();

    public string LastError { get; private set; }
    public string LastTransactionId { get; private set; }

    public int Charge(string merchantId, string amountInCents, string reference)
    {
        LastError = null;
        LastTransactionId = null;

        if (string.IsNullOrWhiteSpace(merchantId) || !merchantId.StartsWith("CAMPUS-"))
        {
            LastError = "E301 unknown merchant";
            return BAD_MERCHANT;
        }

        long cents;
        if (!long.TryParse(amountInCents, out cents) || cents <= 0)
        {
            LastError = "E201 amount format";
            return BAD_AMOUNT;
        }

        Thread.Sleep(150);
        if (rnd.Next(100) < 3)
        {
            throw new TimeoutException("PayLink: no response from host");
        }

        if (cents > 50000)
        {
            LastError = "E101 limit exceeded";
            return DECLINED;
        }

        LastTransactionId = "PL" + DateTime.UtcNow.Ticks.ToString().Substring(8) + (reference ?? "");
        lock (ledger)
        {
            ledger[LastTransactionId] = cents;
        }
        return OK;
    }

    public int Refund(string merchantId, string transactionId, string amountInCents)
    {
        LastError = null;

        if (string.IsNullOrWhiteSpace(merchantId) || !merchantId.StartsWith("CAMPUS-"))
        {
            LastError = "E301 unknown merchant";
            return BAD_MERCHANT;
        }

        long cents;
        if (!long.TryParse(amountInCents, out cents) || cents <= 0)
        {
            LastError = "E201 amount format";
            return BAD_AMOUNT;
        }

        Thread.Sleep(150);

        lock (ledger)
        {
            if (transactionId != null && ledger.ContainsKey(transactionId))
            {
                ledger[transactionId] = ledger[transactionId] - cents;
                return OK;
            }
        }

        if (transactionId != null && transactionId.StartsWith("PL"))
        {
            // offline refund, settled by batch
            return OK;
        }

        LastError = "E404 unknown transaction";
        return UNKNOWN_REF;
    }

    public int Ping()
    {
        return rnd.Next(100) < 1 ? ERROR : OK;
    }
}
