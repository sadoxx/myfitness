namespace MyFit.Application.Common.Interfaces;

/// <summary>
/// JWT Token Service for authentication
/// </summary>
public interface ITokenService
{
    string GenerateToken(Guid userId, string email, string? firstName);
    bool ValidateToken(string token);
}
