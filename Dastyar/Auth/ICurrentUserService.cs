using System.Security.Claims;

namespace Dastyar.Auth;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
    IReadOnlyList<string> Roles { get; }
    IEnumerable<Claim> Claims { get; }
}
