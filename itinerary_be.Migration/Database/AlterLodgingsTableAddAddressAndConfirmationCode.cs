using FluentMigrator;

namespace itinerary_be.Migration.Database;

[Migration(202607081500, "Alter Lodgings Table Add Address, Confirmation Code and Trip FK")]
public class AlterLodgingsTableAddAddressAndConfirmationCode : FluentMigrator.Migration
{
    public override void Up()
    {
        Alter.Table("lodgings").InSchema(Schemas.Itinerary)
            .AddColumn("address").AsString(1000).NotNullable().WithDefaultValue(string.Empty)
            .AddColumn("confirmation_code").AsString(100).NotNullable().WithDefaultValue(string.Empty);

        Create.ForeignKey("fk_lodgings_trip_id")
            .FromTable("lodgings").InSchema(Schemas.Itinerary).ForeignColumn("trip_id")
            .ToTable("trips").InSchema(Schemas.Itinerary).PrimaryColumn("id")
            .OnDeleteOrUpdate(System.Data.Rule.Cascade);
    }

    public override void Down()
    {
        Delete.ForeignKey("fk_lodgings_trip_id").OnTable("lodgings").InSchema(Schemas.Itinerary);
        Delete.Column("address").FromTable("lodgings").InSchema(Schemas.Itinerary);
        Delete.Column("confirmation_code").FromTable("lodgings").InSchema(Schemas.Itinerary);
    }
}
