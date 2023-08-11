using MediatR;
using Route256.Week5.Workshop.PriceCalculator.Bll.Extensions;
using Route256.Week5.Workshop.PriceCalculator.Bll.Models;
using Route256.Week5.Workshop.PriceCalculator.Bll.Services.Interfaces;

namespace Route256.Week5.Workshop.PriceCalculator.Bll.Commands;

public record SaveAnomalyPriceCommand(
        SaveAnomalyPriceModel PriceModel)
    : IRequest<bool>;

public class SaveAnomalyPriceCommandHandler
    : IRequestHandler<SaveAnomalyPriceCommand, bool>
{
    private readonly ICalculationService _calculationService;

    public SaveAnomalyPriceCommandHandler(
        ICalculationService calculationService)
    {
        _calculationService = calculationService;
    }
    
    public Task<bool> Handle(
        SaveAnomalyPriceCommand request, 
        CancellationToken cancellationToken)
    {
        _calculationService.SaveAnomalyPrice(request.PriceModel, cancellationToken);

        return Task.FromResult(true);
    }
}