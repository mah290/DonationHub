using FluentValidation;

namespace DonationHub.Application.Common.Validation;

public static class PasswordRules
{
    // نفس سياسة Identity: 8+ حروف، كبير، صغير، رقم
    public static IRuleBuilderOptions<T, string> MeetsPasswordPolicy<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches(@"\p{Lu}").WithMessage("Password must contain an uppercase letter.")
            .Matches(@"\p{Ll}").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.");
}