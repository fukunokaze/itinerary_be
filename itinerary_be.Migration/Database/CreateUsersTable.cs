using FluentMigrator;

namespace itinerary_be.Migration.Database;

[Migration(20260713120001, "Create Users Table")]
public class CreateUsersTable : FluentMigrator.Migration
{
    public override void Up()
    {
        Create.Table("users").InSchema(Schemas.Itinerary)
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("email").AsString(320).NotNullable()
            .WithColumn("name").AsString(200).NotNullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

        Execute.Sql($"ALTER TABLE {Schemas.Itinerary}.users ALTER COLUMN id SET DEFAULT gen_random_uuid();");

        Create.Index("ix_users_email")
            .OnTable("users").InSchema(Schemas.Itinerary)
            .OnColumn("email").Ascending()
            .WithOptions().Unique();
    }

    public override void Down()
    {
        Delete.Index("ix_users_email").OnTable("users").InSchema(Schemas.Itinerary);
        Delete.Table("users").InSchema(Schemas.Itinerary);
    }
}
