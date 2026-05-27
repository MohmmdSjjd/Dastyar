/* =========================================================
 * This file is part of the Csis.Template
 * Do not make any modifications on this file
 * All changes will be rejected in code review sessions
 * ========================================================= */

namespace Csis.Template.Domain.Common;

/// <summary>
/// IEntity interface
/// </summary>
public interface IEntity<TKey>
{
    /// <summary>
    /// شناسه موجودیت
    /// </summary>
    TKey Id { get; set; }

    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    DateTime CreatedOn { get; }

    /// <summary>
    /// تاریخ آخرین بروزرسانی
    /// </summary>
    DateTime? UpdatedOn { get; }

    /// <summary>
    /// توضیحات
    /// </summary>
    string Description { get; set; }

    /// <summary>
    /// شناسه کاربر ایجاد کننده
    /// </summary>
    int? CreatedById { get; }

    /// <summary>
    /// شناسه کاربر آخرین بروزرسانی
    /// </summary>
    int? LastUpdatedById { get; }

    /// <summary>
    /// شناسه کاربر ایجاد کننده تفویض دهنده
    /// </summary>
    int? CreatedByDelegatedId { get; }

    /// <summary>
    /// شناسه کاربر  تفویض دهنده آخرین بروزرسانی
    /// </summary>
    int? LastUpdatedByDelegatedId { get; }

    /// <summary>
    /// بروزرسانی موجودیت
    /// </summary>
    /// <param name="userId">شناسه کاربر بروزرسانی کننده</param>
    /// <param name="delegatedUserId">شناسه کاربر تفویض دهنده به بروزرسانی کننده</param>
    /// <param name="updatedOn">تاریخ بروزرسانی</param>
    void Update(int? userId, int? delegatedUserId, DateTime updatedOn);

    /// <summary>
    /// تنظیم شناسه کاربر ایجاد کننده
    /// </summary>
    /// <param name="userId">شناسه کاربر</param>
    /// <param name="delegatedUserId">شناسه کاربر تفویض دهنده به ایجاد کننده</param>
    void SetCreatedById(int? userId, int? delegatedUserId);

    /// <summary>
    /// تنظیم تاریخ ایجاد
    /// </summary>
    /// <param name="createdOn"></param>
    void SetCreatedOn(DateTime createdOn);
}

/// <summary>
/// IEntity interface with int key
/// </summary>
public interface IEntity : IEntity<int> { }
