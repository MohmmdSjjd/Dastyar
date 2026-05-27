/* =========================================================
 * This file is part of the Csis.Template
 * Do not make any modifications on this file
 * All changes will be rejected in code review sessions
 * ========================================================= */

using System.Text.Json.Serialization;

namespace Csis.Template.Domain.Common;

/// <summary>
/// Abstract implementation of <see cref="ISoftDeletedEntity{TKey}"/>
/// </summary>
public abstract class SoftDeletedBaseEntity<TKey> : BaseEntity<TKey>, ISoftDeletedEntity<TKey>, IEntity<TKey>
{
    /// <inheritdoc/>
    [JsonInclude]
    public bool Deleted { get; private set; }

    /// <inheritdoc/>
    [JsonInclude]
    public DateTime? DeletedOn { get; private set; }

    /// <inheritdoc/>
    [JsonInclude]
    public int? DeletedById { get; private set; }

    /// <inheritdoc/>
    [JsonInclude]
    public int? DeletedByDelegatedId { get; private set; }

    /// <inheritdoc/>
    public virtual void SoftDelete(int? deletedById, int? deletedByDelegatedUserId) {
        Deleted = true;
        DeletedOn = DateTime.Now;
        DeletedById = deletedById;
        DeletedByDelegatedId = deletedByDelegatedUserId;
    }
}

/// <summary>
/// Abstract implementation of <see cref="ISoftDeletedEntity"/>
/// </summary>
public abstract class SoftDeletedBaseEntity : SoftDeletedBaseEntity<int>, ISoftDeletedEntity, IEntity { }
