/* =========================================================
 * This file is part of the Csis.Template
 * Do not make any modifications on this file
 * All changes will be rejected in code review sessions
 * ========================================================= */

namespace Csis.Template.Application.Common.Mappings;

/// <summary>
/// Register auto mappings for <see cref="IMappable"/> types
/// </summary>
public sealed class AutoMappingProfile : Profile
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="mappables"></param>
    public AutoMappingProfile(IEnumerable<IMappable> mappables) {
        foreach ( var item in mappables ) {
            item.CreateMappings(this);
        }
    }
}
