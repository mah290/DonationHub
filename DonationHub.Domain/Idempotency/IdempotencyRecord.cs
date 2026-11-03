using DonationHub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Domain.Idempotency
{
    public sealed class IdempotencyRecord
    {
        public string Key { get; private set; } = default!;
        public Guid UserId { get; private set; }
        public string RequestHash { get; private set; } = default!;
        public int ResponseStatusCode { get; private set; }
        public string ResponseBody { get; private set; } = default!;
        public DateTime CreatedAt { get; private set; }
        public DateTime ExpiresAt { get; private set; }

        private IdempotencyRecord() { }
        public static IdempotencyRecord Create(
        string key, Guid userId, string requestHash,
        int responseStatusCode, string responseBody, DateTime now, DateTime expiresAt)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new DomainException("idempotency.key_required", "Idempotency key is required.");
            if (expiresAt <= now)
                throw new DomainException("idempotency.invalid_expiry", "Expiry must be in the future.");

            return new IdempotencyRecord
            {
                Key = key,
                UserId = userId,
                RequestHash = requestHash,
                ResponseStatusCode = responseStatusCode,
                ResponseBody = responseBody,
                CreatedAt = now,
                ExpiresAt = expiresAt
            };
        }

        public bool IsExpired(DateTime now) => now >= ExpiresAt;
        public bool MatchesRequest(string requestHash) => RequestHash == requestHash;
    }
}
