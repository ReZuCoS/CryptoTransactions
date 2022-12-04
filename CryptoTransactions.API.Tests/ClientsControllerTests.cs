using CryptoTransactions.API.Controllers;
using CryptoTransactions.API.Model;
using CryptoTransactions.API.Model.Entities;
using CryptoTransactions.API.Model.Entities.QueryEntities;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace CryptoTransactions.API.Tests
{
    public class ClientsControllerTests
    {
        private static readonly ClientsController TestController = new();

        [SetUp]
        public void Setup() =>
            CryptoTransactionsContext.ConnectionString = $"Data source = " +
            $"{Environment.CurrentDirectory}\\SampleData\\CryptoTransactionsDEBUG.db";

        #region GET_Tests
        [Order(1)]
        [TestCase(200, 5)]
        [TestCase(200, 3, 3, 2)]
        [TestCase(200, 2, 5, 0, "Mendez")]
        [TestCase(200, 1, 5, 0, "", "Owen", "Smith")]
        public void GetClientsFiltered_Positive(int expectedCode,
            int expectedResultsCount, int limit = 5, int offset = 0,
            string surname = "", string name = "", string patronymic = "")
        {
            var query = new ClientQuery()
            {
                Surname = surname,
                Name = name,
                Patronymic = patronymic
            };

            var result = TestController.GetAllFiltered(query, limit, offset);

            var okResult = result as ObjectResult;
            var okList = okResult.Value as IEnumerable<Client>;

            Assert.Multiple(() =>
            {
                Assert.That(okList.Count(), Is.EqualTo(expectedResultsCount));
                Assert.That(okResult.StatusCode, Is.EqualTo(expectedCode));
            });
        }

        [Order(2)]
        [TestCase(204, 0, 0)]
        [TestCase(204, 5, 5)]
        [TestCase(204, 5, 0, "Smith")]
        [TestCase(204, 5, 0, "Rick", "Buffer")]
        public void GetClientsFiltered_NoContent(int expectedCode,
            int limit = 5, int offset = 0,
            string surname = "", string name = "", string patronymic = "")
        {
            var query = new ClientQuery()
            {
                Surname = surname,
                Name = name,
                Patronymic = patronymic
            };

            var result = TestController.GetAllFiltered(query, limit, offset);

            var okResult = result as StatusCodeResult;

            Assert.That(okResult.StatusCode, Is.EqualTo(expectedCode));
        }

        [Order(3)]
        [TestCase(400, -1, 0)]
        [TestCase(400, 150, 0)]
        public void GetClientsFiltered_Negative(int expectedCode,
            int limit = 5, int offset = 0,
            string surname = "", string name = "", string patronymic = "")
        {
            var query = new ClientQuery()
            {
                Surname = surname,
                Name = name,
                Patronymic = patronymic
            };

            var result = TestController.GetAllFiltered(query, limit, offset);

            var statusResult = result as ObjectResult;

            Assert.That(statusResult.StatusCode, Is.EqualTo(expectedCode));
        }
        
        [Order(4)]
        [Test]
        public void GetClientByWalletNumber_Positive()
        {
            int expectedCode = 200;
            string walletNumber = "d0630000-5d0f-0015-2872-08da3058ad5a";

            var expectedClient = new Client()
            {
                Surname = "Mendez",
                Name = "Anthony",
                Patronymic = "Morgan"
            };

            var result = TestController.GetClientByWalletNumber(walletNumber);

            var statusResult = result as ObjectResult;
            var client = statusResult.Value as Client;

            Assert.Multiple(() =>
            {
                Assert.That(statusResult.StatusCode, Is.EqualTo(expectedCode));
                Assert.That(client.Name, Is.EqualTo(expectedClient.Name));
                Assert.That(client.Surname, Is.EqualTo(expectedClient.Surname));
                Assert.That(client.Patronymic, Is.EqualTo(expectedClient.Patronymic));
            });
        }

        [Order(5)]
        [TestCase(400, "NOT-GUID-VALUE-ID")]
        [TestCase(404, "d3630000-5d0f-0015-ed68-08da3058ad5c")]
        public void GetClientByWalletNumber_Negative(int expectedCode,
            string walletNumber)
        {
            var result = TestController.GetClientByWalletNumber(walletNumber);

            var statusResult = result as ObjectResult;

            Assert.That(statusResult.StatusCode, Is.EqualTo(expectedCode));
        }

        [Order(6)]
        [TestCase(200, 1, "d0630000-5d0f-0015-2872-08da3058ad5a", 1, 0)]
        [TestCase(200, 2, "d2630000-5d0f-0015-2872-08da3058ad5a", 2, 0)]
        [TestCase(200, 1, "d2630000-5d0f-0015-2872-08da3058ad5a", 1, 1)]
        public void GetClientTransactions_Positive(int expectedCode,
            int expectedResultsCount, string walletNumber,
            int limit = 5, int offset = 0)
        {
            var result = TestController.GetClientTransactions(walletNumber, limit, offset);

            var statusResult = result as ObjectResult;
            var transactions = statusResult.Value as IEnumerable<Transaction>;

            Assert.Multiple(() =>
            {
                Assert.That(statusResult.StatusCode, Is.EqualTo(expectedCode));
                Assert.That(transactions.Count(), Is.EqualTo(expectedResultsCount));
            });
        }

        [Order(7)]
        [TestCase(204, "d3630000-5d0f-0015-2872-08da3058ad5a", 5, 0)]
        [TestCase(204, "d2630000-5d0f-0015-2872-08da3058ad5a", 1, 2)]
        public void GetClientTransactions_NoContent(int expectedCode,
            string walletNumber,
            int limit = 5, int offset = 0)
        {
            var result = TestController.GetClientTransactions(walletNumber, limit, offset);

            var okResult = result as StatusCodeResult;

            Assert.That(okResult.StatusCode, Is.EqualTo(expectedCode));
        }

        [Order(8)]
        [TestCase(400, "NOT-GUID-VALUE-ID")]
        [TestCase(400, "d3630000-5d0f-0015-ed68-08da3058ad5c", 120, 0)]
        [TestCase(400, "d3630000-5d0f-0015-ed68-08da3058ad5c", -5, 0)]
        public void GetClientTransactions_Negative(int expectedCode,
            string walletNumber, int limit = 5, int offset = 0)
        {
            var result = TestController.GetClientTransactions(walletNumber, limit, offset);

            var statusResult = result as ObjectResult;

            Assert.That(statusResult.StatusCode, Is.EqualTo(expectedCode));
        }

        [Order(9)]
        [TestCase("~/api/transactions/d0630000-5d0f-2015-2872-08da3058ad5a",
            "d0630000-5d0f-0015-ed68-08da3058ad5c",
            "d0630000-5d0f-2015-2872-08da3058ad5a")]
        public void GetClientTransactionByGuid_Positive(string expectedURL,
            string walletNumber, string transactionGUID)
        {
            var result = TestController.GetClientTransactionByKey(walletNumber, transactionGUID);

            var statusResult = result as LocalRedirectResult;

            Assert.That(statusResult.Url, Is.EqualTo(expectedURL));
        }

        [Order(10)]
        [TestCase(400, "NON-GUID-VALUE",
            "d0630000-5d0f-2015-2872-08da3058ad5a")]
        [TestCase(400, "d0630000-5d0f-0015-ed68-08da3058ad5c",
            "NON-GUID-VALUE")]
        [TestCase(404, "d0630000-5d0f-0015-ed68-08da3058ad5c",
            "d0630000-5d0f-0015-ed68-08da3058ad5c")]
        public void GetClientTransactionByGuid_Negative(int expectedCode, string walletNumber,
            string transactionGUID)
        {
            var result = TestController.GetClientTransactionByKey(walletNumber, transactionGUID);

            var statusResult = result as ObjectResult;

            Assert.That(statusResult.StatusCode, Is.EqualTo(expectedCode));
        }
        #endregion

        #region POST_Tests
        [Order(11)]
        [TestCase(201, "Kevin", "Ethan", "Roberts")]
        [TestCase(201, "Alex", "Brandon", "Howard")]
        public void AddNewClient_Positive(int expectedCode,
            string surname, string name, string patronymic)
        {
            var client = new Client()
            {
                Surname = surname,
                Name = name,
                Patronymic = patronymic
            };

            var result = TestController.AddNew(client);

            var statusResult = result as ObjectResult;

            using var dbContext = new CryptoTransactionsContext();

            Assert.Multiple(() =>
            {
                Assert.That(statusResult.StatusCode, Is.EqualTo(expectedCode));
                Assert.That(dbContext.Clients.Contains(client), Is.True);
            });
        }

        [Order(12)]
        [TestCase(409, "d0630000-5d0f-0015-2872-08da3058ad5a")]
        [TestCase(409, "d1630000-5d0f-0015-2872-08da3058ad5a")]
        [TestCase(409, "d4630000-5d0f-0015-2872-08da3058ad5a")]
        public void AddNewClient_Negative(int expectedCode, string existWalletNumber)
        {
            var client = new Client()
            {
                Surname = "Temporary",
                Name = "Temporary",
                Patronymic = "Temporary"
            };

            client.SetWalletNumber(existWalletNumber);

            var result = TestController.AddNew(client);

            var statusResult = result as ObjectResult;

            using var dbContext = new CryptoTransactionsContext();

            Assert.Multiple(() =>
            {
                Assert.That(statusResult.StatusCode, Is.EqualTo(expectedCode));
                Assert.That(dbContext.Clients.Contains(client), Is.True);
            });
        }
        #endregion

        #region DELETE_Tests
        [Order(13)]
        [TestCase(202, "d3630000-5d0f-0015-2872-08da3058ad5a")]
        public void DeleteClient_Positive(int expectedCode, string existWalletNumber)
        {
            var result = TestController.Delete(existWalletNumber);

            var statusResult = result as ObjectResult;

            using var dbContext = new CryptoTransactionsContext();

            Assert.Multiple(() =>
            {
                Assert.That(statusResult.StatusCode, Is.EqualTo(expectedCode));
                Assert.That(dbContext.Clients.Any(c => c.WalletNumber == existWalletNumber), Is.False);
            });
        }

        [Order(14)]
        [TestCase(400, "NOT-GUID-VALUE")]
        [TestCase(404, "aaaaaaaa-5d0f-0015-2872-08da3058ad5a")]
        [TestCase(409, "d0630000-5d0f-0015-2872-08da3058ad5a")]
        public void DeleteClient_Negative(int expectedCode, string walletNumber)
        {
            var result = TestController.Delete(walletNumber);
        
            var statusResult = result as ObjectResult;
        
            Assert.That(statusResult.StatusCode, Is.EqualTo(expectedCode));
        }
        #endregion

        #region PUT_Tests
        [Order(15)]
        [TestCase(202, "d0630000-5d0f-0015-2872-08da3058ad5a",
            "Updated", "Updated", "Updated")]
        [TestCase(202, "d1630000-5d0f-0015-2872-08da3058ad5a",
            "Updated", "Updated", "Updated")]
        public void UpdateClient_Positive(int expectedCode, string walletNumber,
            string surname, string name, string patronymic)
        {
            var clientQuery = new Client()
            {
                Surname = surname,
                Name = name,
                Patronymic = patronymic
            };

            var result = TestController.Update(walletNumber, clientQuery);

            var statusResult = result as ObjectResult;

            using var dbContext = new CryptoTransactionsContext();
            var updatedClient = dbContext.Clients.FirstOrDefault(c =>
                c.WalletNumber == walletNumber);

            Assert.Multiple(() =>
            {
                Assert.That(statusResult.StatusCode, Is.EqualTo(expectedCode));
                Assert.That(updatedClient.Surname, Is.EqualTo(clientQuery.Surname));
                Assert.That(updatedClient.Name, Is.EqualTo(clientQuery.Name));
                Assert.That(updatedClient.Patronymic, Is.EqualTo(clientQuery.Patronymic));
            });
        }

        [Order(16)]
        [TestCase(400, "NOT-GUID-VALUE")]
        [TestCase(404, "aaaaaaaa-5d0f-0015-2872-08da3058ad5a")]
        public void UpdateClient_Negative(int expectedCode, string walletNumber)
        {
            var result = TestController.Update(walletNumber, null);

            var statusResult = result as ObjectResult;

            using var dbContext = new CryptoTransactionsContext();
            var updatedClient = dbContext.Clients.FirstOrDefault(c =>
                c.WalletNumber == walletNumber);

            Assert.That(statusResult.StatusCode, Is.EqualTo(expectedCode));
        }
        #endregion
    }
}
