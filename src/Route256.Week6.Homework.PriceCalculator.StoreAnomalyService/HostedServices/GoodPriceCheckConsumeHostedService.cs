using Route256.Week6.Homework.PriceCalculator.StoreAnomalyService.ProcessingServices.Interfaces;

namespace Route256.Week6.Homework.PriceCalculator.StoreAnomalyService
{
    public class GoodPriceCheckConsumeHostedService : BackgroundService
    {
        private readonly ILogger<GoodPriceCheckConsumeHostedService> _logger;

        public GoodPriceCheckConsumeHostedService(
            IServiceProvider services,
            ILogger<GoodPriceCheckConsumeHostedService> logger)
        {
            Services = services;
            _logger = logger;
        }

        public IServiceProvider Services { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "GoodPriceCheckConsumeHostedService is running.");

            await DoWork(stoppingToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "GoodPriceCheckConsumeHostedService is working.");

            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService =
                    scope.ServiceProvider
                        .GetRequiredService<IGoodPriceCheckProcessingService>();

                await scopedProcessingService.DoWork(stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "GoodPriceCheckConsumeHostedService is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}
