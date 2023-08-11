namespace Route256.Week6.Homework.PriceCalculator.BackgroundServices.Models
{
    public record ConsumeGoodPriceCalcModel(
        long GoodId,
        double Height,
        double Length,
        double Width,
        double Weight);
}
