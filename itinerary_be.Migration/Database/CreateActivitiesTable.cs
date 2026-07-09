using FluentMigrator;

namespace itinerary_be.Migration.Database
{
    [Migration(20260701000005, "Create Activities Table")]
    public class CreateActivitiesTable : FluentMigrator.Migration
    {
        public override void Up()
        {
            Create.Table("activities").InSchema(Schemas.Itinerary)
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("itinerary_day_id").AsGuid().NotNullable()
                .WithColumn("name").AsString(200).NotNullable()
                .WithColumn("start_time").AsTime().Nullable()
                .WithColumn("location").AsString(500).Nullable();

            Execute.Sql($"ALTER TABLE {Schemas.Itinerary}.activities ALTER COLUMN id SET DEFAULT gen_random_uuid();");

            Create.ForeignKey("fk_activities_itinerary_day_id")
                .FromTable("activities").InSchema(Schemas.Itinerary).ForeignColumn("itinerary_day_id")
                .ToTable("itinerary_days").InSchema(Schemas.Itinerary).PrimaryColumn("id");
        }

        public override void Down()
        {
            Delete.ForeignKey("fk_activities_itinerary_day_id").OnTable("activities").InSchema(Schemas.Itinerary);
            Delete.Table("activities").InSchema(Schemas.Itinerary);
        }
    }
}
 