using BCrypt.Net;
using DocumentFormat.OpenXml.Math;
using Inspector.Data;
using Inspector.Models;
using Microsoft.EntityFrameworkCore;

namespace Inspector.Services;

public interface IAuthService
{
    Task<User?> AuthenticateAsync(string login, string password);
    Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
}

public sealed class AuthService : IAuthService
{
    private readonly IDbContextFactory<InspectorDbContext> _dbFactory;

    public AuthService(IDbContextFactory<InspectorDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<User?> AuthenticateAsync(string login, string password)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Login == login);

        if (user == null)
            return null;
        if (string.IsNullOrEmpty(user.PasswordHash))
            return user;
        if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return user;

        return null;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var user = await db.Users.FindAsync(userId);
        if (user == null) return false;

        if (!string.IsNullOrEmpty(user.PasswordHash))
        {
            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
                return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await db.SaveChangesAsync();
        return true;
    }
}