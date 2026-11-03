using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DonationHub.Domain.Donations
{
    public sealed class DonationOptions : IValidatableObject
    {
        public const string SectionName = "Donation";

        [Range(typeof(decimal), "0.01", "1000000")]
        public decimal MinAmount { get; set; } = 1.00m;

        [Range(typeof(decimal), "0.01", "1000000")]
        public decimal MaxAmount { get; set; } = 10_000.00m;

        [Range(1, 1440)]
        public int PendingTimeoutMinutes { get; set; } = 30;

        [Required, RegularExpression("^[A-Za-z]{3}$", ErrorMessage = "DefaultCurrency must be a 3-letter ISO code.")]
        public string DefaultCurrency { get; set; } = "USD";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MaxAmount < MinAmount)
                yield return new ValidationResult(
                    "MaxAmount must be greater than or equal to MinAmount.",
                    [nameof(MaxAmount)]);
        }
    }
}
