using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class UpdateSaleValidatorTests
{
    [Fact(DisplayName = "Given duplicated products When validating update sale Then should be invalid")]
    public void Validate_DuplicatedProducts_ShouldBeInvalid()
    {
        var productId = Guid.NewGuid();

        var command = new UpdateSaleCommand
        {
            Id = Guid.NewGuid(),
            SaleNumber = "SALE-001",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch",
            Items =
            [
                new UpdateSaleItemCommand
                {
                    ProductId = productId,
                    ProductName = "Product",
                    Quantity = 2,
                    UnitPrice = 100m
                },
                new UpdateSaleItemCommand
                {
                    ProductId = productId,
                    ProductName = "Product Duplicate",
                    Quantity = 1,
                    UnitPrice = 100m
                }
            ]
        };

        var validator = new UpdateSaleValidator();
        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Duplicated products are not allowed"));
    }

    [Fact(DisplayName = "Given cancelled item id absent from items When validating update sale Then should be invalid")]
    public void Validate_CancelledItemNotInItems_ShouldBeInvalid()
    {
        var command = new UpdateSaleCommand
        {
            Id = Guid.NewGuid(),
            SaleNumber = "SALE-001",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch",
            CancelledItemProductIds = [Guid.NewGuid()],
            Items =
            [
                new UpdateSaleItemCommand
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product",
                    Quantity = 2,
                    UnitPrice = 100m
                }
            ]
        };

        var validator = new UpdateSaleValidator();
        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("must exist in the sale items list"));
    }
}
