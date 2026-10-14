namespace UavFleetManagement.Application.DTOs.Auth;

public record LoginRequestDto(string Username, string Password);

public record RegisterRequestDto(
    string Username,
    string Email,
    string Password,
    int? LinkedUserId = null,
    int? LinkedEngineerId = null
);

public record AuthResponseDto(
    string Token,
    string Username,
    string Role,
    DateTime ExpiresAt
);
