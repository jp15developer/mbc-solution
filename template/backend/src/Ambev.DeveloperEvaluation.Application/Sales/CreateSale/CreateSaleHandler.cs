using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, CreateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateSaleHandler> _logger;

    public CreateSaleHandler(ISaleRepository saleRepository, IMapper mapper, ILogger<CreateSaleHandler> logger)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CreateSaleResult> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = _mapper.Map<Sale>(command);
        var items = _mapper.Map<List<SaleItem>>(command.Items);
        sale.SetItems(items);

        var created = await _saleRepository.CreateAsync(sale, cancellationToken);

        var saleCreatedEvent = new SaleCreatedEvent(created);
        _logger.LogInformation(
            "{EventName}: SaleId={SaleId}, SaleNumber={SaleNumber}",
            nameof(SaleCreatedEvent),
            saleCreatedEvent.Sale.Id,
            saleCreatedEvent.Sale.SaleNumber);

        return _mapper.Map<CreateSaleResult>(created);
    }
}
