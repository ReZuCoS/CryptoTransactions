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
        [SetUp]
        public void Setup() =>
            CryptoTransactionsContext.ConnectionString = $"Data source = " +
            $"{Environment.CurrentDirectory}\\SampleData\\CryptoTransactionsDEBUG.db";

        [Order(1)]
        [TestCase(200, 5)]
        [TestCase(200, 3, 3, 2)]
        [TestCase(200, 2, 5, 0, "Mendez")]
        [TestCase(200, 1, 5, 0, "", "Owen", "Smith")]
        public void GetClientsFiltered_Positive(int expectedCode,
            int expectedResultsCount,
            int limit = 5, int offset = 0,
            string surname = "", string name = "", string patronymic = "")
        {
            var query = new ClientQuery()
            {
                Surname = surname,
                Name = name,
                Patronymic = patronymic
            };

            var controller = new ClientsController();
            var result = controller.GetAllFiltered(query, limit, offset);

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

            var controller = new ClientsController();
            var result = controller.GetAllFiltered(query, limit, offset);

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

            var controller = new ClientsController();
            var result = controller.GetAllFiltered(query, limit, offset);

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

            var controller = new ClientsController();
            var result = controller.GetClientByWalletNumber(walletNumber);

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
            var controller = new ClientsController();
            var result = controller.GetClientByWalletNumber(walletNumber);

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
            var controller = new ClientsController();
            var result = controller.GetClientTransactions(walletNumber, limit, offset);

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
            var controller = new ClientsController();
            var result = controller.GetClientTransactions(walletNumber, limit, offset);

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
            var controller = new ClientsController();
            var result = controller.GetClientTransactions(walletNumber, limit, offset);

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
            var controller = new ClientsController();
            var result = controller.GetClientTransactionByKey(walletNumber, transactionGUID);

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
            var controller = new ClientsController();
            var result = controller.GetClientTransactionByKey(walletNumber, transactionGUID);

            var statusResult = result as ObjectResult;

            Assert.That(statusResult.StatusCode, Is.EqualTo(expectedCode));
        }

        [Order(11)]
        [TestCase(201, "Kevin", "Ethan", "Roberts")]
        public void AddNewClient_Positive(int expectedCode,
            string surname, string name,
            string patronymic)
        {
            var client = new Client()
            {
                Surname = surname,
                Name = name,
                Patronymic = patronymic
            };

            var controller = new ClientsController();
            var result = controller.AddNew(client);

            var statusResult = result as ObjectResult;

            using var dbContext = new CryptoTransactionsContext();

            Assert.Multiple(() =>
            {
                Assert.That(statusResult.StatusCode, Is.EqualTo(expectedCode));
                Assert.That(dbContext.Clients.Contains(client), Is.True);
            });
        }

        public void AddNewClient_Negative()
        {

        }
    }
}
