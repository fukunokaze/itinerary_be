using FluentMigrator;

namespace itinerary_be.Migration;

[Migration(202607071404, "Create Trip Event Table")]
public class CreateTripEventTable : FluentMigrator.Migration
{
    public override void Up()
    {
        Execute.Sql("CREATE TYPE itinerary.event_type AS ENUM ('activity', 'flight', 'lodging');");

        Create.Table("trip_events").InSchema(Schemas.Itinerary)
            .WithColumn("id").AsGuid().PrimaryKey().NotNullable()
            .WithColumn("trip_id").AsGuid().NotNullable()
            .WithColumn("type").AsCustom(Schemas.Itinerary + ".event_type").NotNullable()
            .WithColumn("title").AsString(200).NotNullable()
            .WithColumn("location").AsString(1000).Nullable()
            .WithColumn("notes").AsString(1000).Nullable()
            .WithColumn("booking_code").AsString(100).Nullable()
            .WithColumn("image_url").AsString(1000).Nullable()
            .WithColumn("tags").AsString(1000).Nullable()
            .WithColumn("date").AsDate().NotNullable()
            .WithColumn("start_time").AsTime().Nullable()
            .WithColumn("end_time").AsTime().Nullable();

        Create.ForeignKey("fk_trip_events_trip_id")
            .FromTable("trip_events").InSchema(Schemas.Itinerary).ForeignColumn("trip_id")
            .ToTable("trips").InSchema(Schemas.Itinerary).PrimaryColumn("id")
            .OnDeleteOrUpdate(System.Data.Rule.Cascade);

        Create.Index("idx_trip_events_type")
            .OnTable("trip_events").InSchema(Schemas.Itinerary)
            .OnColumn("type").Ascending()
            .WithOptions().NonClustered();
    }

    public override void Down()
    {
        Delete.Index("idx_trip_events_type").OnTable("trip_events").InSchema(Schemas.Itinerary);
        Delete.ForeignKey("fk_trip_events_trip_id").OnTable("trip_events").InSchema(Schemas.Itinerary);
        Delete.Table("trip_events").InSchema(Schemas.Itinerary);
        Execute.Sql("DROP TYPE " + Schemas.Itinerary + ".event_type;");
    }
}