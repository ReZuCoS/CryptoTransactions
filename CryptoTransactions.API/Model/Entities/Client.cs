using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CryptoTransactions.API.Model.Entities
{
    [PrimaryKey(nameof(WalletNumber))]
    public class Client
    {
        [Required]
        [MinLength(36)]
        [MaxLength(36)]
        public string WalletNumber { get; set; } = default!;

        [Required]
        [MaxLength(50)]
        public string Surname { get; set; } = default!;

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = default!;

        [MaxLength(50)]
        public string? Patronymic { get; set; }

        [DefaultValue(0.0d)]
        public double Balance { get; set; }

        [JsonIgnore]
        public virtual ICollection<Transaction>? SentTransactions { get; set; }

        [JsonIgnore]
        public virtual ICollection<Transaction>? ReceivedTransactions { get; set; }
    }
}
