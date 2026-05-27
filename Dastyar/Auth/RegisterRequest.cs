namespace Dastyar.Auth;

public sealed record RegisterRequest(string UserName, string Password, string? DisplayName);
