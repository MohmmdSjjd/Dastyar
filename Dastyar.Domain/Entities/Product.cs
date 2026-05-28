using Dastyar.Domain.Common;

namespace Dastyar.Domain.Entities;

public sealed class Product : BaseEntity<Guid>, IFilterable, ISoftDeletedEntity<Guid>
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? Unit { get; set; }

    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }

    public string DynamicFieldsJson { get; set; } = string.Empty;

    public bool Deleted { get; private set; }
    public DateTime? DeletedOn { get; private set; }
    public int? DeletedById { get; private set; }
    public int? DeletedByDelegatedId { get; private set; }

    public string[] GetFilterableFields() => new[] { "Code", "Name", "IsActive", "CategoryId", "CreatedAtUtc", "Unit" };

    public void SoftDelete(int? deletedById, int? deletedByDelegatedUserId)
    {
        Deleted = true;
        DeletedOn = DateTime.UtcNow;
        DeletedById = deletedById;
        DeletedByDelegatedId = deletedByDelegatedUserId;
    }
}
