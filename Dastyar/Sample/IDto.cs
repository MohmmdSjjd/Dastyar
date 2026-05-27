/* =========================================================
 * This file is part of the Csis.Template
 * Do not make any modifications on this file
 * All changes will be rejected in code review sessions
 * ========================================================= */

namespace Csis.Template.Application.Common.Dtos;

/// <summary>
/// Generica interface for DTOs
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface IDto<TKey>
{
    /// <summary>
    /// Identifier
    /// </summary>
    TKey Id { get; set; }
}

/// <summary>
/// Interface for DTOs with int key
/// </summary>
public interface IDto : IDto<int> { }
