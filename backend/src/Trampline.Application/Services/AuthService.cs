using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Contracts.DTOs.Responses;
using Trampline.Core.Models;
using Trampline.Core.Storage;
using Trampline.Shared.Results;

namespace Trampline.Application.Services;

public class AuthService(
    IUserService userService,
    ILogger<AuthService> logger,
    ITokenService tokenService,
    IDistributedCache cache,
    IHostEnvironment env,
    IEmailService emailService,
    IStorageService storage) : IAuthService
{
    private static readonly string DummyHash = PasswordHasher.Hash("timing_equalization_dummy");

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, UserAgent agent, CancellationToken cancellationToken)
    {
        if (request.Role == Role.Admin)
            request.Role = Role.Worker;

        var userResult = await userService.CreateUserAsync(request, cancellationToken);

        if (userResult.IsFailure)
        {
            logger.LogWarning("User registration failed for {Email}", request.Email);
            return Result<AuthResponse>.Failure(userResult.Errors.ToArray());
        }

        var accessToken = tokenService.GenerateJwtToken(userResult.Value!);
        var refreshToken = await tokenService.GenerateToken(userResult.Value!, agent, cancellationToken);

        logger.LogInformation("User registered {UserId} with role {Role}", userResult.Value!.Id, userResult.Value!.Role);

        try
        {
            var codeBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(4);
            var code = (Math.Abs(BitConverter.ToInt32(codeBytes)) % 900000 + 100000).ToString();

            var cacheKey = $"email_verify:{request.Email.ToLowerInvariant()}";
            await cache.SetStringAsync(cacheKey, code,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) },
                cancellationToken);

            await emailService.SendVerificationCodeAsync(request.Email, code, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send verification email to {Email}", request.Email);
        }

        return Result<AuthResponse>.Success(new AuthResponse()
        {
            Id = userResult.Value!.Id,
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token
        });
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, UserAgent agent, CancellationToken cancellationToken)
    {
        var contact = request.Contact.Trim();
        var isPhone = contact.StartsWith("+") || (contact.Length >= 10 && contact.All(char.IsDigit));

        var attemptsKey = $"login_attempts:{contact.ToLowerInvariant()}";
        var attemptsStr = await cache.GetStringAsync(attemptsKey, cancellationToken);
        var attempts = int.TryParse(attemptsStr, out var a) ? a : 0;

        if (attempts >= 10)
        {
            logger.LogWarning("Login locked due to too many attempts for {Contact}", contact);
            return Result<AuthResponse>.Failure(new ErrorDetail(nameof(request.Contact),
                "Too many failed login attempts. Try again in 15 minutes.", 429));
        }

        User? find;
        if (isPhone)
            find = await userService.GetByPhoneAsync(contact, cancellationToken);
        else
            find = await userService.GetByEmailAsync(contact, cancellationToken);

        if (find == null)
        {
            PasswordHasher.Verify(request.Password, DummyHash);
            await cache.SetStringAsync(attemptsKey, (attempts + 1).ToString(),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) }, cancellationToken);
            logger.LogWarning("Failed login attempt for {Contact}, attempt {Attempt}", contact, attempts + 1);
            return Result<AuthResponse>.Failure(new ErrorDetail("credentials", "Invalid credentials", 401));
        }

        var isPassword = PasswordHasher.Verify(request.Password, find.PasswordHash);

        if (!isPassword)
        {
            await cache.SetStringAsync(attemptsKey, (attempts + 1).ToString(),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) }, cancellationToken);
            logger.LogWarning("Failed login attempt for {Contact}, attempt {Attempt}", contact, attempts + 1);
            return Result<AuthResponse>.Failure(new ErrorDetail("credentials", "Invalid credentials", 401));
        }

        if (find.IsBlocked)
        {
            logger.LogWarning("Blocked user login attempt for {Contact}", contact);
            return Result<AuthResponse>.Failure(new ErrorDetail(nameof(request.Contact), "Account is blocked", 403));
        }

        await cache.RemoveAsync(attemptsKey, cancellationToken);

        var accessToken = tokenService.GenerateJwtToken(find);
        var refreshToken = await tokenService.GenerateToken(find, agent, cancellationToken);

        logger.LogInformation("User logged in {UserId}", find.Id);

        return Result<AuthResponse>.Success(new AuthResponse()
        {
            Id = find.Id,
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token
        });
    }

    public async Task<Result<AuthResponse>> RefreshAsync(string refreshToken, UserAgent agent, CancellationToken cancellationToken)
    {
        var find = await tokenService.ValidateToken(refreshToken, cancellationToken);

        if (find.IsFailure)
        {
            logger.LogWarning("Refresh attempted with invalid token");
            return Result<AuthResponse>.Failure(new ErrorDetail("refresh token", "token is invalid", 401));
        }

        var findUser = await tokenService.GetUserFromToken(find.Value!.Token, cancellationToken);

        if (findUser.IsFailure)
        {
            logger.LogWarning("Refresh token valid but user not found");
            return Result<AuthResponse>.Failure(new ErrorDetail("refresh token", "user not found", 401));
        }

        if (findUser.Value!.IsBlocked)
        {
            logger.LogWarning("Blocked user refresh attempt for {UserId}", findUser.Value!.Id);
            return Result<AuthResponse>.Failure(new ErrorDetail("refresh token", "Account is blocked", 403));
        }

        await tokenService.DisableTokenAsync(find.Value!.Token, cancellationToken);

        var newRefresh = await tokenService.GenerateToken(findUser.Value!, agent, cancellationToken);
        var accessToken = tokenService.GenerateJwtToken(findUser.Value!);

        logger.LogDebug("Token refreshed for {UserId}, old token revoked", findUser.Value!.Id);

        return Result<AuthResponse>.Success(new AuthResponse()
        {
            Id = findUser.Value!.Id,
            AccessToken = accessToken,
            RefreshToken = newRefresh.Token
        });
    }

    public async Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            return Result.Failure(new ErrorDetail("user", "Пользователь не найден", 404));

        if (!PasswordHasher.Verify(currentPassword, user.PasswordHash))
        {
            logger.LogWarning("Wrong current password for {UserId}", userId);
            return Result.Failure(new ErrorDetail("currentPassword", "Неверный текущий пароль", 400));
        }

        if (newPassword.Length < 8)
            return Result.Failure(new ErrorDetail("newPassword", "Пароль должен быть минимум 8 символов", 400));

        user.ChangePassword(PasswordHasher.Hash(newPassword));
        user.RevokeAllSessions();
        await userService.UpdateAsync(user, cancellationToken);

        logger.LogInformation("Password changed for {UserId}", userId);
        return Result.Success();
    }

    public async Task<Result> DeleteAccountAsync(Guid userId, string password, CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            return Result.Failure(new ErrorDetail("user", "Пользователь не найден", 404));

        if (!PasswordHasher.Verify(password, user.PasswordHash))
        {
            logger.LogWarning("Account deletion failed, wrong password for {UserId}", userId);
            return Result.Failure(new ErrorDetail("password", "Неверный пароль", 400));
        }

        try
        {
            var filePaths = new List<string>();

            if (user.Avatar != null)
                filePaths.Add(user.Avatar);

            if (user.WorkerProfile != null)
            {
                if (user.WorkerProfile.Resume != null)
                    filePaths.Add(user.WorkerProfile.Resume);
                if (user.WorkerProfile.Photo != null)
                    filePaths.Add(user.WorkerProfile.Photo);
            }

            if (user.EmployeeProfile != null)
            {
                filePaths.AddRange(user.EmployeeProfile.Photos);
                filePaths.AddRange(user.EmployeeProfile.Videos);
            }

            foreach (var filePath in filePaths)
            {
                await storage.DeleteAsync(filePath, cancellationToken);
                logger.LogInformation("Deleted file {Path} for user {UserId}", filePath, userId);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to clean up files for user {UserId}", userId);
        }

        user.SoftDelete();
        user.RevokeAllSessions();
        await userService.UpdateAsync(user, cancellationToken);
        logger.LogInformation("Account soft-deleted {UserId}", userId);
        return Result.Success();
    }

    public async Task<Result<string>> ForgotPasswordAsync(string email, CancellationToken cancellationToken)
    {
        var user = await userService.GetByEmailAsync(email, cancellationToken);

        logger.LogInformation("Password reset requested for {Email}", email);

        var codeBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(4);
        var code = (Math.Abs(BitConverter.ToInt32(codeBytes)) % 900000 + 100000).ToString();

        if (user != null)
        {
            var cacheKey = $"password_reset:{email}";
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
            };
            await cache.SetStringAsync(cacheKey, code, options, cancellationToken);
            await emailService.SendPasswordResetCodeAsync(email, code, cancellationToken);
        }

        if (!emailService.IsConfigured)
            return Result<string>.Success(code);

        return Result<string>.Success("Код отправлен на указанную почту");
    }

    public async Task<Result> ResetPasswordAsync(string email, string code, string newPassword, CancellationToken cancellationToken)
    {
        var attemptsKey = $"password_reset_attempts:{email}";
        var attemptsStr = await cache.GetStringAsync(attemptsKey, cancellationToken);
        var attempts = int.TryParse(attemptsStr, out var a) ? a : 0;

        if (attempts >= 5)
        {
            logger.LogWarning("Too many reset attempts for {Email}", email);
            return Result.Failure(new ErrorDetail("code", "Too many attempts. Please request a new code.", 429));
        }

        var cacheKey = $"password_reset:{email}";
        var storedCode = await cache.GetStringAsync(cacheKey, cancellationToken);

        if (string.IsNullOrEmpty(storedCode) || storedCode != code)
        {
            await cache.SetStringAsync(attemptsKey, (attempts + 1).ToString(),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) }, cancellationToken);
            logger.LogWarning("Invalid reset code for {Email}, attempt {Attempt}", email, attempts + 1);
            return Result.Failure(new ErrorDetail("code", "Invalid or expired reset code", 400));
        }

        if (newPassword.Length < 8)
            return Result.Failure(new ErrorDetail("newPassword", "Пароль должен быть минимум 8 символов", 400));

        var user = await userService.GetByEmailAsync(email, cancellationToken);
        if (user == null)
            return Result.Failure(new ErrorDetail("email", "Invalid or expired reset code", 400));

        user.ChangePassword(PasswordHasher.Hash(newPassword));
        user.RevokeAllSessions();
        await userService.UpdateAsync(user, cancellationToken);

        await cache.RemoveAsync(cacheKey, cancellationToken);
        await cache.RemoveAsync(attemptsKey, cancellationToken);

        logger.LogInformation("Password reset completed for {Email}", email);
        return Result.Success();
    }

    public async Task<Result<string>> SendVerificationCodeAsync(string email, CancellationToken cancellationToken)
    {
        var codeBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(4);
        var code = (Math.Abs(BitConverter.ToInt32(codeBytes)) % 900000 + 100000).ToString();

        var cacheKey = $"email_verify:{email.ToLowerInvariant()}";
        await cache.SetStringAsync(cacheKey, code,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) },
            cancellationToken);

        await emailService.SendVerificationCodeAsync(email, code, cancellationToken);
        logger.LogInformation("Verification code sent to {Email}", email);

        if (!emailService.IsConfigured)
            return Result<string>.Success(code);

        return Result<string>.Success("Код отправлен на указанную почту");
    }

    public async Task<Result> VerifyEmailAsync(string email, string code, CancellationToken cancellationToken)
    {
        var attemptsKey = $"email_verify_attempts:{email.ToLowerInvariant()}";
        var attemptsStr = await cache.GetStringAsync(attemptsKey, cancellationToken);
        var attempts = int.TryParse(attemptsStr, out var a) ? a : 0;

        if (attempts >= 5)
        {
            logger.LogWarning("Too many email verification attempts for {Email}", email);
            return Result.Failure(new ErrorDetail("code", "Too many attempts. Please request a new code.", 429));
        }

        var cacheKey = $"email_verify:{email.ToLowerInvariant()}";
        var storedCode = await cache.GetStringAsync(cacheKey, cancellationToken);

        if (string.IsNullOrEmpty(storedCode) || storedCode != code)
        {
            await cache.SetStringAsync(attemptsKey, (attempts + 1).ToString(),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) },
                cancellationToken);
            return Result.Failure(new ErrorDetail("code", "Invalid or expired verification code", 400));
        }

        await cache.RemoveAsync(cacheKey, cancellationToken);
        await cache.RemoveAsync(attemptsKey, cancellationToken);

        logger.LogInformation("Email verified for {Email}", email);
        return Result.Success();
    }
}