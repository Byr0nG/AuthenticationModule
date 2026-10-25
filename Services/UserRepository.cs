using AuthenticationModule.Models;
using AuthenticationModule.Security;
using Dapper;
using Npgsql;

namespace AuthenticationModule.Services
{
    public interface IUserRepository
    {
        Task<User> GetUserByUsernameAsync(string username);
        Task<User> GetUserByEmailAsync(string email);
        Task<bool> CreateUserAsync(User user, string password);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> UpdateLoginAttemptsAsync(int userId, bool success);
        Task<bool> RecordLoginAsync(int userId);
    }

    public class UserRepository : IUserRepository
    {
        private readonly string _cs;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(IConfiguration config, ILogger<UserRepository> logger)
        {
            _cs = config.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        private NpgsqlConnection GetConnection() => new NpgsqlConnection(_cs);

        private async Task<User> GetOneAsync(string where, object param)
        {
            var sql = $@"SELECT u.*, r.* FROM users u
                         LEFT JOIN roles r ON u.role_id = r.id
                         WHERE {where} AND u.is_active = true";
            using var c = GetConnection();
            var res = await c.QueryAsync<User, Role, User>(sql, (u, r) => { u.Role = r; return u; }, param, splitOn: "id");
            return res.FirstOrDefault();
        }

        public Task<User> GetUserByUsernameAsync(string username) => GetOneAsync("u.username = @Username", new { Username = username });
        public Task<User> GetUserByEmailAsync(string email) => GetOneAsync("u.email = @Email", new { Email = email });

        public async Task<bool> CreateUserAsync(User user, string password)
        {
            var (hash, salt) = PasswordHasher.HashPassword(password);
            const string sql = @"INSERT INTO users (username, email, password_hash, password_salt, role_id, created_at, is_active, failed_login_attempts)
                                 VALUES (@Username, @Email, @PasswordHash, @PasswordSalt, @RoleId, @CreatedAt, @IsActive, @FailedLoginAttempts)";
            try
            {
                using var c = GetConnection();
                return await c.ExecuteAsync(sql, new
                {
                    user.Username,
                    user.Email,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    user.RoleId,
                    user.CreatedAt,
                    user.IsActive,
                    user.FailedLoginAttempts
                }) > 0;
            }
            catch (NpgsqlException ex) { _logger.LogError(ex, "Error creating user: {Username}", user.Username); return false; }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            const string sql = @"UPDATE users SET email=@Email, role_id=@RoleId, is_active=@IsActive,
                                 last_login=@LastLogin, failed_login_attempts=@FailedLoginAttempts, lockout_until=@LockoutUntil
                                 WHERE id=@Id";
            try
            {
                using var c = GetConnection();
                return await c.ExecuteAsync(sql, new { user.Id, user.Email, user.RoleId, user.IsActive, user.LastLogin, user.FailedLoginAttempts, user.LockoutUntil }) > 0;
            }
            catch (NpgsqlException ex) { _logger.LogError(ex, "Error updating user: {Username}", user.Username); return false; }
        }

        public async Task<bool> UpdateLoginAttemptsAsync(int userId, bool success)
        {
            var sql = success
                ? @"UPDATE users SET failed_login_attempts = 0, lockout_until = NULL, last_login = @LastLogin WHERE id = @UserId"
                : @"UPDATE users SET failed_login_attempts = failed_login_attempts + 1,
                        lockout_until = CASE WHEN failed_login_attempts + 1 >= 5 THEN @LockoutTime ELSE lockout_until END
                    WHERE id = @UserId";
            using var c = GetConnection();
            return await c.ExecuteAsync(sql, new { UserId = userId, LastLogin = DateTime.UtcNow, LockoutTime = DateTime.UtcNow.AddMinutes(15) }) > 0;
        }

        public async Task<bool> RecordLoginAsync(int userId)
        {
            const string sql = "UPDATE users SET last_login = @LastLogin, failed_login_attempts = 0 WHERE id = @UserId";
            using var c = GetConnection();
            return await c.ExecuteAsync(sql, new { UserId = userId, LastLogin = DateTime.UtcNow }) > 0;
        }
    }
}