
using FluentMigrator;

namespace itinerary_be.Migration.Database
{
    [Migration(20260701000000, "Create Itinerary Schema")]
    public class CreateItinerarySchema : FluentMigrator.Migration
    {
        public override void Up()
        {
            Execute.Sql($"CREATE SCHEMA IF NOT EXISTS {Schemas.Itinerary};");
        }

        public override void Down()
        {
            Execute.Sql($"DROP SCHEMA IF EXISTS {Schemas.Itinerary} CASCADE;");
        }
    }
}
