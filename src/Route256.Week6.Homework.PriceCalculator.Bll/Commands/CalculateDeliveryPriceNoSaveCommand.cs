using MediatR;
using Route256.Week5.Workshop.PriceCalculator.Bll.Extensions;
using Route256.Week5.Workshop.PriceCalculator.Bll.Models;
using Route256.Week5.Workshop.PriceCalculator.Bll.Services.Interfaces;

namespace Route256.Week5.Workshop.PriceCalculator.Bll.Commands;

public record CalculateDeliveryPriceNoSaveCommand(
        GoodModel[] Goods)
    : IRequest<decimal>;

public class CalculateDeliveryPriceNoSaveCommandHandler
    : IRequestHandler<CalculateDeliveryPriceNoSaveCommand, decimal>
{
    private readonly ICalculationService _calculationService;

    public CalculateDeliveryPriceNoSaveCommandHandler(
        ICalculationService calculationService)
    {
        _calculationService = calculationService;
    }
    
    public Task<decimal> Handle(
        CalculateDeliveryPriceNoSaveCommand request, 
        CancellationToken cancellationToken)
    {
        request.EnsureHasGoods();

        var volumePrice = _calculationService.CalculatePriceByVolume(request.Goods, out var volume);
        var weightPrice = _calculationService.CalculatePriceByWeight(request.Goods, out var weight);
        var resultPrice = Math.Max(volumePrice, weightPrice);

        return Task.FromResult<decimal>(resultPrice);
    }
}