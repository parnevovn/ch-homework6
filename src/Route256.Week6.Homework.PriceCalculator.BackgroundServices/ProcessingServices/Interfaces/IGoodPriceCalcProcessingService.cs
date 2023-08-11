namespace Route256.Week6.Homework.PriceCalculator.BackgroundServices.ProcessingServices.Interfaces
{
    internal interface IGoodPriceCalcProcessingService
    {
        Task DoWork(CancellationToken stoppingToken);
    }
}
