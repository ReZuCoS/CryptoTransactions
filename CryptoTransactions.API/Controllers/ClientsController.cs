using CryptoTransactions.API.Model.Entities;
using CryptoTransactions.API.Model.Entities.QueryEntities;
using CryptoTransactions.API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CryptoTransactions.API.Controllers
{
    [ApiController]
    [Route("api/clients")]
    public sealed class ClientsController : ControllerBase
    {
        /// <summary>
        /// Returns clients list
        /// </summary>
        /// <param name="clientQuery">Client fields to filter</param>
        /// <param name="limit">Count of returned results</param>
        /// <param name="offset">ID offset (starts from 0)</param>
        /// <response code="200">Successfully returned list</response>
        /// <response code="204">Clients count equals zero</response>
        /// <response code="400">Limit must be lower than 100 and greather than 0</response>
        [HttpGet(Name = "GetClientsListWithFilter")]
        public IActionResult GetAllFiltered([FromQuery] ClientQuery clientQuery, int limit = 20,
            int offset = 0)
        {
            if (limit < 0 || limit > 100)
                return base.BadRequest("Limit value must be in range (0 - 100)");

            using var dbContext = new CryptoTransactionsContext();
            IEnumerable<Client> clients = dbContext.Clients.ToList();

            if (!clientQuery.IsEmpty())
                clients = clients.Where(c =>
                    c.Surname.ToLower().Contains(clientQuery.Surname.ToLower()) &&
                    c.Name.ToLower().Contains(clientQuery.Name.ToLower()) &&
                    c.Patronymic.ToLower().Contains(clientQuery.Patronymic.ToLower()));

            clients = clients.Skip(offset)
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
        /// <response code="400">Sended value doest match GUID standart</response>
        /// <response code="404">Client wallet number not found</response>
        [HttpGet("{walletNumber}", Name = "GetClientByWalletNumber")]
        public IActionResult GetClientByWalletNumber(string walletNumber)
        {
            if (Guid.TryParse(walletNumber, out _) == false)
                return base.BadRequest("Sended wallet number doest match GUID standart");

            using var dbContext = new CryptoTransactionsContext();
            var client = dbContext.Clients.FirstOrDefault(c => c.WalletNumber == walletNumber);

            if (client is null)
                return base.NotFound("Client not found");

            return base.Ok(client);
        }

        /// <summary>
        /// Returns client transactions by wallet number
        /// </summary>
        /// <param name="walletNumber">Wallet number (GUID)</param>
        /// <param name="limit">Count of returned results</param>
        /// <param name="offset">ID offset (starts from 0)</param>
        /// <response code="200">Successfully returned transactions</response>
        /// <response code="204">Client's transactions count equals zero</response>
        /// <response code="400">Sended value doest match GUID standart</response>
        /// <response code="404">Client wallet number not found</response>
        [HttpGet("{walletNumber}/transactions", Name = "GetClientTransactions")]
        public IActionResult GetClientTransactions(string walletNumber, int limit = 20,
            int offset = 0)
        {
            if (limit < 0 || limit > 100)
                return base.BadRequest("Limit value must be in range (0 - 100)");

            if (Guid.TryParse(walletNumber, out _) == false)
                return base.BadRequest("Sended wallet number doest match GUID standart");

            using var dbContext = new CryptoTransactionsContext();
            var transactions = dbContext.Transactions.Where(t =>
                t.SenderWallet == walletNumber ||
                t.RecipientWallet == walletNumber)
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
        /// <param name="transactionGUID">Transaction key parameters, separated by '$' symbol</param>
        /// <response code="200">Successfully returned transactions</response>
        /// <response code="400">Sended value doest match GUID standart</response>
        /// <response code="404">Client wallet number not found</response>
        [HttpGet("{walletNumber}/transactions/{transactionGUID}", Name = "GetClientTransactionByKey")]
        public IActionResult GetClientTransactionByKey(string walletNumber,
            string transactionGUID)
        {
            if (Guid.TryParse(walletNumber, out _) == false)
                return base.BadRequest("Sended wallet number doest match GUID standart");
            if (Guid.TryParse(transactionGUID, out _) == false)
                return base.BadRequest("Sended transactionGUID number doest match GUID standart");

            using var dbContext = new CryptoTransactionsContext();
            var transaction = dbContext.Transactions.FirstOrDefault(t =>
                t.GUID == transactionGUID);

            if (transaction is null)
                return base.NotFound("Transaction not found");

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
            using var dbContext = new CryptoTransactionsContext();

            if (dbContext.Clients.Any(c => c.WalletNumber == client.WalletNumber))
                return base.Conflict("Wallet number alredy exists. Try to resend data");

            try
            {
                dbContext.Clients.Add(client);
                dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return base.StatusCode(500, "An error ocurred when saving database. " +
                    "Check client data and try again.\n" +
                    $"Error message: {ex.Message}");
            }

            var location = Url.Action(nameof(AddNew), new { client = client.WalletNumber }) ??
                $"/{client.WalletNumber}";

            return base.Created(location, client);
        }

        /// <summary>
        /// Deletes client by wallet number
        /// </summary>
        /// <param name="walletNumber">Wallet number (GUID)</param>
        /// <response code="200">Success</response>
        /// <response code="202">Client removed</response>
        /// <response code="400">Sended value doesn't match GUID standart</response>
        /// <response code="404">Client wallet number not found</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{walletNumber}", Name = "DeleteClientByWalletNumber")]
        public IActionResult Delete(string walletNumber)
        {
            if (Guid.TryParse(walletNumber, out _) == false)
                return base.BadRequest("Sended wallet number doesn't match GUID standart");

            using var dbContext = new CryptoTransactionsContext();
            var client = dbContext.Clients.FirstOrDefault(c => c.WalletNumber == walletNumber);

            if (client is null)
                return base.NotFound("Client not found");
            try
            {
                dbContext.Clients.Remove(client);
                dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return base.StatusCode(500, "An error ocurred when saving database. " +
                    "Check client wallet number and try again.\n" +
                    $"Error message: {ex.Message}");
            }

            var location = Url.Action(nameof(Delete), new { client = client.WalletNumber }) ??
                $"/{client.WalletNumber}";

            return base.Accepted(location);
        }

        /// <summary>
        /// Updates client data
        /// </summary>
        /// <param name="walletNumber">Client wallet number which is going to update</param>
        /// <param name="client">Client data</param>
        /// <response code="200">Success</response>
        /// <response code="202">Client updated</response>
        /// <response code="400">Sended value doesn't match GUID standart</response>
        /// <response code="404">Wallet number not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("{walletNumber}", Name = "UpdateClient")]
        public IActionResult Update(string walletNumber, Client client)
        {
            if (Guid.TryParse(walletNumber, out _) == false)
                return base.BadRequest("Sended wallet number doesn't match GUID standart");

            using var dbContext = new CryptoTransactionsContext();

            if (!dbContext.Clients.Any(c => c.WalletNumber == walletNumber))
                return base.NotFound("Client not found");

            client.SetWalletNumber(walletNumber);

            try
            {
                dbContext.Clients.Update(client);
                dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return base.StatusCode(500, "An error ocurred when saving database. " +
                    "Check client data and try again.\n" +
                    $"Error message: {ex.Message}");
            }

            string link = client.WalletNumber;
            var location = Url.Action(nameof(Update), new { client = link }) ??
                $"/{link}";

            return base.Accepted(location, client);
        }
    }
}
