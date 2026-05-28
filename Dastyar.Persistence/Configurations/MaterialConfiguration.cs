using Dastyar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dastyar.Persistence.Configurations;

public sealed class MaterialConfiguration : IEntityTypeConfiguration<Material>
{
    public void Configure(EntityTypeBuilder<Material> builder)
    {
        builder.ToTable("Materials");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasFilter("[Code] IS NOT NULL");

        builder.Property(x => x.Code).HasMaxLength(100);
        builder.Property(x => x.Name).HasMaxLength(200);
        builder.Property(x => x.Unit).HasMaxLength(50);
        builder.Property(x => x.BasePrice).IsRequired();
        builder.Property(x => x.LastPurchasePrice).IsRequired();
        builder.Property(x => x.DailyPurchasePrice).IsRequired();
        builder.Property(x => x.DynamicFieldsJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Materials)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
