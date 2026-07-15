namespace itinerary_be.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using itinerary_be.Core.Domain.Entities;

public class UserGoogleTokenConfiguration : IEntityTypeConfiguration<UserGoogleToken>
{
    public void Configure(EntityTypeBuilder<UserGoogleToken> builder)
    {
        builder.ToTable("user_google_tokens");

        builder.HasKey(t => t.UserId);

        builder.Property(t => t.AccessToken)
               .IsRequired();

        builder.Property(t => t.Scope)
               .IsRequired()
               .HasMaxLength(500);

        builder.Property(t => t.UpdatedAt)
               .HasDefaultValueSql("timezone('utc', now())");

        builder.HasOne(t => t.User)
               .WithOne()
               .HasForeignKey<UserGoogleToken>(t => t.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
