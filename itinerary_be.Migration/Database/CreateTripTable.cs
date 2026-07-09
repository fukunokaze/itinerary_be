
using FluentMigrator;

namespace itinerary_be.Migration.Database
{
    [Migration(20260701000001, "Create Trip Table")]
    public class CreateTripTable : FluentMigrator.Migration
    {
        public override void Up()
        {
            Create.Table("trips").InSchema(Schemas.Itinerary)
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("title").AsString(200).NotNullable()
                .WithColumn("start_date").AsDate().NotNullable()
                .WithColumn("end_date").AsDate().NotNullable();

            Execute.Sql($"ALTER TABLE {Schemas.Itinerary}.trips ALTER COLUMN id SET DEFAULT gen_random_uuid();");
        }

        public override void Down()
        {
            Delete.Table("trips").InSchema(Schemas.Itinerary);
        }
    }
}
