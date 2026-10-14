using Microsoft.EntityFrameworkCore;
using UavFleetManagement.Application.Common.Exceptions;
using UavFleetManagement.Application.Common.Interfaces;
using UavFleetManagement.Application.DTOs.Auth;
using UavFleetManagement.Domain.Entities.Identity;

namespace UavFleetManagement.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokenService;

    public AuthService(IApplicationDbContext context, IPasswordHasher hasher, ITokenService tokenService)
    {
        _context = context;
        _hasher = hasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> RegisterUserAsync(RegisterRequestDto request)
    {
        if (await _context.UserCredentials.AnyAsync(u => u.Username == request.Username))
        {
            throw new BadRequestException("Пользователь с таким именем уже существует.");
        }

        var (hash, salt) = _hasher.HashPassword(request.Password);
        var user = new UserCredential
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            LinkedUserId = request.LinkedUserId,
            LinkedEngineerId = request.LinkedEngineerId,
            CreatedAt = DateTime.UtcNow
        };

        _context.UserCredentials.Add(user);
        await _context.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user.UserCredentialId, user.Username, "User");
        return new AuthResponseDto(token, user.Username, "User", DateTime.UtcNow.AddHours(2));
    }

    public async Task<AuthResponseDto> LoginUserAsync(LoginRequestDto request)
    {
        var user = await _context.UserCredentials.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user == null || !_hasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            throw new UnauthorizedException("Неверный логин или пароль.");
        }

        if (user.IsBlocked)
        {
            throw new ForbiddenException("Учетная запись заблокирована.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user.UserCredentialId, user.Username, "User");
        return new AuthResponseDto(token, user.Username, "User", DateTime.UtcNow.AddHours(2));
    }

    public async Task<AuthResponseDto> RegisterSuperuserAsync(RegisterRequestDto request)
    {
        if (await _context.SuperuserCredentials.AnyAsync(u => u.Username == request.Username))
        {
            throw new BadRequestException("Суперпользователь с таким именем уже существует.");
        }

        var (hash, salt) = _hasher.HashPassword(request.Password);
        var superuser = new SuperuserCredential
        {
            Username = request.Username,
            PasswordHash = hash,
            PasswordSalt = salt,
            SecurityStamp = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow
        };

        _context.SuperuserCredentials.Add(superuser);
        await _context.SaveChangesAsync();

        var token = _tokenService.GenerateToken(superuser.SuperuserId, superuser.Username, "Superuser");
        return new AuthResponseDto(token, superuser.Username, "Superuser", DateTime.UtcNow.AddHours(2));
    }

    public async Task<AuthResponseDto> LoginSuperuserAsync(LoginRequestDto request)
    {
        var superuser = await _context.SuperuserCredentials.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (superuser == null || !_hasher.VerifyPassword(request.Password, superuser.PasswordHash, superuser.PasswordSalt))
        {
            throw new UnauthorizedException("Неверные учетные данные администратора.");
        }

        superuser.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var token = _tokenService.GenerateToken(superuser.SuperuserId, superuser.Username, "Superuser");
        return new AuthResponseDto(token, superuser.Username, "Superuser", DateTime.UtcNow.AddHours(2));
    }
}
