using Dastyar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dastyar.Persistence.Configurations;

public sealed class MaterialAddonAssignmentConfiguration : IEntityTypeConfiguration<MaterialAddonAssignment>
{
    public void Configure(EntityTypeBuilder<MaterialAddonAssignment> builder)
    {
        builder.ToTable("MaterialAddonAssignments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity).IsRequired();
        builder.Property(x => x.UnitOverride).HasMaxLength(50);

        builder.HasOne(x => x.AddonMaterial)
            .WithMany()
            .HasForeignKey(x => x.AddonMaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TargetCategory)
            .WithMany()
            .HasForeignKey(x => x.TargetCategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.TargetMaterial)
            .WithMany()
            .HasForeignKey(x => x.TargetMaterialId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.AddonMaterialId);
        builder.HasIndex(x => x.TargetCategoryId);
        builder.HasIndex(x => x.TargetMaterialId);
    }
}
