using Dapper;
using Microsoft.Extensions.Options;
using Route256.Week5.Workshop.PriceCalculator.Dal.Entities;
using Route256.Week5.Workshop.PriceCalculator.Dal.Models;
using Route256.Week5.Workshop.PriceCalculator.Dal.Repositories.Interfaces;
using Route256.Week5.Workshop.PriceCalculator.Dal.Settings;
using Route256.Week6.Homework.PriceCalculator.Dal.Entities;

namespace Route256.Week5.Workshop.PriceCalculator.Dal.Repositories;

public class AnomalyPriceRepository : BaseRepository, IAnomalyPriceRepository
{
    public AnomalyPriceRepository(
        IOptions<DalOptions> dalSettings) : base(dalSettings.Value)
    {
    }
    
    public async Task Save(
        SaveAnomalyPriceEntityV1 entityV1, 
        CancellationToken token)
    {
        const string sqlQuery = @"
            insert into anomaly_prices (good_id, price)
            values (@GoodId, @Price);
            ";
        
        var sqlQueryParams = new
        {
            GoodId = entityV1.GoodId,
            Price = entityV1.Price
        };
        
        await using var connection = await GetAndOpenConnection();
        var ids = await connection.QueryAsync<long>(
            new CommandDefinition(
                sqlQuery,
                sqlQueryParams,
                cancellationToken: token));
    }
}