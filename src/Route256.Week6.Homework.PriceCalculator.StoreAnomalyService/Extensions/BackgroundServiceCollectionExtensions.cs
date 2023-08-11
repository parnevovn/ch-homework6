using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.Options;
using Route256.Week5.Workshop.PriceCalculator.Bll.Commands;
using Route256.Week6.Homework.PriceCalculator.StoreAnomalyService;
using Route256.Week6.Homework.PriceCalculator.StoreAnomalyService.Converters;
using Route256.Week6.Homework.PriceCalculator.StoreAnomalyService.Models;
using Route256.Week6.Homework.PriceCalculator.StoreAnomalyService.Options;
using Route256.Week6.Homework.PriceCalculator.StoreAnomalyService.ProcessingServices;
using Route256.Week6.Homework.PriceCalculator.StoreAnomalyService.ProcessingServices.Interfaces;

namespace RRoute256.Week6.Homework.PriceCalculator.StoreAnomalyService.Extensions;

public static class BackgroundServiceCollectionExtensions
{
    public static IServiceCollection AddStoreAnomalyService(
        this IServiceCollection services,
        IConfigurationRoot config)
    {
        services
            .AddMediatR(c => c.RegisterServicesFromAssembly(typeof(BackgroundServiceCollectionExtensions).Assembly))
            .AddTransient<IRequestHandler<SaveAnomalyPriceCommand, bool>, SaveAnomalyPriceCommandHandler>()
            .Configure<KafkaOptions>(config.GetSection(key: "KafkaOptions"))
            .AddScoped(x => x.GetRequiredService<IOptionsSnapshot<KafkaOptions>>().Value)
            .AddScoped<IGoodPriceCheckProcessingService, GoodPriceCheckProcessingService>(x =>
            {
                return new GoodPriceCheckProcessingService(
                    new ConsumerBuilder<long, ConsumeGoodPriceModel>(
                        new ConsumerConfig
                        {
                            BootstrapServers = config.GetSection("KafkaOptions").GetValue<string>("KafkaHost"),
                            GroupId = "PriceCalculatorBackgroundService",
                            EnableAutoCommit = true,
                            EnableAutoOffsetStore = false
                        })
                        .SetValueDeserializer(new JsonValueSerializer<ConsumeGoodPriceModel>())
                        .Build(),
                    x.GetService<ILogger<GoodPriceCheckProcessingService>>(),
                    x.GetRequiredService<IOptionsSnapshot<KafkaOptions>>().Value,
                    x.GetRequiredService<IMediator>()
                    );
            })
            .AddHostedService<GoodPriceCheckConsumeHostedService>();
        return services;
    }
}