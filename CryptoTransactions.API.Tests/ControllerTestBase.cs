using CryptoTransactions.API.Model;
using NUnit.Framework;

namespace CryptoTransactions.API.Tests
{
    public class ControllerTestBase
    {
        [SetUp]
        public void Setup()
        {
            CryptoTransactionsContext.ConnectionString = $"Data source = " +
            $"{Environment.CurrentDirectory}\\SampleData\\CryptoTransactionsDEBUG.db";
        }
    }
}
