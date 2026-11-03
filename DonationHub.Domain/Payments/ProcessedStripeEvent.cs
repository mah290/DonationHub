using DonationHub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Domain.Payments
{
    public sealed class ProcessedStripeEvent
    {
        public string EventId { get; private set; } = default!;   // Key
        public string Type { get; private set; } = default!;
        public DateTime ProcessedAt { get; private set; }

        private ProcessedStripeEvent() { }

        public static ProcessedStripeEvent Create(string eventId, string type, DateTime now)
        {
            if (string.IsNullOrWhiteSpace(eventId) || string.IsNullOrWhiteSpace(type))
                throw new DomainException("stripe_event.invalid", "Event id and type are required.");

            return new ProcessedStripeEvent { EventId = eventId, Type = type, ProcessedAt = now };
        }
    }
}
