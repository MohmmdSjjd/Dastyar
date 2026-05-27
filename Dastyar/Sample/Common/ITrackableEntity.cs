/* =========================================================
 * This file is part of the Csis.Template
 * Do not make any modifications on this file
 * All changes will be rejected in code review sessions
 * ========================================================= */

namespace Csis.Template.Domain.Common;

/// <summary>
/// موجودیت دارای کد رهگیری
/// </summary>
public interface ITrackableEntity
{
    /// <summary>
    /// کد رهگیری
    /// </summary>
    string TrackingCode { get; set; }
}
