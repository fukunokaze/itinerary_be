using FluentMigrator;

namespace itinerary_be.Migration.Database
{
    [Migration(2023091503, "Create Flights Table")]
    public class CreateFlightsTable : FluentMigrator.Migration
    {
        public override void Up()
        {
            Create.Table("flights").InSchema(Schemas.Itinerary)
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("trip_id").AsGuid().NotNullable()
                .WithColumn("flight_number").AsString(50).NotNullable()
                .WithColumn("departure_time").AsDateTimeOffset().NotNullable()
                .WithColumn("arrival_time").AsDateTimeOffset().NotNullable();

            Execute.Sql($"ALTER TABLE {Schemas.Itinerary}.flights ALTER COLUMN id SET DEFAULT gen_random_uuid();");

            Create.ForeignKey("fk_flights_trip_id")
                .FromTable("flights").InSchema(Schemas.Itinerary).ForeignColumn("trip_id")
                .ToTable("trips").InSchema(Schemas.Itinerary).PrimaryColumn("id");
        }

        public override void Down()
        {
            Delete.ForeignKey("fk_flights_trip_id").OnTable("flights").InSchema(Schemas.Itinerary);
            Delete.Table("flights").InSchema(Schemas.Itinerary);
        }
    }
}
