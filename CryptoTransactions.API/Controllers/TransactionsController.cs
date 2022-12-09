using CryptoTransactions.API.Model;
using CryptoTransactions.API.Model.Entities;
using CryptoTransactions.API.Model.Entities.QueryEntities;
using CryptoTransactions.API.Model.Repositories;
using CryptoTransactions.API.Model.Repositories.Interfaces;
using CryptoTransactions.API.Model.Validators;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CryptoTransactions.API.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public sealed class TransactionsController : ControllerBase
    {
        private readonly IRepository<Transaction, string> _repository;

        public TransactionsController()
        {
            _repository = new TransactionRepository(new CryptoTransactionsContext());
        }

        /// <summary>
        /// Returns transactions list
        /// </summary>
        /// <param name="transactionQuery">Transaction fields to filter</param>
        /// <param name="limit">Count of returned results (1 - 100)</param>
        /// <param name="offset">ID offset (starts from 0)</param>
        /// <response code="200">Successfully returned list</response>
        /// <response code="204">Transactions count equals zero</response>
        /// <response code="400">Limit must be lower than 100 and greather than 0</response>
        [HttpGet(Name = "GetAllTransactionsFiltered")]
        public async Task<IActionResult> GetAllFiltered([FromQuery] TransactionQuery transactionQuery,
            [Range(1, 100)] int limit = 20, [Range(0, int.MaxValue)] int offset = 0)
        {
            var transactions = await _repository.GetAllAsync(t =>
                    t.TimeStamp.Contains(transactionQuery.TimeStamp) &&
                    t.SenderWallet.Contains(transactionQuery.SenderWallet) &&
                    t.RecipientWallet.Contains(transactionQuery.RecipientWallet));

            transactions = transactions
                .Skip(offset)
                .Take(limit);

            return transactions.Any() ?
                base.Ok(transactions) :
                base.NoContent();
        }
        
        /// <summary>
        /// Returns transaction by GUID
        /// </summary>
        /// <param name="transactionGUID">Transaction GUID</param>
        /// <response code="200">Successfully returned transaction</response>
        /// <response code="404">Sended value doest match GUID standart</response>
        [HttpGet("{transactionGUID}", Name = "GetTransactionByGUID")]
        public async Task<IActionResult> GetTransactionByGUID([GuidValue] string transactionGUID)
        {
            var transaction = await _repository.GetByKeyDetailedAsync(transactionGUID);

            return transaction is not null ?
                base.Ok(transaction) :
                base.NotFound("Transaction not found");
        }

        /// <summary>
        /// Adds new transaction in database
        /// </summary>
        /// <param name="transaction">Transaction data</param>
        /// <response code="201">Successfully created</response>
        /// <response code="400">Request arguments errors</response>
        /// <response code="409">Creation error. Check and resend data</response>
        /// <response code="500">Internal server error</response>
        [HttpPost(Name = "AddTransaction")]
        public async Task<IActionResult> AddNew(Transaction transaction)
        {
            if (!transaction.IsValid())
                return base.BadRequest("Check transaction data and try again!");

            if (await _repository.HasAny(transaction.GUID))
                return base.Conflict("Transaction GUID alredy exists. Try to resend data");

            try
            {
                await _repository.CreateAsync(transaction);
                await _repository.SaveAsync();
            }
            catch (ArgumentOutOfRangeException)
            {
                return base.BadRequest("Sender balance lower than transaction amount!");
            }
            catch (ArgumentException ex)
            {
                return base.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return base.StatusCode(500, "An error ocurred when saving database. " +
                    "Check transaction data and try again.\n" +
                    $"Error message: {ex.Message}");
            }

            return base.CreatedAtAction(nameof(GetTransactionByGUID),
                new { transactionGUID = transaction.GUID }, transaction);
        }

        /// <summary>
        /// Deletes transaction by GUID
        /// </summary>
        /// <param name="transactionGUID">Transaction GUID</param>
        /// <response code="202">Transaction removed</response>
        /// <response code="404">Sended value doesn't match GUID standart</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{transactionGUID}", Name = "DeleteTransaction")]
        public async Task<IActionResult> Delete([GuidValue] string transactionGUID)
        {
            var transaction = await _repository.GetByKey(transactionGUID);

            if (transaction is null)
                return base.NotFound("Transaction not found");

            try
            {
                _repository.Delete(transaction);
                await _repository.SaveAsync();
            }
            catch (Exception ex)
            {
                return base.StatusCode(500, "An error ocurred when saving database. " +
                    "Check transaction GUID and try again.\n" +
                    $"Error message: {ex.Message}");
            }

            return base.AcceptedAtAction(nameof(GetTransactionByGUID),
                new { transactionGUID = transaction.GUID }, transaction);
        }
    }
}
