using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Route256.Week6.Homework.GoodsFill;
using Route256.Week6.Homework.GoodsFill.Converters;
using Route256.Week6.Homework.GoodsFill.Models;

using System.Text;

var builder = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json", true, true);

IConfiguration config = builder.Build();

var goodsOptions = config.GetSection("GoodsOptions").Get<GoodsOptions>();

var producerTestConfig = new ProducerConfig
{
    BootstrapServers = "kafka:9092",
    Acks = Acks.All
};

var producerTest = new ProducerBuilder<long, ProduceGoodPriceCalcModel>(producerTestConfig).SetValueSerializer(new JsonValueSerializer<ProduceGoodPriceCalcModel>()).Build();

var goods = GenerateRandomData(goodsOptions.GoodsCount);

foreach (var good in goods)
{
    await producerTest.ProduceAsync(
        "good_price_calc_requests",
        new Message<long, ProduceGoodPriceCalcModel>
        {
            Headers = new()
            {
                { "Producer", Encoding.Default.GetBytes("Route 256 set goods Producer") },
                { "Machine",  Encoding.Default.GetBytes(Environment.MachineName)}
            },
            Key = good.GoodId,
            Value = good
        });
}

static ProduceGoodPriceCalcModel[] GenerateRandomData(int count)
{
    var random = new Random();
    var goods = new ProduceGoodPriceCalcModel[count];

    for (int i = 0; i < count; i++)
    {
        var goodId = (long)random.Next(1000, 10000);
        var height = random.NextDouble() * 100;
        var length = random.NextDouble() * 100;
        var width = random.NextDouble() * 100;
        var weight = random.NextDouble() * 100;

        goods[i] = new ProduceGoodPriceCalcModel
        (
            goodId,
            height,
            length,
            width,
            weight
        );
    }

    return goods;
}




