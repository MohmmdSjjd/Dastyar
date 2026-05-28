using Dastyar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dastyar.Persistence.Configurations;

public sealed class ProductMaterialConfiguration : IEntityTypeConfiguration<ProductMaterial>
{
    public void Configure(EntityTypeBuilder<ProductMaterial> builder)
    {
        builder.ToTable("ProductMaterials");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.WastePercent).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.Unit).HasMaxLength(50);
        builder.Property(x => x.SortOrder).IsRequired();

        builder.HasIndex(x => new { x.ProductId, x.MaterialId }).IsUnique();

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Material)
            .WithMany()
            .HasForeignKey(x => x.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
