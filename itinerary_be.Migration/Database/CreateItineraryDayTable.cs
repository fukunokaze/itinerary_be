using FluentMigrator;

namespace itinerary_be.Migration.Database
{
    [Migration(2023091502, "Create ItineraryDay Table")]
    public class CreateItineraryDayTable : FluentMigrator.Migration
    {
        public override void Up()
        {
            Create.Table("itinerary_days").InSchema(Schemas.Itinerary)
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("trip_id").AsGuid().NotNullable()
                .WithColumn("date").AsDate().NotNullable()
                .WithColumn("notes").AsString(1000).Nullable();

            Execute.Sql($"ALTER TABLE {Schemas.Itinerary}.itinerary_days ALTER COLUMN id SET DEFAULT gen_random_uuid();");

            Create.ForeignKey("fk_itinerary_days_trip_id")
                .FromTable("itinerary_days").InSchema(Schemas.Itinerary).ForeignColumn("trip_id")
                .ToTable("trips").InSchema(Schemas.Itinerary).PrimaryColumn("id");
        }

        public override void Down()
        {
            Delete.ForeignKey("fk_itinerary_days_trip_id").OnTable("itinerary_days").InSchema(Schemas.Itinerary);
            Delete.Table("itinerary_days").InSchema(Schemas.Itinerary);
        }
    }
}
