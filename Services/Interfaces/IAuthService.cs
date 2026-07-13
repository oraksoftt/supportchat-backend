using Microsoft.AspNetCore.Identity.Data;
using SupportChat.Backend.Endpoints;
using SupportChat.Backend.Models.Requests;
using SupportChat.Backend.Models.Responses;
using SupportChat.Backend.Models.Domain;

namespace SupportChat.Backend.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(Models.Requests.LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task LogoutAsync(string refreshToken);
    Task<long> RegisterUserAsync(Models.Requests.RegisterRequest request);
    Task SetOnlineStatusAsync(long userId, bool isOnline);
    Task<bool> ValidateUserAsync(long userId);
    Task<User?> GetUserByIdAsync(long userId);
}

 