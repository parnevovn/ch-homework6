using FluentMigrator;

namespace Route256.Week5.Workshop.PriceCalculator.Dal.Migrations;

[Migration(20230414, TransactionBehavior.None)]
public class AddAnomalyPricesV1 : Migration
{
    public override void Up()
    {
        Create.Table("anomaly_prices")
            .WithColumn("id").AsInt64().PrimaryKey("anomaly_prices_pk").Identity()
            .WithColumn("good_id").AsInt64().NotNullable()
            .WithColumn("price").AsDouble().NotNullable();

        const string sql = @"
            DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'anomaly_prices_v1') THEN
                        CREATE TYPE anomaly_prices_v1 as
                        (
                              id      bigint
                            , good_id bigint
                            , price   double precision
                        );
                    END IF;
                END
            $$;";

        Execute.Sql(sql);
    }

    public override void Down()
    {
        const string sql = @"
            DO $$
                BEGIN
                    DROP TYPE IF EXISTS anomaly_prices_v1;
                END
            $$;";

        Execute.Sql(sql);

        Delete.Table("anomaly_prices");
    }
}