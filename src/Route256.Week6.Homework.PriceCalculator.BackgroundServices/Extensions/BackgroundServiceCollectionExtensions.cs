using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.Options;
using Route256.Week5.Workshop.PriceCalculator.Bll.Commands;
using Route256.Week6.Homework.PriceCalculator.BackgroundServices.Converters;
using Route256.Week6.Homework.PriceCalculator.BackgroundServices.Models;
using Route256.Week6.Homework.PriceCalculator.BackgroundServices.Options;
using Route256.Week6.Homework.PriceCalculator.BackgroundServices.ProcessingServices;
using Route256.Week6.Homework.PriceCalculator.BackgroundServices.ProcessingServices.Interfaces;

namespace Route256.Week6.Homework.PriceCalculator.BackgroundServices.Extensions;

public static class BackgroundServiceCollectionExtensions
{
    public static IServiceCollection AddBackgroundService(
        this IServiceCollection services,
        IConfigurationRoot config)
    {
        services
            .AddMediatR(c => c.RegisterServicesFromAssembly(typeof(BackgroundServiceCollectionExtensions).Assembly))
            .AddTransient<IRequestHandler<CalculateDeliveryPriceNoSaveCommand, decimal>, CalculateDeliveryPriceNoSaveCommandHandler>()
            .Configure<KafkaOptions>(config.GetSection(key: "KafkaOptions"))
            .AddScoped(x => x.GetRequiredService<IOptionsSnapshot<KafkaOptions>>().Value)
            .AddScoped<IGoodPriceCalcProcessingService, GoodPriceCalcProcessingService>(x =>
            {
                return new GoodPriceCalcProcessingService(
                    new ConsumerBuilder<long, ConsumeGoodPriceCalcModel>(
                        new ConsumerConfig
                        {
                            BootstrapServers = config.GetSection("KafkaOptions").GetValue<string>("KafkaHost"),
                            GroupId = "PriceCalculatorBackgroundService",
                            EnableAutoCommit = true,
                            EnableAutoOffsetStore = false
                        })
                        .SetValueDeserializer(new JsonValueSerializer<ConsumeGoodPriceCalcModel>())
                        .Build(),
                    new ProducerBuilder<long, ProduceGoodPriceModel>(
                        new ProducerConfig
                        {
                            BootstrapServers = config.GetSection("KafkaOptions").GetValue<string>("KafkaHost"),
                            Acks = Acks.All
                        })
                        .SetValueSerializer(new JsonValueSerializer<ProduceGoodPriceModel>())
                        .Build(),
                    new ProducerBuilder<byte[], byte[]>(
                        new ProducerConfig
                        {
                            BootstrapServers = config.GetSection("KafkaOptions").GetValue<string>("KafkaHost"),
                            Acks = Acks.All
                        })
                        .Build(),
                    x.GetService<ILogger<GoodPriceCalcProcessingService>>(),
                    x.GetRequiredService<IOptionsSnapshot<KafkaOptions>>().Value,
                    x.GetRequiredService<IMediator>()
                    );
            })
            .AddHostedService<GoodPriceCalcConsumeHostedService>();
        return services;
    }
}