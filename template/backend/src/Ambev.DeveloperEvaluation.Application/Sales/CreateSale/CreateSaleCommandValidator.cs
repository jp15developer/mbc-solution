using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleCommandValidator()
    {
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
