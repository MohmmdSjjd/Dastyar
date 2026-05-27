using Dastyar.Domain.Common;

namespace Dastyar.Domain.Entities;

public sealed class AppUser : BaseEntity<Guid>, IFilterable
{
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public string[] GetFilterableFields() => new[]
    {
        "UserName",
        "DisplayName",
        "IsActive",
        "CreatedAtUtc",
    };
}
