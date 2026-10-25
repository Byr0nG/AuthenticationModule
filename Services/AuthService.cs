using System.Security.Claims;
using AuthenticationModule.Decorators;
using AuthenticationModule.Models;
using AuthenticationModule.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AuthenticationModule.Services
{
    public interface IAuthService
    {
        Task<LoginResult> LoginAsync(string username, string password);
        // dentro de IAuthService:
        Task<RegisterResult> RegisterAsync(string username, string email, string password, string confirmPassword);
        Task LogoutAsync();
        Task<User> GetCurrentUserAsync(ClaimsPrincipal principal);
        bool ValidatePassword(string password);
    }


    public class LoginResult 
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public User User { get; set; }
    }
    // al lado de la clase LoginResult:
    public class RegisterResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public User User { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repo;
        private readonly IHttpContextAccessor _http;
        private readonly ILogger<AuthService> _logger;
        private readonly ILoggingService _log;

        public AuthService(IUserRepository repo, IHttpContextAccessor http, ILogger<AuthService> logger, ILoggingService log)
        { _repo = repo; _http = http; _logger = logger; _log = log; }

        private string Ip => _http.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "desconocida";

        public async Task<LoginResult> LoginAsync(string username, string password)
        {
            try
            {
                var user = await _repo.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    await Task.Delay(1000); // anti timing-attack
                    await _log.LogAsync("LoginFailed", username, $"Usuario no encontrado. IP: {Ip}", LogLevel.Warning);
                    return new LoginResult { Message = "Credenciales inválidas" };
                }

                if (user.LockoutUntil.HasValue && user.LockoutUntil > DateTime.UtcNow)
                {
                    var msg = $"Cuenta bloqueada hasta {user.LockoutUntil.Value.ToLocalTime():HH:mm}";
                    await _log.LogAsync("LoginBlocked", username, $"{msg}. IP: {Ip}", LogLevel.Warning);
                    return new LoginResult { Message = msg };
                }

                if (!PasswordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
                {
                    await _repo.UpdateLoginAttemptsAsync(user.Id, false);
                    await _log.LogAsync("LoginFailed", username, $"Contraseña incorrecta. IP: {Ip}", LogLevel.Warning);
                    return new LoginResult { Message = "Credenciales inválidas" };
                }

                await _repo.UpdateLoginAttemptsAsync(user.Id, true);
                await _repo.RecordLoginAsync(user.Id);

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new(ClaimTypes.Name, user.Username),
                    new(ClaimTypes.Email, user.Email),
                    new(ClaimTypes.Role, user.Role.Name),
                    new("UserId", user.Id.ToString())
                };
                var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

                await _http.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                    new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTime.UtcNow.AddDays(7), IssuedUtc = DateTime.UtcNow });

                await _log.LogAsync("Login", username, $"Login exitoso. Rol: {user.Role.Name}. IP: {Ip}", LogLevel.Information);
                return new LoginResult { Success = true, User = user };
            }
            catch (Exception ex)
            {
                await _log.LogAsync("LoginError", username, $"Error del sistema: {ex.Message}. IP: {Ip}", LogLevel.Error);
                _logger.LogError(ex, "Error de login para: {Username}", username);
                return new LoginResult { Message = "Error en el sistema" };
            }
        }

        public async Task LogoutAsync()
        {
            var username = _http.HttpContext?.User?.Identity?.Name ?? "desconocido";
            await _http.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await _log.LogAsync("Logout", username, $"Sesión cerrada. IP: {Ip}", LogLevel.Information);
        }

        public async Task<User> GetCurrentUserAsync(ClaimsPrincipal principal)
        {
            if (principal?.Identity?.IsAuthenticated != true) return null;
            return await _repo.GetUserByUsernameAsync(principal.Identity.Name);
        }
        public async Task<RegisterResult> RegisterAsync(string username, string email, string password, string confirmPassword)
        {
            var result = new RegisterResult();
            try
            {
                // 1. Validaciones de entrada
                if (string.IsNullOrWhiteSpace(username)) result.Errors.Add("El nombre de usuario es obligatorio");
                if (string.IsNullOrWhiteSpace(email)) result.Errors.Add("El email es obligatorio");
                if (string.IsNullOrWhiteSpace(password)) result.Errors.Add("La contraseña es obligatoria");
                if (!string.IsNullOrWhiteSpace(email) && !IsValidEmail(email)) result.Errors.Add("El formato del email no es válido");
                if (!string.IsNullOrWhiteSpace(username) && username.Length < 3) result.Errors.Add("El nombre de usuario debe tener al menos 3 caracteres");
                if (!ValidatePassword(password))
                    result.Errors.Add("La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula, un número y un carácter especial");
                if (password != confirmPassword) result.Errors.Add("Las contraseñas no coinciden");

                if (result.Errors.Any())
                {
                    result.Message = "Corrige los errores antes de continuar";
                    return result;
                }

                // 2. Duplicados
                if (await _repo.GetUserByUsernameAsync(username) != null) result.Errors.Add("El nombre de usuario ya está en uso");
                if (await _repo.GetUserByEmailAsync(email) != null) result.Errors.Add("El email ya está registrado");

                if (result.Errors.Any())
                {
                    result.Message = "El usuario ya existe";
                    await _log.LogAsync("RegisterFailed", username, $"Intento de registro con datos duplicados. IP: {Ip}", LogLevel.Warning);
                    return result;
                }

                // 3. Crear usuario (el repositorio hace el hash)
                var newUser = new User
                {
                    Username = username.Trim(),
                    Email = email.Trim().ToLowerInvariant(),
                    RoleId = 2, // rol "User" por defecto
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    FailedLoginAttempts = 0
                };

                if (!await _repo.CreateUserAsync(newUser, password))
                {
                    result.Message = "No se pudo crear el usuario. Intenta nuevamente";
                    await _log.LogAsync("RegisterError", username, $"Error al crear usuario en BD. IP: {Ip}", LogLevel.Error);
                    return result;
                }

                // 4. Éxito
                result.Success = true;
                result.User = newUser;
                result.Message = "Usuario registrado exitosamente";
                await _log.LogAsync("Register", username, $"Usuario registrado con rol User. IP: {Ip}", LogLevel.Information);
                return result;
            }
            catch (Exception ex)
            {
                await _log.LogAsync("RegisterError", username, $"Error del sistema: {ex.Message}. IP: {Ip}", LogLevel.Error);
                _logger.LogError(ex, "Error de registro para: {Username}", username);
                result.Message = "Error en el sistema. Intenta nuevamente";
                return result;
            }
        }

        private static bool IsValidEmail(string email)
        {
            try { return new System.Net.Mail.MailAddress(email).Address == email; }
            catch { return false; }
        }

        public bool ValidatePassword(string p) =>
            !string.IsNullOrEmpty(p) && p.Length >= 8 && p.Any(char.IsUpper) && p.Any(char.IsLower)
            && p.Any(char.IsDigit) && p.Any(c => !char.IsLetterOrDigit(c));
    }
}