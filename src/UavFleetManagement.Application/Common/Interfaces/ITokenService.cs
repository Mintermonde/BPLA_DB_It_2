namespace UavFleetManagement.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateToken(int id, string username, string role);
}
