namespace Dastyar.Domain.Common;

public interface IEntity<TKey>
{
    TKey Id { get; set; }
    DateTime CreatedOn { get; }
    DateTime? UpdatedOn { get; }
    string Description { get; set; }
    int? CreatedById { get; }
    int? LastUpdatedById { get; }
    int? CreatedByDelegatedId { get; }
    int? LastUpdatedByDelegatedId { get; }
    void Update(int? userId, int? delegatedUserId, DateTime updatedOn);
    void SetCreatedById(int? userId, int? delegatedUserId);
    void SetCreatedOn(DateTime createdOn);
}

public interface IEntity : IEntity<int> { }
