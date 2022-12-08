using CryptoTransactions.API.Model;
using CryptoTransactions.API.Model.Entities;
using CryptoTransactions.API.Model.Entities.QueryEntities;
using CryptoTransactions.API.Model.Repositories;
using CryptoTransactions.API.Model.Repositories.Interfaces;
using CryptoTransactions.API.Model.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CryptoTransactions.API.Controllers
{
    [ApiController]
    [Route("api/clients")]
    public sealed class ClientsController : ControllerBase
    {
        private readonly IRepository<Client, string> _repository;

        public ClientsController()
        {
            _repository = new ClientRepository(new CryptoTransactionsContext());
        }

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
        public async Task<IActionResult> GetAllFiltered([FromQuery] ClientQuery clientQuery, [Range(1, 100)] int limit = 20,
            [Range(0, int.MaxValue)] int offset = 0)
        {
            var clients = await _repository.GetAllAsync(c =>
                c.Surname.ToLower().Contains(clientQuery.Surname.ToLower()) &&
                c.Name.ToLower().Contains(clientQuery.Name.ToLower()) &&
                c.Patronymic.ToLower().Contains(clientQuery.Patronymic.ToLower()));
                
            clients = clients
                .Skip(offset)
                .Take(limit);

            return clients.Any() ?
                base.Ok(clients) :
                base.NoContent();
        }

        /// <summary>
        /// Returns client by wallet number
        /// </summary>
        /// <param name="walletNumber">Wallet number (GUID)</param>
        /// <response code="200">Successfully returned client</response>
        /// <response code="404">Wallet number doesn't match GUID standart</response>
        [HttpGet("{walletNumber}", Name = "GetClientByWalletNumber")]
        public async Task<IActionResult> GetClientByWalletNumber([GuidValue] string walletNumber)
        {
            var client = await _repository.GetByKeyDetailedAsync(walletNumber);

            return client is not null ?
                base.Ok(client) :
                base.NotFound("Client not found");
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
        public async Task<IActionResult> GetClientTransactions([GuidValue] string walletNumber, [Range(1, 100)] int limit = 20,
            [Range(0, int.MaxValue)] int offset = 0)
        {
            var clientDetailed = await _repository.GetByKeyDetailedAsync(walletNumber);

            if (clientDetailed is null)
                return base.NotFound("Client not found!");

            var transactions = clientDetailed.Transactions
                .Skip(offset)
                .Take(limit);

            return clientDetailed.Transactions.Any() ?
                base.Ok(transactions) :
                base.NoContent();
        }

        /// <summary>
        /// Returns client transactions by wallet number
        /// </summary>
        /// <param name="walletNumber">Wallet number (GUID)</param>
        /// <param name="transactionGUID">Transaction key (GUID)</param>
        /// <response code="200">Successfully returned transactions</response>
        /// <response code="404">Wallet number doesn't match GUID standart</response>
        [HttpGet("{walletNumber}/transactions/{transactionGUID}", Name = "GetClientTransactionByKey")]
        public async Task<IActionResult> GetClientTransactionByKey([GuidValue] string walletNumber,
            [GuidValue] string transactionGUID)
        {
            var clientDetailed = await _repository.GetByKeyDetailedAsync(walletNumber);

            if (clientDetailed is null)
                return base.NotFound("Client not found!");

            var transaction = clientDetailed.Transactions
                .FirstOrDefault(t => t.GUID.Equals(transactionGUID));

            return transaction is not null ? 
                base.LocalRedirect($"~/api/transactions/{transaction.GUID}") :
                base.NotFound("Client transaction not found");
        }

        /// <summary>
        /// Adds new client in database
        /// </summary>
        /// <param name="client">Client data</param>
        /// <response code="201">Successfully created</response>
        /// <response code="409">Creation error. Try to resend data</response>
        /// <response code="500">Internal server error</response>
        [HttpPost(Name = "AddClient")]
        public async Task<IActionResult> AddNew(Client client)
        {
            if (await _repository.HasAny(client.WalletNumber))
                return base.Conflict("Wallet number alredy exists. Try to resend data");

            try
            {
                await _repository.CreateAsync(client);
                await _repository.SaveAsync();
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
        public async Task<IActionResult> Delete([GuidValue] string walletNumber)
        {
            var client = await _repository.GetByKey(walletNumber);

            if (client is null)
                return base.NotFound("Client not found");

            try
            {
                _repository.Delete(client);
                await _repository.SaveAsync();
            }
            catch (DbUpdateException ex)
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
        public async Task<IActionResult> Update([GuidValue] string walletNumber, Client client)
        {
            if (!await _repository.HasAny(walletNumber))
                return base.NotFound("Client not found");

            client.SetWalletNumber(walletNumber);

            try
            {
                _repository.Update(client);
                await _repository.SaveAsync();
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
