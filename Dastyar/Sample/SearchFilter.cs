/* =========================================================
 * This file is part of the Csis.Template
 * Do not make any modifications on this file
 * All changes will be rejected in code review sessions
 * ========================================================= */

using Csis.Template.Application.Enums;

namespace Csis.Template.Application.Common;

/// <summary>
/// Search filter
/// </summary>
public sealed record SearchFilter
{
    /// <summary>
    /// Field name
    /// </summary>
    public string Field { get; init; }

    /// <summary>
    /// Search operator
    /// </summary>
    public SearchOperator Operator { get; init; }

    /// <summary>
    /// Search value
    /// </summary>
    public string Value { get; init; }
}
