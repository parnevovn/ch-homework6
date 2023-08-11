namespace Route256.Week6.Homework.PriceCalculator.BackgroundServices.Options;

public sealed class KafkaOptions
{
    public string TopicConsumeCalcRequests { get; set; }
    public string TopicProduceCalc { get; set; }
    public string TopicProduceCalcDlq { get; set; }

    public string KafkaHost { get; set; }
}