/* =========================================================
 * This file is part of the Csis.Template
 * Do not make any modifications on this file
 * All changes will be rejected in code review sessions
 * ========================================================= */

namespace Csis.Template.Domain.Common;

/// <summary>
/// Represents a soft-deleted (without actually deleting from storage) entity
/// </summary>
public interface ISoftDeletedEntity<TKey> : IEntity<TKey>
{
    /// <summary>
    /// بیانگر اینکه آیا رکورد حذف منطقی شده است یا خیر
    /// </summary>
    bool Deleted { get; }

    /// <summary>
    /// تاریخ حذف منطقی
    /// </summary>
    DateTime? DeletedOn { get; }

    /// <summary>
    /// شناسه کاربر حذف کننده
    /// </summary>
    int? DeletedById { get; }

    /// <summary>
    /// شناسه کاربر تفویض دهنده به حذف کننده
    /// </summary>
    int? DeletedByDelegatedId { get; }

    /// <summary>
    /// انجام حذف منطقی
    /// </summary>
    /// <param name="deletedById">شناسه کاربر حذف کننده</param>
    /// <param name="deletedByDelegatedUserId">شناسه کاربر تفویض دهنده به حذف کننده</param>
    void SoftDelete(int? deletedById, int? deletedByDelegatedUserId);
}

/// <summary>
/// Represents a soft-deleted (without actually deleting from storage) entity with int key
/// </summary>
public interface ISoftDeletedEntity : ISoftDeletedEntity<int>, IEntity { }
