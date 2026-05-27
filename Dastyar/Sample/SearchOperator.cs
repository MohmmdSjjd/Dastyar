/* =========================================================
 * This file is part of the Csis.Template
 * Do not make any modifications on this file
 * All changes will be rejected in code review sessions
 * ========================================================= */

namespace Csis.Template.Application.Enums;

/// <summary>
/// Search operator types
/// </summary>
public enum SearchOperator
{
    /// <summary>
    /// Equal
    /// </summary>
    Equal = 0,

    /// <summary>
    /// Greater than
    /// </summary>
    GreaterThan = 1,

    /// <summary>
    /// Greater than or equal
    /// </summary>
    GreaterThanOrEqual = 2,

    /// <summary>
    /// Less than
    /// </summary>
    LessThan = 3,

    /// <summary>
    /// Less than or equal
    /// </summary>
    LessThanOrEqual = 4,

    /// <summary>
    /// Not equal
    /// </summary>
    NotEqual = 5,

    /// <summary>
    /// Contains
    /// </summary>
    Contains = 6,

    /// <summary>
    /// Starts with
    /// </summary>
    StartsWith = 7,

    /// <summary>
    /// Ends with
    /// </summary>
    EndsWith = 8,
}
