using Moq;
using Route256.Week5.Workshop.PriceCalculator.Bll.Services;
using Route256.Week5.Workshop.PriceCalculator.Dal.Repositories.Interfaces;

namespace Route256.Week5.Workshop.PriceCalculator.UnitTests.Stubs;

public class CalculationServiceStub : CalculationService
{
    public Mock<ICalculationRepository> CalculationRepository { get; }
    public Mock<IGoodsRepository> GoodsRepository { get; }
    public Mock<IAnomalyPriceRepository> AnomalyPriceRepository { get; }

    public CalculationServiceStub(
        Mock<ICalculationRepository> calculationRepository,
        Mock<IGoodsRepository> goodsRepository,
        Mock<IAnomalyPriceRepository> anomalyPriceRepository) 
        : base(
            calculationRepository.Object, 
            goodsRepository.Object,
            anomalyPriceRepository.Object)
    {
        CalculationRepository = calculationRepository;
        GoodsRepository = goodsRepository;
        AnomalyPriceRepository = anomalyPriceRepository;
    }
    
    public void VerifyNoOtherCalls()
    {
        CalculationRepository.VerifyNoOtherCalls();
        GoodsRepository.VerifyNoOtherCalls();
        AnomalyPriceRepository.VerifyNoOtherCalls();
    }
}