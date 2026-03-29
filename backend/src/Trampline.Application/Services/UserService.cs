using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Core.Models;
using Trampline.Core.Models.Worker;
using Trampline.Core.Repositories;
using Trampline.Shared.Results;

namespace Trampline.Application.Services;

public class UserService(
    IUserRepository userRepository,
    ILogger<UserService> logger) : IUserService
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await userRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetByEmail for {Email}", email);
        return await userRepository.GetByEmailAsync(email, cancellationToken);
    }

    public async Task<User?> GetByPhoneAsync(string phone, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetByPhone for {Phone}", phone);
        return await userRepository.GetByPhoneAsync(phone, cancellationToken);
    }

    public async Task<T?> GetMeAsync<T>(Guid id, Expression<Func<User, T>> expression, CancellationToken cancellationToken)
    {
        return await userRepository.GetMeAsync(id, expression, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await userRepository.AddAsync(user, cancellationToken);
    }

    public async Task<Result<User>> CreateUserAsync(RegisterRequest request, CancellationToken ct)
    {
        var findUser = await userRepository.GetByEmailAsync(request.Email, ct);

        if (findUser != null)
            return Result<User>.Failure(new ErrorDetail("email", "Email уже занят"));

        var newUser = User.Create(request.Email, request.Name, request.Password, request.Role);

        if (newUser.IsFailure)
            return Result<User>.Failure(newUser.Errors.ToArray());

        if (request.Role == Role.Worker)
        {
            var parts = request.Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var (firstName, lastName, patronymic) = parts.Length switch
            {
                1 => (parts[0], "", ""),
                2 => (parts[0], parts[1], ""),
                _ => (parts[1], parts[0], string.Join(" ", parts.Skip(2)))
            };

            var profile = WorkerProfile.Create(
                newUser.Value!.Id, firstName, lastName, patronymic, null, null, null);
            newUser.Value!.SetWorkerProfile(profile);
        }

        await userRepository.AddAsync(newUser.Value!, ct);
        logger.LogInformation("User created {UserId} ({Email}, role: {Role})", newUser.Value!.Id, newUser.Value!.Email, newUser.Value!.Role);
        return Result<User>.Success(newUser.Value!);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken)
    {
        await userRepository.UpdateAsync(user, cancellationToken);
        logger.LogDebug("User updated {UserId}", user.Id);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await userRepository.DeleteAsync(id, cancellationToken);
        logger.LogInformation("User deleted {UserId}", id);
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await userRepository.GetAllAsync(cancellationToken);
    }

    public async Task<(IEnumerable<User> Items, int Total)> GetPaginatedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        return await userRepository.GetPaginatedAsync(pageNumber, pageSize, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(Role role, CancellationToken cancellationToken)
    {
        return await userRepository.GetByRoleAsync(role, cancellationToken);
    }

    public async Task<IDictionary<Guid, User>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        return await userRepository.GetByIdsAsync(ids, cancellationToken);
    }
}