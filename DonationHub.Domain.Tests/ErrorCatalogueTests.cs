using DonationHub.Application.Common.Results;
using DonationHub.Application.Features.Auth;
using DonationHub.Application.Features.Campaigns;
using DonationHub.Application.Features.Categories;
using DonationHub.Application.Features.Donations;
using DonationHub.Application.Features.Files;
using DonationHub.Application.Features.Idempotency;
using DonationHub.Application.Features.Payments;

namespace DonationHub.Domain.Tests;

public class ErrorCatalogueTests
{
    public static IEnumerable<object[]> Catalogue =>
    [
        [CommonErrors.ValidationFailed("x"), "Validation.Failed", ErrorType.Validation],
        [AuthErrors.InvalidCredentials(), "Auth.InvalidCredentials", ErrorType.Unauthorized],
        [AuthErrors.EmailAlreadyExists(), "Auth.EmailAlreadyExists", ErrorType.Conflict],
        [AuthErrors.RefreshTokenInvalid(), "Auth.RefreshTokenInvalid", ErrorType.Unauthorized],
        [AuthErrors.AccountDeactivated(), "Auth.AccountDeactivated", ErrorType.Forbidden],
        [CategoryErrors.NotFound(), "Category.NotFound", ErrorType.NotFound],
        [CategoryErrors.HasCampaigns(), "Category.HasCampaigns", ErrorType.Conflict],
        [CampaignErrors.NotFound(), "Campaign.NotFound", ErrorType.NotFound],
        [CampaignErrors.NotActive(), "Campaign.NotActive", ErrorType.Conflict],
        [CampaignErrors.GoalBelowRaised(), "Campaign.GoalBelowRaised", ErrorType.UnprocessableEntity],
        [CampaignErrors.EndDateInPast(), "Campaign.EndDateInPast", ErrorType.UnprocessableEntity],
        [CampaignErrors.ConcurrencyConflict(), "Campaign.ConcurrencyConflict", ErrorType.Conflict],
        [DonationErrors.NotFound(), "Donation.NotFound", ErrorType.NotFound],
        [DonationErrors.AmountOutOfRange(1m, 10000m), "Donation.AmountOutOfRange", ErrorType.UnprocessableEntity],
        [DonationErrors.InvalidState(), "Donation.InvalidState", ErrorType.Conflict],
        [CommonErrors.RateLimited(), "Common.RateLimited", ErrorType.TooManyRequests],
        [FileErrors.InvalidType(), "File.InvalidType", ErrorType.Validation],
        [FileErrors.TooLarge(), "File.TooLarge", ErrorType.PayloadTooLarge],
        [PaymentErrors.InvalidSignature(), "Payment.InvalidSignature", ErrorType.Validation],
        [IdempotencyErrors.KeyReusedWithDifferentRequest(), "Idempotency.KeyReusedWithDifferentRequest", ErrorType.UnprocessableEntity],
    ];

    [Theory]
    [MemberData(nameof(Catalogue))]
    public void Error_HasExpectedCodeAndType(Error error, string code, ErrorType type)
    {
        Assert.Equal(code, error.Code);
        Assert.Equal(type, error.Type);
    }
}