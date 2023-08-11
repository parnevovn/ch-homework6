using Confluent.Kafka;
using MediatR;

using Route256.Week5.Workshop.PriceCalculator.Bll.Commands;
using Route256.Week5.Workshop.PriceCalculator.Bll.Models;
using Route256.Week6.Homework.PriceCalculator.BackgroundServices.Converters;
using Route256.Week6.Homework.PriceCalculator.BackgroundServices.Models;
using Route256.Week6.Homework.PriceCalculator.BackgroundServices.Options;
using Route256.Week6.Homework.PriceCalculator.BackgroundServices.ProcessingServices.Interfaces;
using System.Text;
using System.Threading.Channels;

namespace Route256.Week6.Homework.PriceCalculator.BackgroundServices.ProcessingServices
{
    internal class GoodPriceCalcProcessingService : IGoodPriceCalcProcessingService
    {
        private int executionCount = 0;

        private IConsumer<long, ConsumeGoodPriceCalcModel> _consumer;
        private IProducer<long, ProduceGoodPriceModel> _producer;
        private IProducer<byte[], byte[]> _producerDlq;
        private readonly ILogger _logger;
        private readonly KafkaOptions _options;
        private readonly IMediator _mediator;

        public GoodPriceCalcProcessingService(
            IConsumer<long, ConsumeGoodPriceCalcModel> consumer,
            IProducer<long, ProduceGoodPriceModel> producer,
            IProducer<byte[], byte[]> producerDlq,
            ILogger<GoodPriceCalcProcessingService> logger,
            KafkaOptions options,
            IMediator mediator)
        {
            _consumer = consumer;
            _producer = producer;
            _producerDlq = producerDlq;
            _logger = logger;
            _options = options;
            _mediator = mediator;
        }

        public async Task DoWork(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_options.TopicConsumeCalcRequests);

            while (!stoppingToken.IsCancellationRequested)
            {
                executionCount++;

                var channelConsume = Channel.CreateUnbounded<ConsumeResult<long, ConsumeGoodPriceCalcModel>>();
                var channelProduce = Channel.CreateUnbounded<IDictionary<ConsumeResult<long, ConsumeGoodPriceCalcModel>, ProduceGoodPriceModel>>();
                var channelProduceBlq = Channel.CreateUnbounded<ConsumeResult<long, byte[]>>();

                var doReadConsume = Task.Run(async () =>
                {
                    try
                    {
                        while (_consumer.Consume(stoppingToken) is { } consumeResult)
                        {
                            await channelConsume.Writer.WriteAsync(consumeResult, stoppingToken);

                            _logger.LogInformation(
                                "{0}:{1}:Write consume to channel",
                                consumeResult.Partition.Value,
                                consumeResult.Offset.Value);
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        var consumeResult = ConvertConsumeExceptionRecordToConsumeResult(ex.ConsumerRecord);

                        await channelProduceBlq.Writer.WriteAsync(consumeResult, stoppingToken);

                        _logger.LogInformation(
                                "Error Consume: {0}", ex.Message);
                    }

                    channelConsume.Writer.Complete();
                });

                var doCalc = Task.Run(async () =>
                {
                    await foreach (var consumeResult in channelConsume.Reader.ReadAllAsync(stoppingToken))
                    {
                        decimal resultPrice = 0;
                        bool    isError = false;

                        var goodModel = new GoodModel(
                            Height: consumeResult.Message.Value.Height,
                            Length: consumeResult.Message.Value.Length,
                            Width: consumeResult.Message.Value.Width,
                            Weight: consumeResult.Message.Value.Weight
                        );

                        try
                        {
                            resultPrice = await getCalcPrice(goodModel, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            // если какие-то проверки слоя BLL не прошли, то тоже записываем невалидные данные в DLQ
                            isError = true;

                            _logger.LogInformation(
                                "Error Calc: {0}", ex.Message);
                        }

                        if (isError)
                        {
                            await channelProduceBlq.Writer.WriteAsync(
                                ConvertConsumeResultToByte(consumeResult), 
                                stoppingToken);
                        }
                        else
                        {
                            var dictionary = new Dictionary<ConsumeResult<long, ConsumeGoodPriceCalcModel>, ProduceGoodPriceModel>();

                            dictionary.Add(
                                consumeResult,
                                new ProduceGoodPriceModel(
                                    consumeResult.Message.Value.GoodId,
                                    resultPrice));

                            await channelProduce.Writer.WriteAsync(
                                dictionary,
                                stoppingToken);

                            _logger.LogInformation(
                               "{0}:{1}:Calculated:{2}:{3}",
                                consumeResult.Partition.Value,
                                consumeResult.Offset.Value,
                                consumeResult.Message.Key,
                                consumeResult);
                        }
                    }

                    channelProduce.Writer.Complete();
                    channelProduceBlq.Writer.Complete();
                });

                var doWriteProduce = Task.Run(async () =>
                {
                    await foreach (var dictionary in channelProduce.Reader.ReadAllAsync(stoppingToken))
                    {
                        var consumeResult = dictionary.First().Key;
                        var produceGoodPriceModel = dictionary.First().Value;

                        await _producer.ProduceAsync(
                            _options.TopicProduceCalc,
                            new Message<long, ProduceGoodPriceModel>
                            {
                                Headers = new()
                                {
                                    { "Producer", Encoding.Default.GetBytes("Route 256 calculating price Producer") },
                                    { "Machine",  Encoding.Default.GetBytes(Environment.MachineName)}
                                },
                                Key = produceGoodPriceModel.GoodId,
                                Value = produceGoodPriceModel
                            },
                            stoppingToken);

                        _consumer.StoreOffset(consumeResult);

                        _logger.LogInformation(
                            "{0}:{1}:Send message to Kafka topic {2}",
                            consumeResult.Partition.Value,
                            consumeResult.Offset.Value,
                            _options.TopicProduceCalc);
                    }
                });

                var doWriteDlqProduce = Task.Run(async () =>
                {
                    await foreach (var consumeResult in channelProduceBlq.Reader.ReadAllAsync(stoppingToken))
                    {
                        await _producerDlq.ProduceAsync(
                            _options.TopicProduceCalcDlq,
                            new Message<byte[], byte[]>
                            {
                                Headers = new()
                                {
                                    { "Producer", Encoding.Default.GetBytes("Route 256 calculating price Producer") },
                                    { "Machine",  Encoding.Default.GetBytes(Environment.MachineName)}
                                },
                                Key = BitConverter.GetBytes(consumeResult.Message.Key),
                                Value = consumeResult.Message.Value
                            },
                            stoppingToken);

                        _consumer.StoreOffset(consumeResult.TopicPartitionOffset);
                     
                        _logger.LogInformation(
                            "{0}:{1}:Send bad message to Kafka topic {2}",
                            consumeResult.Partition.Value,
                            consumeResult.Offset.Value,
                            _options.TopicProduceCalcDlq);
                    }
                });

                _logger.LogInformation(
                    "GoodPriceCalcProcessingService is working. Count: {Count}", executionCount);

                await Task.WhenAll(doReadConsume, doCalc, doWriteProduce, doWriteDlqProduce);
            }
        }

        private async Task<decimal> getCalcPrice(GoodModel goodModel, CancellationToken stoppingToken)
        {
            var command = new CalculateDeliveryPriceNoSaveCommand(new[] { goodModel });

            var resultPrice = await _mediator.Send(command, stoppingToken);

            return resultPrice;
        }

        private ConsumeResult<long, byte[]> ConvertConsumeExceptionRecordToConsumeResult(ConsumeResult<byte[], byte[]> consumeException)
        {
            return new ConsumeResult<long, byte[]>
            {
                Message = new Message<long, byte[]>
                {
                    Headers = consumeException.Message.Headers,
                    Key = BitConverter.ToInt64(consumeException.Message.Key),
                    Value = consumeException.Message.Value
                },
                Topic = consumeException.Topic,
                Partition = consumeException.Partition,
                Offset = consumeException.Offset,
                IsPartitionEOF = consumeException.IsPartitionEOF
            };
        }

        private ConsumeResult<long, byte[]> ConvertConsumeResultToByte(ConsumeResult<long, ConsumeGoodPriceCalcModel> consumeResult)
        {
            return new ConsumeResult<long, byte[]>
            {
                Message = new Message<long, byte[]>
                {
                    Headers = consumeResult.Message.Headers,
                    Key = consumeResult.Message.Key,
                    Value = new JsonValueSerializer<ConsumeGoodPriceCalcModel>().Serialize(consumeResult.Message.Value, new SerializationContext())
                },
                Topic = consumeResult.Topic,
                Partition = consumeResult.Partition,
                Offset = consumeResult.Offset,
                IsPartitionEOF = consumeResult.IsPartitionEOF
            };
        }
    }

}
