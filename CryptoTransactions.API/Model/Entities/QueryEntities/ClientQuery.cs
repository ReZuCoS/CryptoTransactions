namespace CryptoTransactions.API.Model.Entities
{
    /// <summary>
    /// Client patch data
    /// </summary>
    public class ClientQuery
    {
        /// <summary>
        /// Client's surname
        /// </summary>
        public string Surname { get; set; } = string.Empty;

        /// <summary>
        /// Client's name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Client's patronymic (if available)
        /// </summary>
        public string Patronymic { get; set; } = string.Empty;
    }
}
