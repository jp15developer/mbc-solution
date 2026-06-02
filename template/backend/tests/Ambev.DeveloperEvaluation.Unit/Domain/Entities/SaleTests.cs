using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleTests
{
    [Fact(DisplayName = "Given quantity below 4 When recalculating Then applies no discount")]
    public void RecalculateTotals_QuantityBelow4_AppliesNoDiscount()
    {
        var sale = SaleTestData.GenerateValidSale(quantity: 3, unitPrice: 100m);

        sale.TotalAmount.Should().Be(300m);
        sale.Items[0].DiscountPercentage.Should().Be(0m);
    }

    [Fact(DisplayName = "Given quantity between 4 and 9 When recalculating Then applies 10 percent discount")]
    public void RecalculateTotals_QuantityBetween4And9_AppliesTenPercentDiscount()
    {
        var sale = SaleTestData.GenerateValidSale(quantity: 4, unitPrice: 100m);

        sale.TotalAmount.Should().Be(360m);
        sale.Items[0].DiscountPercentage.Should().Be(0.10m);
    }

    [Fact(DisplayName = "Given quantity between 10 and 20 When recalculating Then applies 20 percent discount")]
    public void RecalculateTotals_QuantityBetween10And20_AppliesTwentyPercentDiscount()
    {
        var sale = SaleTestData.GenerateValidSale(quantity: 10, unitPrice: 100m);

        sale.TotalAmount.Should().Be(800m);
        sale.Items[0].DiscountPercentage.Should().Be(0.20m);
    }

    [Fact(DisplayName = "Given quantity above 20 When recalculating Then throws domain exception")]
    public void RecalculateTotals_QuantityAbove20_ThrowsDomainException()
    {
        var act = () => SaleTestData.GenerateValidSale(quantity: 21, unitPrice: 100m);

        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Given cancelled item When cancelling item Then total excludes cancelled item")]
    public void CancelItem_ExistingItem_TotalExcludesItem()
    {
        var sale = SaleTestData.GenerateValidSale(quantity: 4, unitPrice: 100m);
        var productId = sale.Items[0].ProductId;

        sale.CancelItem(productId);

        sale.TotalAmount.Should().Be(0m);
        sale.Items[0].IsCancelled.Should().BeTrue();
    }

    [Fact(DisplayName = "Given sale When cancelling sale Then sale is cancelled and total is zero")]
    public void CancelSale_ValidSale_MarksSaleAsCancelled()
    {
        var sale = SaleTestData.GenerateValidSale(quantity: 5, unitPrice: 100m);

        sale.CancelSale();

        sale.IsCancelled.Should().BeTrue();
        sale.TotalAmount.Should().Be(0m);
    }
}
