using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NouFlix.DTOs;
using NouFlix.Helpers;
using NouFlix.Mapper;
using NouFlix.Models.Common;
using NouFlix.Models.Entities;
using NouFlix.Models.ValueObject;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Services;

public class AuthService(
    IConfiguration configuration,
    MinioObjectStorage storage,
    IUnitOfWork uow
    )
{
    public async Task<UserDto.UserRes> GetCurrentUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await uow.Users.FindAsync(userId)
                   ?? throw new KeyNotFoundException("User không tồn tại.");
        return await user.ToUserResAsync(storage, ct);
    }

    public async Task RegisterAsync(RegisterReq req, CancellationToken ct = default)
    {
        if (await uow.Users.EmailExistsAsync(req.Email))
            throw new EmailAlreadyUsedException("Email đã được sử dụng.");

        var hashed = AuthHelper.HashPassword(req.Password);
        var user = new User
        {
            Email = Email.Create(req.Email),
            Password = hashed,
        };
        await uow.Users.AddAsync(user, ct);
        await uow.SaveChangesAsync(ct);
    }
    
    public async Task<AuthRes> LoginAsync(LoginReq req, CancellationToken ct = default)
    {
        var user = await uow.Users.GetByEmailAsync(req.Email)
                   ?? throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");
        if (!AuthHelper.VerifyPassword(req.Password, user.Password))
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email.ToString()),
            new Claim(ClaimTypes.Role, user.MapRole())
        };
        
        var accessExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30);
        var accessToken = GenerateAccessToken(claims);
        var refreshToken = GenerateRefreshToken();

        var rt = new RefreshToken
        {
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            UserId = user.Id
        };
        await uow.Refreshes.AddAsync(rt, ct);
        await uow.SaveChangesAsync(ct);
        
        var userDto = await user.ToUserResAsync(storage, ct);

        return new AuthRes(
            accessToken,
            refreshToken,
            accessExpiresAt,
            userDto);
    }
    
    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["JWT:Issuer"],
            audience: configuration["JWT:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string GenerateRefreshToken()
    {
        var randomNumber = new Byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task<string?> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var existingToken = await uow.Refreshes.GetByTokenAsync(refreshToken);
        if (existingToken is null || existingToken.ExpiresAt < DateTime.Now || existingToken.IsRevoked)
            return null;

        // existingToken.ExpiresAt = existingToken.ExpiresAt.AddMinutes(-1);
        
        var user = existingToken.User;
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, user.Email.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.MapRole())
        };
        
        var newAccessToken = GenerateAccessToken(claims);
        await uow.SaveChangesAsync(ct);
        
        return newAccessToken;
    }

    public async Task<(string AccessToken, string RefreshToken, DateTimeOffset AccessExpiresAt)> IssueTokensForUserAsync(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email.ToString()),
            new Claim(ClaimTypes.Role, user.MapRole())
        };

        var accessExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30);
        var accessToken = GenerateAccessToken(claims);
        var refreshToken = GenerateRefreshToken();

        var rt = new RefreshToken
        {
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            UserId = user.Id
        };
        await uow.Refreshes.AddAsync(rt);
        await uow.SaveChangesAsync();

        return (accessToken, refreshToken, accessExpiresAt);
    }
}