using CryptoTransactions.API.Model;
using CryptoTransactions.API.Model.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CryptoTransactions.API.Controllers
{
    [ApiController]
    [Route("api/routes")]
    public class TransactionsController : ControllerBase
    {
        /// <summary>
        /// Returns transactions list
        /// </summary>
        /// <param name="transactionQuery">Transaction fields to filter</param>
        /// <param name="limit">Count of returned results</param>
        /// <param name="offset">ID offset (starts from 0)</param>
        /// <response code="200">Successfully returned list</response>
        /// <response code="400">Limit must be lower than 100 and greather than 0</response>
        /// <response code="404">Transactions count equals zero</response>
        [HttpGet(Name = "GetAllTransactionsFiltered")]
        public IActionResult GetAllFiltered([FromQuery] TransactionQuery transactionQuery,
            int limit = 20, int offset = 0)
        {
            if (limit < 0 || limit > 100)
                base.BadRequest();

            using var dbContext = new CryptoTransactionsContext();
            IEnumerable<Transaction> transactions = dbContext.Transactions.ToList();

            if (!transactionQuery.IsEmpty())
                transactions = transactions.Where(t =>
                    t.TimeStamp.Contains(transactionQuery.TimeStamp) &&
                    t.SenderWallet.Contains(transactionQuery.SenderWallet) &&
                    t.RecipientWallet.Contains(transactionQuery.RecipientWallet) &&
                    t.CurrencyType.Contains(transactionQuery.CurrencyType) &&
                    t.TransactionType.Contains(transactionQuery.TransactionType));

            transactions = transactions.Skip(offset)
                .Take(limit)
                .ToList();

            if (!transactions.Any())
                return base.NotFound();

            return base.Ok(transactions);
        }

        [HttpPost(Name = "AddTransaction")]
        public IActionResult AddNew(Transaction transaction)
        {
            using var dbContext = new CryptoTransactionsContext();

            if(dbContext.Transactions.Any(t =>
                t.TimeStamp == transaction.TimeStamp &&
                t.SenderWallet == transaction.SenderWallet &&
                t.RecipientWallet == transaction.RecipientWallet))
                    return base.Conflict();

            dbContext.Transactions.Add(transaction);
            dbContext.SaveChangesAsync();

            var location = Url.Action(nameof(AddNew), new { transaction = transaction.TimeStamp }) ??
                $"/{transaction.TimeStamp}";

            return base.Created(location, transaction);
        }
    }
}
