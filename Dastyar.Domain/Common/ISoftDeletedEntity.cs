namespace Dastyar.Domain.Common;

public interface ISoftDeletedEntity<TKey> : IEntity<TKey>
{
    bool Deleted { get; }
    DateTime? DeletedOn { get; }
    int? DeletedById { get; }
    int? DeletedByDelegatedId { get; }
    void SoftDelete(int? deletedById, int? deletedByDelegatedUserId);
}

public interface ISoftDeletedEntity : ISoftDeletedEntity<int>, IEntity { }
