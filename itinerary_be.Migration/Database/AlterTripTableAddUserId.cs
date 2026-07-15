using FluentMigrator;

namespace itinerary_be.Migration.Database
{
    [Migration(20260714000001, "Alter Trip Table Add UserId")]
    public class AlterTripTableAddUserId : FluentMigrator.Migration
    {
        public override void Up()
        {
            // Existing trips predate user ownership and have no owner to assign; clear them
            // so user_id can be added as a NOT NULL foreign key.
            Execute.Sql($"TRUNCATE TABLE {Schemas.Itinerary}.trips CASCADE;");

            Alter.Table("trips").InSchema(Schemas.Itinerary)
                .AddColumn("user_id").AsGuid().NotNullable();

            Create.ForeignKey("fk_trips_user_id")
                .FromTable("trips").InSchema(Schemas.Itinerary).ForeignColumn("user_id")
                .ToTable("users").InSchema(Schemas.Itinerary).PrimaryColumn("id")
                .OnDelete(System.Data.Rule.Cascade);

            Create.Index("ix_trips_user_id")
                .OnTable("trips").InSchema(Schemas.Itinerary)
                .OnColumn("user_id").Ascending();
        }

        public override void Down()
        {
            Delete.Index("ix_trips_user_id").OnTable("trips").InSchema(Schemas.Itinerary);
            Delete.ForeignKey("fk_trips_user_id").OnTable("trips").InSchema(Schemas.Itinerary);
            Delete.Column("user_id").FromTable("trips").InSchema(Schemas.Itinerary);
        }
    }
}
