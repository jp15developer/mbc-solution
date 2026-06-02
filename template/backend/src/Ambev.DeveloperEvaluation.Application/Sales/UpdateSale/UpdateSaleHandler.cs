using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateSaleHandler> _logger;

    public UpdateSaleHandler(ISaleRepository saleRepository, IMapper mapper, ILogger<UpdateSaleHandler> logger)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UpdateSaleResult> Handle(UpdateSaleCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (sale == null)
            throw new KeyNotFoundException($"Sale with ID {request.Id} not found");

        sale.SaleNumber = request.SaleNumber;
        sale.SaleDate = request.SaleDate;
        sale.CustomerId = request.CustomerId;
        sale.CustomerName = request.CustomerName;
        sale.BranchId = request.BranchId;
        sale.BranchName = request.BranchName;
        sale.UpdatedAt = DateTime.UtcNow;

        var mappedItems = _mapper.Map<List<SaleItem>>(request.Items);
        sale.SetItems(mappedItems);

        foreach (var cancelledItemProductId in request.CancelledItemProductIds)
        {
            sale.CancelItem(cancelledItemProductId);
            _logger.LogInformation(
                "ItemCancelled: SaleId={SaleId}, ProductId={ProductId}",
                sale.Id,
                cancelledItemProductId);
        }

        if (request.IsCancelled && !sale.IsCancelled)
        {
            sale.CancelSale();
            _logger.LogInformation("SaleCancelled: SaleId={SaleId}, SaleNumber={SaleNumber}", sale.Id, sale.SaleNumber);
        }
        else
        {
            sale.RecalculateTotals();
        }

        var updatedSale = await _saleRepository.UpdateAsync(sale, cancellationToken);

        _logger.LogInformation("SaleModified: SaleId={SaleId}, SaleNumber={SaleNumber}", updatedSale.Id, updatedSale.SaleNumber);

        return _mapper.Map<UpdateSaleResult>(updatedSale);
    }
}
