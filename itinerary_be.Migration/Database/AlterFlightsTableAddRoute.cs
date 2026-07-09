using FluentMigrator;

namespace itinerary_be.Migration.Database
{
    [Migration(20260709131401, "Alter Flights Table Add Route")]
    public class AlterFlightsTableAddRoute : FluentMigrator.Migration
    {
        public override void Up()
        {
            Alter.Table("flights").InSchema(Schemas.Itinerary)
                .AddColumn("route").AsString(500).Nullable();
        }

        public override void Down()
        {
            Delete.Column("route").FromTable("flights").InSchema(Schemas.Itinerary);
        }
    }
}
