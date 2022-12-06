using CryptoTransactions.API.Model;
using CryptoTransactions.API.Model.Entities;
using CryptoTransactions.API.Model.Entities.QueryEntities;
using CryptoTransactions.API.Model.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CryptoTransactions.API.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public sealed class TransactionsController : ControllerBase
    {
        private readonly CryptoTransactionsContext _databaseContext = new();

        /// <summary>
        /// Returns transactions list
        /// </summary>
        /// <param name="transactionQuery">Transaction fields to filter</param>
        /// <param name="limit">Count of returned results</param>
        /// <param name="offset">ID offset (starts from 0)</param>
        /// <response code="200">Successfully returned list</response>
        /// <response code="204">Transactions count equals zero</response>
        /// <response code="400">Limit must be lower than 100 and greather than 0</response>
        [HttpGet(Name = "GetAllTransactionsFiltered")]
        public IActionResult GetAllFiltered([FromQuery] TransactionQuery transactionQuery,
            [Range(1, 100)] int limit = 20, [Range(0, int.MaxValue)] int offset = 0)
        {
            var transactions = _databaseContext.Transactions
                .OrderBy(t => t.TimeStamp)
                .ToList();

            if (!transactionQuery.IsEmpty())
                transactions = transactions.Where(t =>
                    t.TimeStamp.Contains(transactionQuery.TimeStamp) &&
                    t.SenderWallet.Contains(transactionQuery.SenderWallet) &&
                    t.RecipientWallet.Contains(transactionQuery.RecipientWallet))
                    .ToList();

            transactions = transactions
                .Skip(offset)
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
        /// <response code="404">Sended value doest match GUID standart</response>
        [HttpGet("{transactionGUID}", Name = "GetTransactionByGUID")]
        public IActionResult GetTransactionByGUID([GuidValue] string transactionGUID)
        {
            var transaction = _databaseContext.Transactions
                .Include(t => t.Sender)
                .Include(t => t.Recipient)
                .FirstOrDefault(t =>
                t.GUID == transactionGUID);

            if (transaction is null)
                return base.NotFound("Transaction not found");

            return base.Ok(transaction);
        }

        /// <summary>
        /// Adds new transaction in database
        /// </summary>
        /// <param name="transaction">Transaction data</param>
        /// <response code="201">Successfully created</response>
        /// <response code="400">Request arguments errors</response>
        /// <response code="404">Clients wallets not found</response>
        /// <response code="409">Creation error. Check and resend data</response>
        /// <response code="500">Internal server error</response>
        [HttpPost(Name = "AddTransaction")]
        public IActionResult AddNew(Transaction transaction)
        {
            if (transaction.SenderWallet.Equals(transaction.RecipientWallet))
                return base.BadRequest("Recipient and sender wallets cannot be equal");

            if (transaction.Amount <= 0)
                return base.BadRequest("Transaction amount cannot be lower or equals zero");

            var sender = _databaseContext.Clients.FirstOrDefault(c =>
                c.WalletNumber == transaction.SenderWallet);
            var recipient = _databaseContext.Clients.FirstOrDefault(c =>
                c.WalletNumber == transaction.RecipientWallet);

            if (sender is null)
                return base.NotFound("Sender wallet not found");

            if (recipient is null)
                return base.NotFound("Recipient wallet not found");

            if (_databaseContext.Transactions.Any(t => t.GUID == transaction.GUID))
                return base.Conflict("Transaction GUID alredy exists. Try to resend data");

            if (_databaseContext.Transactions.Any(t =>
                t.TimeStamp == transaction.TimeStamp &&
                t.SenderWallet == transaction.SenderWallet &&
                t.RecipientWallet == transaction.RecipientWallet))
                return base.Conflict("Transaction alredy exists");

            try
            {
                _databaseContext.Transactions.Add(transaction);

                sender.DecreaseBalance(transaction.Amount);
                _databaseContext.Clients.Update(sender);

                recipient.ReplenishBalance(transaction.Amount);
                _databaseContext.Clients.Update(recipient);
                
                _databaseContext.SaveChanges();
            }
            catch (ArgumentOutOfRangeException)
            {
                return base.BadRequest("Sender balance lower than transaction amount!");
            }
            catch (Exception ex)
            {
                return base.StatusCode(500, "An error ocurred when saving database. " +
                    "Check transaction data and try again.\n" +
                    $"Error message: {ex.Message}");
            }

            return base.CreatedAtAction(nameof(Delete), transaction);
        }

        /// <summary>
        /// Deletes transaction by GUID
        /// </summary>
        /// <param name="transactionGUID">Transaction GUID</param>
        /// <response code="202">Transaction removed</response>
        /// <response code="404">Sended value doesn't match GUID standart</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{transactionGUID}", Name = "DeleteTransaction")]
        public IActionResult Delete([GuidValue] string transactionGUID)
        {
            var transaction = _databaseContext.Transactions.FirstOrDefault(t =>
            t.GUID == transactionGUID);

            if (transaction is null)
                return base.NotFound("Transaction not found");

            try
            {
                _databaseContext.Transactions.Remove(transaction);
                _databaseContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return base.StatusCode(500, "An error ocurred when saving database. " +
                    "Check transaction GUID and try again.\n" +
                    $"Error message: {ex.Message}");
            }

            return base.AcceptedAtAction(nameof(Delete), transaction);
        }
    }
}
