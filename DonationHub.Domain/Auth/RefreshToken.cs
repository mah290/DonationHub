using DonationHub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Domain.Auth
{
    public sealed class RefreshToken : Entity
    {
        public Guid UserId { get; private set; }
        public string TokenHash { get; private set; } = default!;
        public DateTime CreatedAt { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public DateTime? RevokedAt { get; private set; }
        public Guid? ReplacedByTokenId { get; private set; }

        private RefreshToken() { }

        public static RefreshToken Create(Guid userId, string tokenHash, DateTime now, DateTime expiresAt)
        {
            if (string.IsNullOrWhiteSpace(tokenHash))
                throw new DomainException("token.hash_required", "Token hash is required.");
            if (expiresAt <= now)
                throw new DomainException("token.invalid_expiry", "Expiry must be in the future.");

            return new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = tokenHash,
                CreatedAt = now,
                ExpiresAt = expiresAt
            };
        }

        public bool IsRevoked => RevokedAt.HasValue;
        public bool IsExpired(DateTime now) => now >= ExpiresAt;
        public bool IsActive(DateTime now) => !IsRevoked && !IsExpired(now);

        public void Revoke(DateTime now, Guid? replacedByTokenId = null)
        {
            if (IsRevoked)
                throw new DomainException("token.already_revoked", "Token is already revoked.");

            RevokedAt = now;
            ReplacedByTokenId = replacedByTokenId;
        }
    }
}

