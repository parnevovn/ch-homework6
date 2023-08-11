using Route256.Week6.Homework.PriceCalculator.BackgroundServices.ProcessingServices.Interfaces;

namespace Route256.Week6.Homework.PriceCalculator.BackgroundServices
{
    public class GoodPriceCalcConsumeHostedService : BackgroundService
    {
        private readonly ILogger<GoodPriceCalcConsumeHostedService> _logger;

        public GoodPriceCalcConsumeHostedService(
            IServiceProvider services,
            ILogger<GoodPriceCalcConsumeHostedService> logger)
        {
            Services = services;
            _logger = logger;
        }

        public IServiceProvider Services { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "GoodPriceCalcConsumeHostedService running.");

            await DoWork(stoppingToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "GoodPriceCalcConsumeHostedService is working.");

            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService =
                    scope.ServiceProvider
                        .GetRequiredService<IGoodPriceCalcProcessingService>();

                await scopedProcessingService.DoWork(stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "GoodPriceCalcProcessingService is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}
