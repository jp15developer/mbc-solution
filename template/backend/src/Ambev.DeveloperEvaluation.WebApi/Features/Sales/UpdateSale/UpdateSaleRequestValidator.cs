using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

public class UpdateSaleRequestValidator : AbstractValidator<UpdateSaleRequest>
{
    public UpdateSaleRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.SaleNumber)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.CustomerId)
            .NotEmpty();

        RuleFor(x => x.CustomerName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.BranchId)
            .NotEmpty();

        RuleFor(x => x.BranchName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Items)
            .NotNull()
            .NotEmpty()
            .Must(items => items
                .Select(i => i.ProductId)
                .Distinct()
                .Count() == items.Count)
            .WithMessage("Duplicated products are not allowed. Consolidate quantities by product.");

        RuleFor(x => x.CancelledItemProductIds)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Cancelled item product IDs must be unique.")
            .Must((request, ids) => ids.All(id => request.Items.Any(item => item.ProductId == id)))
            .WithMessage("Cancelled item product IDs must exist in the sale items list.");

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId).NotEmpty();
                item.RuleFor(i => i.ProductName).NotEmpty().MaximumLength(100);
                item.RuleFor(i => i.Quantity).InclusiveBetween(1, 20);
                item.RuleFor(i => i.UnitPrice).GreaterThan(0);
            });
    }
}
