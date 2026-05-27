/* =========================================================
 * This file is part of the Csis.Template
 * Do not make any modifications on this file
 * All changes will be rejected in code review sessions
 * ========================================================= */

namespace Csis.Template.Application.Common.Interfaces;

/// <summary>
/// Marker interface for mappable classes
/// </summary>
public interface IMappable
{
    /// <summary>
    /// Create automapper mapping profile
    /// </summary>
    /// <param name="profile"></param>
    void CreateMappings(Profile profile);
}
