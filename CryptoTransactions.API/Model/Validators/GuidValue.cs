using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace CryptoTransactions.API.Model.Validators
{
    public class GuidValue : ValidationAttribute
    {
        private const string DefaultErrorMessage = "{0} does not contain a valid guid value";

        public GuidValue() : base(DefaultErrorMessage) { }

        protected override ValidationResult? IsValid(object value, ValidationContext validationContext)
        {
            var input = Convert.ToString(value, CultureInfo.CurrentCulture);

            if (string.IsNullOrWhiteSpace(input))
                return null;

            if (!Guid.TryParse(input, out _))
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));

            return ValidationResult.Success;
        }
    }
}
