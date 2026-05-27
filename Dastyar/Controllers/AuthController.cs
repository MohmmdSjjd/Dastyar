using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dastyar.Auth;
using Dastyar.Domain.Entities;
using Dastyar.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Dastyar.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ApplicationDbContext db, JwtOptions jwtOptions) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var userName = request.UserName.Trim();
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { error = "نام کاربری و رمز عبور الزامی است." });
        }

        if (request.Password.Length < 6)
        {
            return BadRequest(new { error = "رمز عبور باید حداقل ۶ کاراکتر باشد." });
        }

        var exists = await db.Set<AppUser>()
            .AnyAsync(x => x.UserName == userName, cancellationToken);

        if (exists)
        {
            return BadRequest(new { error = "این نام کاربری قبلاً ثبت شده است." });
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName)
                ? userName
                : request.DisplayName.Trim(),
            PasswordHash = PasswordHasher.Hash(request.Password),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        await db.Set<AppUser>().AddAsync(user, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return Ok(CreateToken(user));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var userName = request.UserName.Trim();
        var user = await db.Set<AppUser>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserName == userName, cancellationToken);

        if (user is null || !user.IsActive || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { error = "نام کاربری یا رمز عبور نادرست است." });
        }

        return Ok(CreateToken(user));
    }

    private object CreateToken(AppUser user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(jwtOptions.ExpiryMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims:
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("display_name", user.DisplayName),
                new Claim(ClaimTypes.Role, "User")
            ],
            expires: expiresAt,
            signingCredentials: credentials);

        return new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt,
            user = new
            {
                user.Id,
                user.UserName,
                user.DisplayName
            }
        };
    }
}
