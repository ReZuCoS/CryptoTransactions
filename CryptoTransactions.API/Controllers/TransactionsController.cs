using CryptoTransactions.API.Model;
using CryptoTransactions.API.Model.Entities;
using CryptoTransactions.API.Model.Entities.QueryEntities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CryptoTransactions.API.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public sealed class TransactionsController : ControllerBase
    {
        /// <summary>
        /// Returns transactions list
        /// </summary>
        /// <param name="transactionQuery">Transaction fields to filter</param>
        /// <param name="limit">Count of returned results</param>
        /// <param name="offset">ID offset (starts from 0)</param>
        /// <response code="200">Successfully returned list</response>
        /// <response code="204">Transactions count equals zero</response>
        /// <response code="400">Limit must be lower than 100 and greather than 0</response>
        /// <response code="404">Transactions count equals zero</response>
        [HttpGet(Name = "GetAllTransactionsFiltered")]
        public IActionResult GetAllFiltered([FromQuery] TransactionQuery transactionQuery,
            int limit = 20, int offset = 0)
        {
            if (limit < 0 || limit > 100)
                base.BadRequest("Limit value must be in range(0 - 100)");

            using var dbContext = new CryptoTransactionsContext();
            IEnumerable<Transaction> transactions =
                dbContext.Transactions.OrderBy(t => t.TimeStamp).ToList();

            if (!transactionQuery.IsEmpty())
                transactions = transactions.Where(t =>
                    t.TimeStamp.Contains(transactionQuery.TimeStamp) &&
                    t.SenderWallet.Contains(transactionQuery.SenderWallet) &&
                    t.RecipientWallet.Contains(transactionQuery.RecipientWallet));

            transactions = transactions.Skip(offset)
                .Take(limit)
                .ToList();

            if (!transactions.Any())
                return base.NoContent();

            return base.Ok(transactions);
        }

        /// <summary>
        /// Returns transaction by GUID
        /// </summary>
        /// <param name="transactionGUID">Transaction GUID</param>
        /// <response code="200">Successfully returned transaction</response>
        /// <response code="400">Sended value doest match GUID standart</response>
        /// <response code="404">GUID not found</response>
        [HttpGet("{transactionGUID}", Name = "GetTransactionByGUID")]
        public IActionResult GetTransactionByGUID(string transactionGUID)
        {
            if (Guid.TryParse(transactionGUID, out _) == false)
                return base.BadRequest("Sended transuctionGUID doest match GUID standart");

            using var dbContext = new CryptoTransactionsContext();
            var transaction = dbContext.Transactions.Include(t => t.Sender)
                .Include(t => t.Recipient)
                .FirstOrDefault(t =>
                t.GUID == transactionGUID);

            if (transaction is null)
                return base.BadRequest("Transaction not found");

            return base.Ok(transaction);
        }

        /// <summary>
        /// Adds new transaction in database
        /// </summary>
        /// <param name="transaction">Transaction data</param>
        /// <response code="201">Successfully created</response>
        /// <response code="400"></response>
        /// <response code="409">Creation error. Check and resend data</response>
        /// <response code="500">Internal server error</response>
        [HttpPost(Name = "AddTransaction")]
        public IActionResult AddNew(Transaction transaction)
        {
            using var dbContext = new CryptoTransactionsContext();

            if (Guid.TryParse(transaction.SenderWallet, out _) == false)
                return base.BadRequest("Sender wallet doesn't match GUID standart");

            if (Guid.TryParse(transaction.RecipientWallet, out _) == false)
                return base.BadRequest("Recipient wallet doesn't match GUID standart");

            if (transaction.SenderWallet.Equals(transaction.RecipientWallet))
                return base.BadRequest("Recipient and sender wallets cannot be equal");

            var sender = dbContext.Clients.FirstOrDefault(c =>
                c.WalletNumber == transaction.SenderWallet);
            var recipient = dbContext.Clients.FirstOrDefault(c =>
                c.WalletNumber == transaction.RecipientWallet);

            if (sender is null)
                return base.BadRequest("Sender wallet not found");

            if (recipient is null)
                return base.BadRequest("Recipient wallet not found");

            if (dbContext.Transactions.Any(t => t.GUID == transaction.GUID))
                return base.Conflict("Transaction GUID alredy exists. Try to resend data");

            if (dbContext.Transactions.Any(t =>
                t.TimeStamp == transaction.TimeStamp &&
                t.SenderWallet == transaction.SenderWallet &&
                t.RecipientWallet == transaction.RecipientWallet))
                return base.Conflict("Transaction alredy exists");

            try
            {
                dbContext.Transactions.Add(transaction);
                dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return base.StatusCode(500, "An error ocurred when saving database. " +
                    "Check transaction data and try again.\n" +
                    $"Error message: {ex.Message}");
            }

            var link = transaction.GUID;

            var location = Url.Action(nameof(AddNew),
                new
                {
                    transaction = link.ToString(),
                }) ?? $"/{link}";

            return base.Created(location, transaction);
        }

        /// <summary>
        /// Deletes transaction by GUID
        /// </summary>
        /// <param name="transactionGUID">Transaction GUID</param>
        /// <response code="202">Transaction removed</response>
        /// <response code="400">Sended value doesn't match GUID standart</response>
        /// <response code="404">GUID not found</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{transactionGUID}", Name = "DeleteTransaction")]
        public IActionResult Delete(string transactionGUID)
        {
            if (Guid.TryParse(transactionGUID, out _) == false)
                return base.BadRequest("Sended transuctionGUID doest match GUID standart");

            using var dbContext = new CryptoTransactionsContext();
            var transaction = dbContext.Transactions.FirstOrDefault(t =>
            t.GUID == transactionGUID);

            if (transaction is null)
                return base.NotFound("Transaction not found");

            try
            {
                dbContext.Transactions.Remove(transaction);
                dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return base.StatusCode(500, "An error ocurred when saving database. " +
                    "Check transaction GUID and try again.\n" +
                    $"Error message: {ex.Message}");
            }

            var location = Url.Action(nameof(Delete),
                new
                {
                    transaction = transaction.GUID,
                }) ?? $"/{transaction.GUID}";

            return base.Accepted(location);
        }
    }
}
