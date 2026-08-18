using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReSellHub.Api.Data;
using ReSellHub.Api.Models;

namespace ReSellHub.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthApiController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<AppUser> _hasher = new();
    public AuthApiController(AppDbContext context) => _context = context;

    [HttpGet("me")]
    public IActionResult Me() => User.Identity?.IsAuthenticated == true
        ? Ok(new { isAuthenticated = true, id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!), name = User.Identity.Name, email = User.FindFirstValue(ClaimTypes.Email) })
        : Ok(new { isAuthenticated = false });

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (request.Password.Length < 8) return BadRequest(new { message = "密碼至少需要 8 個字元。" });
        if (await _context.Users.AnyAsync(x => x.Email == email)) return Conflict(new { message = "此 Email 已註冊。" });

        var user = new AppUser { DisplayName = request.DisplayName.Trim(), Email = email, Phone = request.Phone.Trim(), Role = "Buyer" };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        _context.ShoppingCarts.Add(new ShoppingCart { UserId = user.Id, UpdatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();
        await SignInAsync(user);
        return Ok(new { isAuthenticated = true, user.Id, name = user.DisplayName, user.Email });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email && x.IsActive);
        if (user is null || _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Email 或密碼錯誤。" });
        await SignInAsync(user);
        return Ok(new { isAuthenticated = true, user.Id, name = user.DisplayName, user.Email });
    }

    [Authorize, HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { message = "已登出。" });
    }

    private async Task SignInAsync(AppUser user)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.DisplayName), new Claim(ClaimTypes.Email, user.Email), new Claim(ClaimTypes.Role, user.Role) };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)), new AuthenticationProperties { IsPersistent = true });
    }
}

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string DisplayName, string Email, string Phone, string Password);
