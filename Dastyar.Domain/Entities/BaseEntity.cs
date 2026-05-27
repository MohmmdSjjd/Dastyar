using Dastyar.Domain.Common;

namespace Dastyar.Domain.Entities;

public abstract class BaseEntity<TId> : IEntity<TId>
{
    public TId Id { get; set; } = default!;
    public DateTime CreatedOn { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedOn { get; private set; }
    public string Description { get; set; } = string.Empty;
    public int? CreatedById { get; private set; }
    public int? LastUpdatedById { get; private set; }
    public int? CreatedByDelegatedId { get; private set; }
    public int? LastUpdatedByDelegatedId { get; private set; }

    public void Update(int? userId, int? delegatedUserId, DateTime updatedOn)
    {
        UpdatedOn = updatedOn;
        LastUpdatedById = userId;
        LastUpdatedByDelegatedId = delegatedUserId;
    }

    public void SetCreatedById(int? userId, int? delegatedUserId)
    {
        CreatedById = userId;
        CreatedByDelegatedId = delegatedUserId;
    }

    public void SetCreatedOn(DateTime createdOn)
    {
        CreatedOn = createdOn;
    }
}
