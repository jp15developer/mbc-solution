using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Validation;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Sale : BaseEntity
{
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public bool IsCancelled { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<SaleItem> Items { get; set; } = [];

    public Sale()
    {
        SaleDate = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetItems(IEnumerable<SaleItem> items)
    {
        Items = items.ToList();
        RecalculateTotals();
    }

    public void CancelSale()
    {
        IsCancelled = true;
        UpdatedAt = DateTime.UtcNow;
        TotalAmount = 0;
    }

    public void CancelItem(Guid productId)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new KeyNotFoundException($"Product {productId} not found in sale {Id}");

        item.Cancel();
        UpdatedAt = DateTime.UtcNow;
        RecalculateTotals();
    }

    public void RecalculateTotals()
    {
        if (IsCancelled)
        {
            TotalAmount = 0;
            return;
        }

        foreach (var item in Items)
        {
            item.Recalculate();
        }

        TotalAmount = Items
            .Where(i => !i.IsCancelled)
            .Sum(i => i.TotalAmount);
    }

    public ValidationResultDetail Validate()
    {
        var validator = new SaleValidator();
        var result = validator.Validate(this);

        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(error => (ValidationErrorDetail)error)
        };
    }
}

public class SaleItem
{
    private const int FirstDiscountTierQuantity = 4;
    private const int SecondDiscountTierQuantity = 10;
    private const int MaximumQuantity = 20;
    private const decimal FirstDiscountTierPercentage = 0.10m;
    private const decimal SecondDiscountTierPercentage = 0.20m;

    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercentage { get; private set; }
    public decimal TotalAmount { get; private set; }
    public bool IsCancelled { get; private set; }

    public void Cancel()
    {
        IsCancelled = true;
        DiscountPercentage = 0;
        TotalAmount = 0;
    }

    public void Recalculate()
    {
        if (IsCancelled)
        {
            DiscountPercentage = 0;
            TotalAmount = 0;
            return;
        }

        if (Quantity > MaximumQuantity)
            throw new DomainException("It's not possible to sell above 20 identical items.");

        DiscountPercentage = CalculateDiscountPercentage();

        var subtotal = UnitPrice * Quantity;
        TotalAmount = subtotal - (subtotal * DiscountPercentage);
    }

    private decimal CalculateDiscountPercentage()
    {
        if (Quantity >= SecondDiscountTierQuantity)
            return SecondDiscountTierPercentage;

        if (Quantity >= FirstDiscountTierQuantity)
            return FirstDiscountTierPercentage;

        return 0m;
    }
}
