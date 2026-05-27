/* =========================================================
 * This file is part of the Csis.Template
 * Do not make any modifications on this file
 * All changes will be rejected in code review sessions
 * ========================================================= */

using System.Text.Json.Serialization;

namespace Csis.Template.Domain.Common;

/// <summary>
/// Abstract implementation of <see cref="IEntity{TKey}"/>
/// </summary>
public abstract class BaseEntity<TKey> : IEntity<TKey>
{
    /// <inheritdoc/>
    public TKey Id { get; set; }

    /// <inheritdoc/>
    public string Description { get; set; }

    /// <inheritdoc/>
    [JsonInclude]
    public DateTime CreatedOn { get; private set; }

    /// <inheritdoc/>
    [JsonInclude]
    public DateTime? UpdatedOn { get; private set; }

    /// <inheritdoc/>
    [JsonInclude]
    public int? CreatedById { get; private set; }

    /// <inheritdoc/>
    [JsonInclude]
    public int? LastUpdatedById { get; private set; }

    /// <inheritdoc/>
    [JsonInclude]
    public int? CreatedByDelegatedId { get; private set; }

    /// <inheritdoc/>
    [JsonInclude]
    public int? LastUpdatedByDelegatedId { get; private set; }

    /// <inheritdoc/>
    public virtual void Update(int? userId, int? delegatedUserId, DateTime updatedOn) {
        UpdatedOn = updatedOn;
        LastUpdatedById = userId;
        LastUpdatedByDelegatedId = delegatedUserId;
    }

    /// <inheritdoc/>
    public void SetCreatedById(int? id, int? delegatedUserId) {
        CreatedById = id;
        CreatedByDelegatedId = delegatedUserId;
    }

    /// <inheritdoc/>
    public void SetCreatedOn(DateTime createdOn) {
        CreatedOn = createdOn;
    }
}

/// <summary>
/// Abstract implementation of <see cref="IEntity"/>
/// </summary>
public abstract class BaseEntity : BaseEntity<int>, IEntity { }
