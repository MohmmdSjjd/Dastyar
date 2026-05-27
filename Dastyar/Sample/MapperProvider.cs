/* =========================================================
 * This file is part of the Csis.Template
 * Do not make any modifications on this file
 * All changes will be rejected in code review sessions
 * ========================================================= */

namespace Csis.Template.Application.Common;

/// <summary>
/// Provide static instance of <see cref="IMapper"/>
/// </summary>
public static class MapperProvider
{
    /// <summary>
    /// Get instance of <see cref="IMapper"/>
    /// </summary>
    public static IMapper Mapper { get; private set; }

    /// <summary>
    /// Get mapping configuration provider
    /// </summary>
    public static IConfigurationProvider MapperConfiguration => Mapper.ConfigurationProvider;

    /// <summary>
    /// Initialize mapper instance
    /// </summary>
    /// <param name="mapper">Mapper instance</param>
    /// <exception cref="Exception"></exception>
    public static void Initialize(IMapper mapper) {
        Mapper = mapper;
    }
}
