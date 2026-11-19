using DonationHub.Application.Common.Results;

namespace DonationHub.Application.Features.Idempotency;

public static class IdempotencyErrors
{
    public static Error KeyReusedWithDifferentRequest() =>
        new("Idempotency.KeyReusedWithDifferentRequest",
            "The Idempotency-Key was already used with a different request.",
            ErrorType.UnprocessableEntity);
}