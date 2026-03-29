using System.ComponentModel.DataAnnotations;
using Trampline.Core.Exceptions;
using Trampline.Core.Models.Employee;
using Trampline.Core.Models.Worker;
using Trampline.Shared.Results;

namespace Trampline.Core.Models;

public class User
{
    [Key]
    public Guid Id { get; private set; }

    public string Nickname { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public Role Role { get; private set; } = Role.Worker;

    public string? Phone { get; private set; }

    public string? Avatar { get; private set; }

    public bool IsPrivate { get; private set; } = false;

    public bool IsBlocked { get; private set; } = false;

    public string? TotpSecret { get; private set; }

    public bool IsTotpEnabled { get; private set; } = false;

    public bool IsSuperAdmin { get; private set; } = false;

    public DateTime? DeletedAt { get; private set; }

    public ICollection<UserSession> Sessions { get; private set; } = new List<UserSession>();

    public IEnumerable<UserSession> GetActiveSessions() =>
        Sessions?.Where(s => s.RevokedAt == null && s.ExpiresAt > DateTime.UtcNow) ?? Enumerable.Empty<UserSession>();

    public WorkerProfile? WorkerProfile { get; private set; }

    public EmployeeProfile? EmployeeProfile { get; private set; }

    private User() { }

    public static Result<User> Create(string email, string nickname, string password, Role role)
    {
        var errors = new List<ErrorDetail>();

        if (string.IsNullOrWhiteSpace(email))
            errors.Add(new ErrorDetail("email", "Email обязателен"));

        if (!email.Contains('@'))
            errors.Add(new ErrorDetail("email", "Некорректный формат email"));

        if (nickname.Length < 2)
            errors.Add(new ErrorDetail("name", "Имя минимум 2 символа"));

        if (errors.Count > 0)
            return Result<User>.Failure(errors.ToArray());

        User user = new()
        {
            Id = Guid.NewGuid(),
            Nickname = nickname.Trim(),
            Email = email.Trim(),
            PasswordHash = PasswordHasher.Hash(password),
            Role = role
        };

        return Result<User>.Success(user);
    }

    public static User CreateSeed(Guid id, string email, string nickname, string passwordHash, Role role, bool isSuperAdmin = false)
    {
        return new User
        {
            Id = id,
            Email = email,
            Nickname = nickname,
            PasswordHash = passwordHash,
            Role = role,
            IsSuperAdmin = isSuperAdmin
        };
    }

    public void SetWorkerProfile(WorkerProfile workerProfile)
    {
        WorkerProfile = workerProfile;
    }

    public void SetEmployeeProfile(EmployeeProfile employeeProfile)
    {
        EmployeeProfile = employeeProfile;
    }

    public void AddSession(UserSession session)
    {
        if (session.UserId != this.Id)
            throw new DomainException("Session belongs to another user");

        Sessions.Add(session);
    }

    public void SetAvatar(string photo)
    {
        Avatar = photo;
    }

    public void RemoveSession(UserSession session)
    {
        Sessions.Remove(session);
    }

    public void RevokeAllSessions()
    {
        foreach (var session in Sessions)
        {
            session.Revoke("PasswordChanged");
        }
    }

    public void SetPhone(string? phone) => Phone = phone;

    public void ChangePrivate() => IsPrivate = !IsPrivate;

    public void SetPrivate(bool isPrivate) => IsPrivate = isPrivate;

    public void Block() => IsBlocked = true;

    public void Unblock() => IsBlocked = false;

    public void ChangeRole(Role role) => Role = role;

    public void ChangePassword(string newPasswordHash) => PasswordHash = newPasswordHash;

    public void SetTotpSecret(string? secret) => TotpSecret = secret;

    public void EnableTotp(string secret)
    {
        TotpSecret = secret;
        IsTotpEnabled = true;
    }

    public void DisableTotp()
    {
        TotpSecret = null;
        IsTotpEnabled = false;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        IsBlocked = true;
        Phone = null;
        Email = $"deleted_{Id:N}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}@deleted.local";
        Nickname = "Удалённый пользователь";
    }
}