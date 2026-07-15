using FluentMigrator;

namespace itinerary_be.Migration.Database;

[Migration(20260715130001, "Create User Google Tokens Table")]
public class CreateUserGoogleTokensTable : FluentMigrator.Migration
{
    public override void Up()
    {
        Create.Table("user_google_tokens").InSchema(Schemas.Itinerary)
            .WithColumn("user_id").AsGuid().PrimaryKey()
            .WithColumn("access_token").AsCustom("text").NotNullable()
            .WithColumn("refresh_token").AsCustom("text").Nullable()
            .WithColumn("expires_at").AsDateTime().NotNullable()
            .WithColumn("scope").AsString(500).NotNullable()
            .WithColumn("updated_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

        Create.ForeignKey("fk_user_google_tokens_user_id")
            .FromTable("user_google_tokens").InSchema(Schemas.Itinerary).ForeignColumn("user_id")
            .ToTable("users").InSchema(Schemas.Itinerary).PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);
    }

    public override void Down()
    {
        Delete.ForeignKey("fk_user_google_tokens_user_id").OnTable("user_google_tokens").InSchema(Schemas.Itinerary);
        Delete.Table("user_google_tokens").InSchema(Schemas.Itinerary);
    }
}
