using FluentMigrator;

namespace itinerary_be.Migration.Database
{
    [Migration(2023091501, "Create Lodgings Table")]
    public class CreateLodgingsTable : FluentMigrator.Migration
    {
        public override void Up()
        {
            Create.Table("lodgings").InSchema(Schemas.Itinerary)
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("trip_id").AsGuid().NotNullable()
                .WithColumn("name").AsString(200).NotNullable()
                .WithColumn("check_in").AsDateTimeOffset().NotNullable()
                .WithColumn("check_out").AsDateTimeOffset().NotNullable();

            Execute.Sql($"ALTER TABLE {Schemas.Itinerary}.lodgings ALTER COLUMN id SET DEFAULT gen_random_uuid();");

        }

        public override void Down()
        {
            Delete.Table("lodgings").InSchema(Schemas.Itinerary);
        }
    }
}
