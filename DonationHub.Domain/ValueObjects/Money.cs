using DonationHub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Domain.ValueObjects
{
    public sealed class Money : ValueObject
    {
        public decimal Amount { get; }
        public string Currency { get; }

        private Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        public static Money Create(decimal amount, string currency)
        {
            if (amount < 0)
                throw new DomainException("money.negative", "Amount cannot be negative.");

            if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
                throw new DomainException("money.invalid_currency", "Currency must be a 3-letter ISO code.");

            return new Money(
                Math.Round(amount, 2, MidpointRounding.AwayFromZero),
                currency.Trim().ToUpperInvariant());
        }
        public Money Add(Money other)
        {
            if (Currency != other.Currency)
                throw new DomainException("money.currency_mismatch", "Cannot add different currencies.");

            return new Money(Amount + other.Amount, Currency);
        }
        public long ToMinorUnits() => (long)(Amount * 100m);

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }
}
