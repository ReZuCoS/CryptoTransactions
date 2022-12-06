using CryptoTransactions.API.Controllers;
using CryptoTransactions.API.Model;
using CryptoTransactions.API.Model.Entities;
using CryptoTransactions.API.Model.Entities.QueryEntities;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace CryptoTransactions.API.Tests
{
    [Order(0)]
    public class TransactionsControllerTests : ControllerTestBase
    {
        private static readonly TransactionsController TestController = new();

        #region GET_Tests
        [Order(1)]
        [TestCase(200, 4)]
        [TestCase(200, 3, 3, 1)]
        [TestCase(200, 2, 3, 0, "02.12.2022 16:25:35")]
        [TestCase(200, 1, 3, 1, "", "", "d4630000-5d0f-0015-2872-08da3058ad5a")]
        public void GetTransactionsFiltered_Positive(int expectedCode,
            int expectedResultsCount, int limit = 20, int offset = 0,
            string timeStamp = "", string senderWallet = "",
            string recipientWallet = "")
        {
            var query = new TransactionQuery()
            {
                TimeStamp = timeStamp,
                SenderWallet = senderWallet,
                RecipientWallet = recipientWallet
            };

            var result = TestController.GetAllFiltered(query, limit, offset);
        
            var okResult = result.Result as ObjectResult;
            var okList = okResult.Value as IEnumerable<Transaction>;
        
            Assert.Multiple(() =>
            {
                Assert.That(okList.Count(), Is.EqualTo(expectedResultsCount));
                Assert.That(okResult.StatusCode, Is.EqualTo(expectedCode));
            });
        }

        [Order(2)]
        [TestCase(204, 0, 0)]
        [TestCase(204, 4, 4)]
        [TestCase(204, 3, 0, "02.12.2022 16:25:34")]
        [TestCase(204, 5, 0, "", "d1630000-5d0f-0015-2872-08da3058ad5a")]
        public void GetTransactionsFiltered_NoContent(int expectedCode,
            int limit = 20, int offset = 0,
            string timeStamp = "", string senderWallet = "",
            string recipientWallet = "")
        {
            var query = new TransactionQuery()
            {
                TimeStamp = timeStamp,
                SenderWallet = senderWallet,
                RecipientWallet = recipientWallet
            };

            var result = TestController.GetAllFiltered(query, limit, offset);

            var okResult = result.Result as StatusCodeResult;

            Assert.That(okResult.StatusCode, Is.EqualTo(expectedCode));
        }

        [Order(4)]
        [Test]
        public void GetTransactionsByGUID_Positive()
        {
            int expectedCode = 200;
            string transactionGUID = "71ed9908-2722-4a1b-9741-8d2ffce8401b";
            
            var expectedTransaction = new Transaction()
            {
                TimeStamp = "05.11.2022 17:22:42",
                SenderWallet = "d2630000-5d0f-0015-2872-08da3058ad5a",
                RecipientWallet = "d4630000-5d0f-0015-2872-08da3058ad5a",
                Amount = 1.0d,
                CurrencyType = "Bitcoin",
                TransactionType = "Transaction"
            };
            
            var result = TestController.GetTransactionByGUID(transactionGUID);
            
            var statusResult = result.Result as ObjectResult;
            var transaction = statusResult.Value as Transaction;
            
            Assert.Multiple(() =>
            {
                Assert.That(statusResult.StatusCode, Is.EqualTo(expectedCode));
                Assert.That(transaction.TimeStamp, Is.EqualTo(expectedTransaction.TimeStamp));
                Assert.That(transaction.SenderWallet, Is.EqualTo(expectedTransaction.SenderWallet));
                Assert.That(transaction.RecipientWallet, Is.EqualTo(expectedTransaction.RecipientWallet));
                Assert.That(transaction.Amount, Is.EqualTo(expectedTransaction.Amount));
                Assert.That(transaction.CurrencyType, Is.EqualTo(expectedTransaction.CurrencyType));
                Assert.That(transaction.TransactionType, Is.EqualTo(expectedTransaction.TransactionType));
            });
        }
        
        [Order(5)]
        [TestCase(404, "NOT-GUID-VALUE-ID")]
        [TestCase(404, "aaaaaaaa-4b2d-4643-bb08-039503d7d789")]
        public void GetTransactionsByGUID_Negative(int expectedCode,
            string transactionGUID)
        {
            var result = TestController.GetTransactionByGUID(transactionGUID);
        
            var statusResult = result.Result as ObjectResult;
        
            Assert.That(statusResult.StatusCode, Is.EqualTo(expectedCode));
        }
        #endregion
        
        #region POST_Tests
        [Order(6)]
        [TestCase(201, "d1630000-5d0f-0015-2872-08da3058ad5a",
            "d2630000-5d0f-0015-2872-08da3058ad5a", 4.0d, "Bitcoin", "Transaction")]
        public void AddNewTransaction_Positive(int expectedCode,
            string senderWallet, string recipientWallet, double amount,
            string currencyType, string transactionType)
        {
            var transaction = new Transaction()
            {
                TimeStamp = DateTime.Now.ToString(),
                SenderWallet = senderWallet,
                RecipientWallet = recipientWallet,
                Amount = amount,
                CurrencyType = currencyType,
                TransactionType = transactionType
            };

            double previousSenderBalance, previousRecipientBalance;

            using var dbContext = new CryptoTransactionsContext();
            {
                previousSenderBalance = dbContext.Clients.FirstOrDefault(c =>
                c.WalletNumber == senderWallet).Balance;

                previousRecipientBalance = dbContext.Clients.FirstOrDefault(c =>
                    c.WalletNumber == recipientWallet).Balance;
            }

            var result = TestController.AddNew(transaction);

            using var updatedDBContext = new CryptoTransactionsContext();

            var statusResult = result.Result as ObjectResult;

            Assert.Multiple(() =>
            {
                Assert.That(statusResult.StatusCode, Is.EqualTo(expectedCode));
                Assert.That(updatedDBContext.Transactions.Contains(transaction), Is.True);

                Assert.That(updatedDBContext.Clients.FirstOrDefault(c =>
                c.WalletNumber == senderWallet).Balance, Is.EqualTo(previousSenderBalance - amount));

                Assert.That(updatedDBContext.Clients.FirstOrDefault(c =>
                c.WalletNumber == recipientWallet).Balance, Is.EqualTo(previousRecipientBalance + amount));
            });
        }
        
        [Order(7)]
        [TestCase(400, "aaaaaaaa-5d0f-0015-2872-08da3058ad5a",
            "aaaaaaaa-5d0f-0015-2872-08da3058ad5a", 4.0d, "Bitcoin", "Transaction")]
        [TestCase(400, "d1630000-5d0f-0015-2872-08da3058ad5a",
            "d2630000-5d0f-0015-2872-08da3058ad5a", -4.0d, "Bitcoin", "Transaction")]
        [TestCase(400, "d1630000-5d0f-0015-2872-08da3058ad5a",
            "d2630000-5d0f-0015-2872-08da3058ad5a", 0d, "Bitcoin", "Transaction")]
        [TestCase(400, "d0630000-5d0f-0015-2872-08da3058ad5a",
            "d1630000-5d0f-0015-2872-08da3058ad5a", 500.0d, "Bitcoin", "Transaction")]
        [TestCase(404, "aaaaaaaa-5d0f-0015-2872-08da3058ad5a",
            "d2630000-5d0f-0015-2872-08da3058ad5a", 5.0d, "Bitcoin", "Transaction")]
        [TestCase(404, "d2630000-5d0f-0015-2872-08da3058ad5a",
            "aaaaaaaa-5d0f-0015-2872-08da3058ad5a", 5.0d, "Bitcoin", "Transaction")]
        [TestCase(409, "d0630000-5d0f-0015-2872-08da3058ad5a",
            "d1630000-5d0f-0015-2872-08da3058ad5a", 5.0d, "Bitcoin", "Transaction",
            "02.12.2022 16:25:35")]
        public void AddNewClient_Negative(int expectedCode,
            string senderWallet, string recipientWallet, double amount,
            string currencyType, string transactionType, string? timestamp = null)
        {
            var transaction = new Transaction()
            {
                TimeStamp = timestamp ?? DateTime.Now.ToString(),
                SenderWallet = senderWallet,
                RecipientWallet = recipientWallet,
                Amount = amount,
                CurrencyType = currencyType,
                TransactionType = transactionType
            };

            var result = TestController.AddNew(transaction);
            
            var statusResult = result.Result as ObjectResult;
            
            using var dbContext = new CryptoTransactionsContext();
            
            Assert.Multiple(() =>
            {
                Assert.That(statusResult.StatusCode, Is.EqualTo(expectedCode));
                Assert.That(dbContext.Transactions.Contains(transaction), Is.False);
            });
        }
        #endregion
        
        #region DELETE_Tests
        [Order(8)]
        [TestCase(202, "71ed9908-2722-4a1b-9741-8d2ffce8401b")]
        public void DeleteTransaction_Positive(int expectedCode, string transactionGUID)
        {
            var result = TestController.Delete(transactionGUID);
            
            var statusResult = result.Result as ObjectResult;
            
            using var dbContext = new CryptoTransactionsContext();
            
            Assert.Multiple(() =>
            {
                Assert.That(statusResult.StatusCode, Is.EqualTo(expectedCode));
                Assert.That(dbContext.Transactions.Any(t => t.GUID == transactionGUID), Is.False);
            });
        }
        
        [Order(9)]
        [TestCase(404, "NOT-GUID-VALUE")]
        [TestCase(404, "aaaaaaaa-5d0f-0015-2872-08da3058ad5a")]
        public void DeleteTransaction_Negative(int expectedCode, string transactionGUID)
        {
            var result = TestController.Delete(transactionGUID);
        
            var statusResult = result.Result as ObjectResult;
        
            Assert.That(statusResult.StatusCode, Is.EqualTo(expectedCode));
        }
        #endregion
    }
}
