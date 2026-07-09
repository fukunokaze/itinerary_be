using FluentMigrator;

namespace itinerary_be.Migration.Database;

[Migration(20260709143401, "Alter Tables Add Cost to TripEvents, Lodgings, Flights")]
public class AlterTablesAddCost : FluentMigrator.Migration
{
    public override void Up()
    {
        Alter.Table("trip_events").InSchema(Schemas.Itinerary)
            .AddColumn("cost").AsDecimal().Nullable();

        Alter.Table("lodgings").InSchema(Schemas.Itinerary)
            .AddColumn("cost").AsDecimal().Nullable();

        Alter.Table("flights").InSchema(Schemas.Itinerary)
            .AddColumn("cost").AsDecimal().Nullable();
    }

    public override void Down()
    {
        Delete.Column("cost").FromTable("flights").InSchema(Schemas.Itinerary);
        Delete.Column("cost").FromTable("lodgings").InSchema(Schemas.Itinerary);
        Delete.Column("cost").FromTable("trip_events").InSchema(Schemas.Itinerary);
    }
}
