using FluentMigrator;

namespace itinerary_be.Migration.Database;

[Migration(202606061251, "Alter Trip Table Add Description and Destination")]
public class AlterTripTableAddDescriptionAndDestination : FluentMigrator.Migration
{
    public override void Up()
    {
        Alter.Table("trips").InSchema(Schemas.Itinerary)
            .AddColumn("destination").AsString(200).NotNullable().WithDefaultValue(string.Empty)
            .AddColumn("description").AsString(1000).Nullable().WithDefaultValue(string.Empty);
    }

    public override void Down()
    {
        Delete.Column("destination").FromTable("trips").InSchema(Schemas.Itinerary);
        Delete.Column("description").FromTable("trips").InSchema(Schemas.Itinerary);
    }
}