using Dastyar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dastyar.Persistence.Configurations;

public sealed class MaterialPriceChangeLogConfiguration : IEntityTypeConfiguration<MaterialPriceChangeLog>
{
    public void Configure(EntityTypeBuilder<MaterialPriceChangeLog> builder)
    {
        builder.ToTable("MaterialPriceChangeLogs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PriceType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ChangeSource).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ChangedAtUtc).IsRequired();
        builder.Property(x => x.ChangedAtUtc).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(x => x.BatchId).IsRequired(false);

        builder.HasIndex(x => new { x.MaterialId, x.ChangedAtUtc });

        builder.HasOne(x => x.Material)
            .WithMany()
            .HasForeignKey(x => x.MaterialId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
