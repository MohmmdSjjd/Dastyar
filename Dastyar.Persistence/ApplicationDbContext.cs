using Dastyar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Dastyar.Persistence;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductMaterial> ProductMaterials => Set<ProductMaterial>();
    public DbSet<MaterialAddonAssignment> MaterialAddonAssignments => Set<MaterialAddonAssignment>();
    public DbSet<MaterialPriceChangeLog> MaterialPriceChangeLogs => Set<MaterialPriceChangeLog>();
    public DbSet<FieldDefinition> FieldDefinitions => Set<FieldDefinition>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        foreach (var entityType in EntityTypeDiscovery.GetEntityTypes(typeof(BaseEntity<>).Assembly))
        {
            modelBuilder.Entity(entityType);
        }

        base.OnModelCreating(modelBuilder);
    }

    private static class EntityTypeDiscovery
    {
        public static IReadOnlyList<Type> GetEntityTypes(Assembly assembly)
        {
            var baseType = typeof(BaseEntity<>);

            return assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && IsSubclassOfRawGeneric(baseType, type))
                .ToList();
        }

        private static bool IsSubclassOfRawGeneric(Type generic, Type? type)
        {
            while (type != null && type != typeof(object))
            {
                var current = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (generic == current)
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }
    }
}
