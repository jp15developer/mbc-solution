namespace Ambev.DeveloperEvaluation.Domain.Events;

public class ItemCancelledEvent
{
    public Guid SaleId { get; }
    public Guid ProductId { get; }

    public ItemCancelledEvent(Guid saleId, Guid productId)
    {
        SaleId = saleId;
        ProductId = productId;
    }
}
