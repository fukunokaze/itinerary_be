using FluentMigrator;

namespace itinerary_be.Migration.Database
{
    [Migration(20260708145601, "Alter Flights Table Add Airline Seat Confirmation Code")]
    public class AlterFlightsTableAddAirlineSeatConfirmationCode : FluentMigrator.Migration
    {
        public override void Up()
        {
            Alter.Table("flights").InSchema(Schemas.Itinerary)
                .AddColumn("airline").AsString(100).Nullable()
                .AddColumn("seat").AsString(20).Nullable()
                .AddColumn("confirmation_code").AsString(50).Nullable();
        }

        public override void Down()
        {
            Delete.Column("airline").FromTable("flights").InSchema(Schemas.Itinerary);
            Delete.Column("seat").FromTable("flights").InSchema(Schemas.Itinerary);
            Delete.Column("confirmation_code").FromTable("flights").InSchema(Schemas.Itinerary);
        }
    }
}
