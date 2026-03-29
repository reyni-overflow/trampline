using Trampline.Contracts.DTOs.Requests;
using Trampline.Contracts.DTOs.Responses;
using Trampline.Core.Models;
using Trampline.Shared.Results;

namespace Trampline.Application.Services;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, UserAgent agent, CancellationToken cancellationToken);

    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, UserAgent agent, CancellationToken cancellationToken);

    Task<Result<AuthResponse>> RefreshAsync(string refreshToken, UserAgent agent, CancellationToken cancellationToken);

    Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken);

    Task<Result> DeleteAccountAsync(Guid userId, string password, CancellationToken cancellationToken);

    Task<Result<string>> ForgotPasswordAsync(string email, CancellationToken cancellationToken);

    Task<Result> ResetPasswordAsync(string email, string code, string newPassword, CancellationToken cancellationToken);
}