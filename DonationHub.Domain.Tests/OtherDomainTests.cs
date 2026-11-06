using DonationHub.Domain.Categories;
using DonationHub.Domain.Common;
using DonationHub.Domain.ValueObjects;
using static DonationHub.Domain.Tests.TestData;

namespace DonationHub.Domain.Tests;

public class OtherDomainTests
{
    // Invariant 10
    [Fact]
    public void Category_WithCampaigns_CannotBeDeleted()
    {
        var category = Category.Create("صحة", "Health", "", "", Now);

        var ex = Assert.Throws<DomainException>(() => category.EnsureCanBeDeleted(hasCampaigns: true));
        Assert.Equal("category.has_campaigns", ex.Code);
    }

    [Fact]
    public void Category_WithoutCampaigns_CanBeDeleted()
    {
        var category = Category.Create("صحة", "Health", "", "", Now);
        category.EnsureCanBeDeleted(hasCampaigns: false);   // مفيش exception
    }

    [Fact]
    public void Category_NameLongerThan100_Throws()
    {
        Assert.Throws<DomainException>(() =>
            Category.Create(new string('ا', 101), "Health", "", "", Now));
    }

    [Fact]
    public void Money_WithSameAmountAndCurrency_AreEqual()
    {
        Assert.Equal(Money.Create(10m, "usd"), Money.Create(10m, "USD"));
    }

    [Fact]
    public void Money_ToMinorUnits_ConvertsToCents()
    {
        Assert.Equal(1050, Money.Create(10.50m, "USD").ToMinorUnits());
    }

    [Fact]
    public void Money_AddDifferentCurrencies_Throws()
    {
        Assert.Throws<DomainException>(() =>
            Money.Create(1m, "USD").Add(Money.Create(1m, "EGP")));
    }
}
