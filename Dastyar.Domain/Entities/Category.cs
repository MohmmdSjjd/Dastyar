using Dastyar.Domain.Common;

namespace Dastyar.Domain.Entities;

public sealed class Category : BaseEntity<Guid>, IFilterable, ISoftDeletedEntity<Guid>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? UnitDefault { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public List<Category> Children { get; init; } = new();
    public List<Product> Products { get; init; } = new();
    public List<Material> Materials { get; init; } = new();

    public bool Deleted { get; private set; }
    public DateTime? DeletedOn { get; private set; }
    public int? DeletedById { get; private set; }
    public int? DeletedByDelegatedId { get; private set; }

    public string[] GetFilterableFields() => new[] { "Code", "Name", "ParentCategoryId", "UnitDefault" };

    public void SoftDelete(int? deletedById, int? deletedByDelegatedUserId)
    {
        Deleted = true;
        DeletedOn = DateTime.UtcNow;
        DeletedById = deletedById;
        DeletedByDelegatedId = deletedByDelegatedUserId;
    }
}
