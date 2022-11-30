using CryptoTransactions.API.Model.Entities;
using CryptoTransactions.API.Model;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace CryptoTransactions.API.Controllers
{
    [ApiController]
    [Route("api/clients")]
    public class ClientsController : ControllerBase
    {
        /// <summary>
        /// Returns clients list
        /// </summary>
        /// <param name="limit">Count of returned results</param>
        /// <param name="offset">ID offset (starts from 0)</param>
        /// <response code="200">Successfully returned list</response>
        /// <response code="400">Limit must be lower than 50 and greather than 0</response>
        /// <response code="404">Clients count equals zero</response>
        [HttpGet(Name = "GetClientsList")]
        public IActionResult GetAll(int limit = 20, int offset = 0)
        {
            if (limit < 0 || limit > 50)
                base.BadRequest();

            using var dbContext = new CryptoTransactionsContext();
            var clients = dbContext.Clients.Skip(offset)
                .Take(limit)
                .ToList();

            if (clients.Count.Equals(0))
                return base.NotFound();

            return base.Ok(clients);
        }

        /// <summary>
        /// Returns client by wallet number
        /// </summary>
        /// <param name="walletNumber">Wallet number (GUID)</param>
        /// <response code="200">Successfully returned client</response>
        /// <response code="400">Sended value doest mach GUID standart</response>
        /// <response code="404">Client wallet number not found</response>
        [HttpGet("{walletNumber:guid}", Name = "GetClientByWalletNumber")]
        public IActionResult GetClientByWalletNumber(string walletNumber)
        {
            if (Guid.TryParse(walletNumber, out _) == false)
                return base.BadRequest();

            using var dbContext = new CryptoTransactionsContext();
            var client = dbContext.Clients.FirstOrDefault(c => c.WalletNumber == walletNumber);

            if (client is null)
                return base.NotFound();

            return base.Ok(client);
        }

        /// <summary>
        /// Adds new client in database
        /// </summary>
        /// <param name="client">Client data</param>
        /// <response code="201">Successfully created</response>
        /// <response code="409">Creation error. Try to resend data</response>
        [HttpPost(Name = "AddClient")]
        public IActionResult AddNew(Client client)
        {
            using var dbContext = new CryptoTransactionsContext();

            client.GenerateNewWalletNumber();

            if (dbContext.Clients.Any(c => c.WalletNumber == client.WalletNumber))
                return base.Conflict();

            dbContext.Clients.Add(client);
            dbContext.SaveChangesAsync();

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
        /// <response code="400">Sended value doest mach GUID standart</response>
        /// <response code="404">Client wallet number not found</response>
        [HttpDelete("{walletNumber}", Name = "DeleteClientByWalletNumber")]
        public IActionResult Delete(string walletNumber)
        {
            if (Guid.TryParse(walletNumber, out _) == false)
                return base.BadRequest();

            using var dbContext = new CryptoTransactionsContext();
            var client = dbContext.Clients.FirstOrDefault(c => c.WalletNumber == walletNumber);

            if (client is null)
                return base.NotFound();

            dbContext.Clients.Remove(client);
            dbContext.SaveChangesAsync();

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
        /// <response code="404">Wallet number not found</response>
        [HttpPut("{walletNumber}", Name = "UpdateClient")]
        public IActionResult Update(string walletNumber, Client client)
        {
            using var dbContext = new CryptoTransactionsContext();

            if (!dbContext.Clients.Any(c => c.WalletNumber == walletNumber))
                return base.NotFound();

            client.UpdateWalletNumber(walletNumber);

            dbContext.Update(client);
            dbContext.SaveChangesAsync();

            var location = Url.Action(nameof(Update), new { client = client.WalletNumber }) ??
                $"/{client.WalletNumber}";

            return base.Accepted(location, client);
        }
    }
}
