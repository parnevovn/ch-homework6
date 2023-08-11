using Moq;
using Route256.Week5.Workshop.PriceCalculator.Dal.Repositories;
using Route256.Week5.Workshop.PriceCalculator.Dal.Repositories.Interfaces;
using Route256.Week5.Workshop.PriceCalculator.UnitTests.Stubs;

namespace Route256.Week5.Workshop.PriceCalculator.UnitTests.Builders;

public class CalculationServiceBuilder
{
    public Mock<ICalculationRepository> CalculationRepository;
    public Mock<IGoodsRepository> GoodsRepository;
    public Mock<IAnomalyPriceRepository> AnomalyPriceRepository;

    public CalculationServiceBuilder()
    {
        CalculationRepository = new Mock<ICalculationRepository>();
        GoodsRepository = new Mock<IGoodsRepository>();
        AnomalyPriceRepository = new Mock<IAnomalyPriceRepository>();
    }
    
    public CalculationServiceStub Build()
    {
        return new CalculationServiceStub(
            CalculationRepository,
            GoodsRepository,
            AnomalyPriceRepository);
    }
}