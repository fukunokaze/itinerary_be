using FluentMigrator;

namespace itinerary_be.Migration;

[Migration(202607071404, "Create Trip Event Table")]
public class CreateTripEventTable : FluentMigrator.Migration
{
    public override void Up()
    {
        Execute.Sql("CREATE TYPE event_type AS ENUM ('activity', 'flight', 'lodging');");

        Create.Table("trip_events").InSchema(Schemas.Itinerary)
            .WithColumn("id").AsGuid().PrimaryKey().NotNullable()
            .WithColumn("trip_id").AsGuid().NotNullable()
            .WithColumn("event_type").AsCustom("event_type").NotNullable()
            .WithColumn("title").AsString(200).NotNullable()
            .WithColumn("description").AsString(1000).Nullable()
            .WithColumn("start_time").AsDateTime().NotNullable()
            .WithColumn("end_time").AsDateTime().NotNullable();

        Create.ForeignKey("fk_trip_events_trip_id")
            .FromTable("trip_events").InSchema(Schemas.Itinerary).ForeignColumn("trip_id")
            .ToTable("trips").InSchema(Schemas.Itinerary).PrimaryColumn("id")
            .OnDeleteOrUpdate(System.Data.Rule.Cascade);

        Create.Index("idx_trip_events_event_type")
            .OnTable("trip_events").InSchema(Schemas.Itinerary)
            .OnColumn("event_type").Ascending()
            .WithOptions().NonClustered();
    }

    public override void Down()
    {
        Delete.Index("idx_trip_events_event_type").OnTable("trip_events").InSchema(Schemas.Itinerary);
        Delete.ForeignKey("fk_trip_events_trip_id").OnTable("trip_events").InSchema(Schemas.Itinerary);
        Delete.Table("trip_events").InSchema(Schemas.Itinerary);
        Execute.Sql("DROP TYPE event_type;");
    }
}