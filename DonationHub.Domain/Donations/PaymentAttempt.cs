using DonationHub.Domain.Common;
using DonationHub.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Domain.Donations
{
    public sealed class PaymentAttempt : AuditableEntity
    {
        public Guid DonationId { get; private set; }
        public string StripeCheckoutSessionId { get; private set; } = default!;
        public string? StripePaymentIntentId { get; private set; }
        public decimal Amount { get; private set; }
        public PaymentAttemptStatus Status { get; private set; }
        public string? FailureCode { get; private set; }

        private PaymentAttempt() { }
        public static PaymentAttempt Create(
       Guid donationId, string stripeCheckoutSessionId, decimal amount, DateTime now)
        {
            if (string.IsNullOrWhiteSpace(stripeCheckoutSessionId))
                throw new DomainException("payment.session_required", "Stripe session id is required.");
            if (amount <= 0)
                throw new DomainException("payment.invalid_amount", "Amount must be greater than zero.");

            var attempt = new PaymentAttempt
            {
                Id = Guid.NewGuid(),
                DonationId = donationId,
                StripeCheckoutSessionId = stripeCheckoutSessionId,
                Amount = amount,
                Status = PaymentAttemptStatus.Created
            };

            attempt.MarkCreated(now);   
            return attempt;
        }

        public void MarkSucceeded(string? paymentIntentId, DateTime now)
        {
            EnsureCreated();
            StripePaymentIntentId = paymentIntentId;
            Status = PaymentAttemptStatus.Succeeded;
            Touch(now);
        }

        public void MarkFailed(string? failureCode, DateTime now)
        {
            EnsureCreated();
            FailureCode = failureCode;
            Status = PaymentAttemptStatus.Failed;
            Touch(now);
        }

        public void MarkExpired(DateTime now)
        {
            EnsureCreated();
            Status = PaymentAttemptStatus.Expired;
            Touch(now);
        }

        // Invariant #8
        private void EnsureCreated()
        {
            if (Status != PaymentAttemptStatus.Created)
                throw new DomainException("payment.final_state", "A final payment attempt cannot change.");
        }
    }
}
