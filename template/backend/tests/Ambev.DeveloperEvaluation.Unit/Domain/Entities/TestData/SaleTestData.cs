using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

public static class SaleTestData
{
    public static Sale GenerateValidSale(int quantity = 1, decimal unitPrice = 100)
    {
        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = $"SALE-{Guid.NewGuid():N}"[..18],
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "John Doe",
            BranchId = Guid.NewGuid(),
            BranchName = "Main Branch"
        };

        sale.SetItems([
            new SaleItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Product A",
                Quantity = quantity,
                UnitPrice = unitPrice
            }
        ]);

        return sale;
    }
}
