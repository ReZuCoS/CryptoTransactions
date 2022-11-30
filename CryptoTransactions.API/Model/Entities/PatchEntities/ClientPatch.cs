namespace CryptoTransactions.API.Model.Entities
{
    /// <summary>
    /// Client patch data
    /// </summary>
    public class ClientPatch
    {
        /// <summary>
        /// Client's surname
        /// </summary>
        public string? Surname { get; set; }

        /// <summary>
        /// Client's name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Client's patronymic (if available)
        /// </summary>
        public string? Patronymic { get; set; }
    }
}
