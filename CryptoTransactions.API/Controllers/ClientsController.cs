using CryptoTransactions.API.Model.Entities;
using CryptoTransactions.API.Model.Entities.QueryEntities;
using CryptoTransactions.API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using CryptoTransactions.API.Model.Validators;

namespace CryptoTransactions.API.Controllers
{
    [ApiController]
    [Route("api/clients")]
    public sealed class ClientsController : ControllerBase
    {
        private readonly CryptoTransactionsContext _databaseContext = new();

        /// <summary>
        /// Returns clients list
        /// </summary>
        /// <param name="clientQuery">Client fields to filter</param>
        /// <param name="limit">Count of returned results (1 - 100)</param>
        /// <param name="offset">ID offset (starts from 0)</param>
        /// <response code="200">Successfully returned list</response>
        /// <response code="204">Clients count equals zero</response>
        /// <response code="400">Limit must be lower than 100 and greather than 0</response>
        [HttpGet(Name = "GetClientsListWithFilter")]
        public IActionResult GetAllFiltered([FromQuery] ClientQuery clientQuery, [Range(1, 100)] int limit = 20,
            [Range(0, int.MaxValue)] int offset = 0)
        {
            var clients = _databaseContext.Clients.ToList();

            if (!clientQuery.IsEmpty())
                clients = clients.Where(c =>
                    c.Surname.ToLower().Contains(clientQuery.Surname.ToLower()) &&
                    c.Name.ToLower().Contains(clientQuery.Name.ToLower()) &&
                    c.Patronymic.ToLower().Contains(clientQuery.Patronymic.ToLower())).ToList();

            clients = clients
                .Skip(offset)
                .Take(limit)
                .ToList();

            if (!clients.Any())
                return base.NoContent();
            
            return base.Ok(clients);
        }

        /// <summary>
        /// Returns client by wallet number
        /// </summary>
        /// <param name="walletNumber">Wallet number (GUID)</param>
        /// <response code="200">Successfully returned client</response>
        /// <response code="404">Wallet number doesn't match GUID standart</response>
        [HttpGet("{walletNumber}", Name = "GetClientByWalletNumber")]
        public IActionResult GetClientByWalletNumber([GuidValue] string walletNumber)
        {
            var client = _databaseContext.Clients.FirstOrDefault(c => c.WalletNumber == walletNumber);

            if (client is null)
                return base.NotFound("Client not found");

            return base.Ok(client);
        }

        /// <summary>
        /// Returns client transactions by wallet number
        /// </summary>
        /// <param name="walletNumber">Wallet number (GUID)</param>
        /// <param name="limit">Count of returned results (1 - 100)</param>
        /// <param name="offset">ID offset (starts from 0)</param>
        /// <response code="200">Successfully returned transactions</response>
        /// <response code="204">Client's transactions count equals zero</response>
        /// <response code="404">Wallet number doesn't match GUID standart</response>
        [HttpGet("{walletNumber}/transactions", Name = "GetClientTransactions")]
        public IActionResult GetClientTransactions([GuidValue] string walletNumber, [Range(1, 100)] int limit = 20,
            [Range(0, int.MaxValue)] int offset = 0)
        {
            var transactions = _databaseContext.Transactions
                .Where(t => t.SenderWallet == walletNumber || t.RecipientWallet == walletNumber)
                .Include(t => t.Sender)
                .Include(t => t.Recipient)
                .Skip(offset)
                .Take(limit)
                .ToList();
            
            if (!transactions.Any())
                return base.NoContent();

            return base.Ok(transactions);
        }

        /// <summary>
        /// Returns client transactions by wallet number
        /// </summary>
        /// <param name="walletNumber">Wallet number (GUID)</param>
        /// <param name="transactionGUID">Transaction key (GUID)</param>
        /// <response code="200">Successfully returned transactions</response>
        /// <response code="404">Wallet number doesn't match GUID standart</response>
        [HttpGet("{walletNumber}/transactions/{transactionGUID}", Name = "GetClientTransactionByKey")]
        public IActionResult GetClientTransactionByKey([GuidValue] string walletNumber,
            [GuidValue] string transactionGUID)
        {
            var client = _databaseContext.Clients
                .Include(c => c.SentTransactions)
                .Include(c => c.ReceivedTransactions)
                .FirstOrDefault(c => c.WalletNumber.Equals(walletNumber));
                
            if (client is null)
                return base.NotFound("Client not found");

            var transaction = client.Transactions
                .FirstOrDefault(t => t.GUID.Equals(transactionGUID));

            if (transaction is null)
                return base.NotFound("Client transaction not found");

            return base.LocalRedirect($"~/api/transactions/{transaction.GUID}");
        }

        /// <summary>
        /// Adds new client in database
        /// </summary>
        /// <param name="client">Client data</param>
        /// <response code="201">Successfully created</response>
        /// <response code="409">Creation error. Try to resend data</response>
        /// <response code="500">Internal server error</response>
        [HttpPost(Name = "AddClient")]
        public IActionResult AddNew(Client client)
        {
            if (_databaseContext.Clients.Any(c => c.WalletNumber == client.WalletNumber))
                return base.Conflict("Wallet number alredy exists. Try to resend data");

            try
            {
                _databaseContext.Clients.Add(client);
                _databaseContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return base.StatusCode(500, "An error ocurred when saving database. " +
                    "Check client data and try again.\n" +
                    $"Error message: {ex.Message}");
            }

            return base.CreatedAtAction(nameof(GetClientByWalletNumber),
                new { client.WalletNumber }, client);
        }

        /// <summary>
        /// Deletes client by wallet number
        /// </summary>
        /// <param name="walletNumber">Wallet number (GUID)</param>
        /// <response code="200">Success</response>
        /// <response code="202">Client removed</response>
        /// <response code="404">Sended value doest match GUID standart</response>
        /// <response code="409">Client has one or more transactions</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{walletNumber}", Name = "DeleteClientByWalletNumber")]
        public IActionResult Delete([GuidValue] string walletNumber)
        {
            var client = _databaseContext.Clients.FirstOrDefault(c => c.WalletNumber == walletNumber);

            if (client is null)
                return base.NotFound("Client not found");

            try
            {
                _databaseContext.Clients.Remove(client);
                _databaseContext.SaveChanges();
            }
            catch(DbUpdateException ex)
            {
                return base.Conflict("An error occurred while deleting " +
                    "a client from the database. You cannot remove client, " +
                    "which has one or more transactions" +
                    $"Error message: {ex.Message}");
            }
            catch (Exception ex)
            {
                return base.StatusCode(500, "An error ocurred when saving database. " +
                    "Check client wallet number and try again.\n" +
                    $"Error message: {ex.Message}");
            }

            return base.AcceptedAtAction(nameof(GetClientByWalletNumber),
                new { client.WalletNumber }, client);
        }

        /// <summary>
        /// Updates client data
        /// </summary>
        /// <param name="walletNumber">Client wallet number which is going to update (GUID)</param>
        /// <param name="client">Client data</param>
        /// <response code="200">Success</response>
        /// <response code="202">Client updated</response>
        /// <response code="404">Sended value doesn't match GUID standart</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("{walletNumber}", Name = "UpdateClient")]
        public IActionResult Update([GuidValue] string walletNumber, Client client)
        {
            if (!_databaseContext.Clients.Any(c => c.WalletNumber.Equals(walletNumber)))
                return base.NotFound("Client not found");

            client.SetWalletNumber(walletNumber);

            try
            {
                _databaseContext.Clients.Update(client);
                _databaseContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return base.StatusCode(500, "An error ocurred when saving database. " +
                    "Check client data and try again.\n" +
                    $"Error message: {ex.Message}");
            }

            return base.AcceptedAtAction(nameof(GetClientByWalletNumber),
                new { client.WalletNumber }, client);
        }
    }
}
