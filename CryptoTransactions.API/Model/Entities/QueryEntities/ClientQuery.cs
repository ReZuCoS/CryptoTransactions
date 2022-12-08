using System.ComponentModel.DataAnnotations;

namespace CryptoTransactions.API.Model.Entities.QueryEntities
{
    /// <summary>
    /// Client query data
    /// </summary>
    public class ClientQuery
    {
        /// <summary>
        /// Client's surname
        /// </summary>
        [DataType(DataType.Text)]
        public string Surname { get; set; } = string.Empty;

        /// <summary>
        /// Client's name
        /// </summary>
        [DataType(DataType.Text)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Client's patronymic (if available)
        /// </summary>
        [DataType(DataType.Text)]
        public string Patronymic { get; set; } = string.Empty;

        internal bool IsEmpty()
        {
            return string.IsNullOrEmpty(Surname) &&
                string.IsNullOrEmpty(Name) &&
                string.IsNullOrEmpty(Patronymic);
        }
    }
}
