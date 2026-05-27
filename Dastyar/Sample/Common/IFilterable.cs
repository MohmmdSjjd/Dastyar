/* =========================================================
 * This file is part of the Csis.Template
 * Do not make any modifications on this file
 * All changes will be rejected in code review sessions
 * ========================================================= */

namespace Csis.Template.Domain.Common;

/// <summary>
/// موجودیت قابل فیلتر
/// </summary>
public interface IFilterable
{
    /// <summary>
    /// دریافت لیست فیلدهایی که قبلیت فیلترگذاری داینامیک دارند
    /// </summary>
    /// <returns></returns>
    string[] GetFilterableFields();
}
