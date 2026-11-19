using DonationHub.Application.Common.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Application.Features.Payments
{
    public static class PaymentErrors
    {
        public static Error InvalidSignature() =>
            new("Payment.InvalidSignature", "Webhook signature is invalid.", ErrorType.Validation);
    }
}
