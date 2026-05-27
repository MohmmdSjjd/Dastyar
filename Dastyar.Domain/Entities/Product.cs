using Dastyar.Domain.Common;

namespace Dastyar.Domain.Entities;

public sealed class Product : BaseEntity<Guid>, IFilterable, ISoftDeletedEntity<Guid>
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }

    // ذخیره مقادیر داینامیک از اکسل به صورت JSON
    public string DynamicFieldsJson { get; set; } = string.Empty;

    public bool Deleted { get; private set; }
    public DateTime? DeletedOn { get; private set; }
    public int? DeletedById { get; private set; }
    public int? DeletedByDelegatedId { get; private set; }

    // برای محصولات داینامیک، فیلدهای قابل فیلتر به صورت پویا تعیین خواهند شد.
    // در حال حاضر، سیستم از جستجوی ساده‌ی متن روی JSON پشتیبانی می‌کند.
    public string[] GetFilterableFields() => new[] { "Code", "Name", "IsActive", "CategoryId", "CreatedAtUtc" };

    public void SoftDelete(int? deletedById, int? deletedByDelegatedUserId)
    {
        Deleted = true;
        DeletedOn = DateTime.UtcNow;
        DeletedById = deletedById;
        DeletedByDelegatedId = deletedByDelegatedUserId;
    }
}
