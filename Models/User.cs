using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace AuthenticationModule.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("username")]
        public string Username { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("email")]
        public string Email { get; set; }

        // Nunca guardamos la contraseña: solo hash + salt
        [Required]
        [Column("password_hash")]
        public string PasswordHash { get; set; }

        [Column("password_salt")]
        public string PasswordSalt { get; set; }

        // Clave foránea
        [Column("role_id")]
        public int RoleId { get; set; }

        // Propiedad de navegación
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }

        // Auditoría
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("last_login")]
        public DateTime? LastLogin { get; set; }

        // Control de acceso
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Prevención de fuerza bruta
        [Column("failed_login_attempts")]
        public int FailedLoginAttempts { get; set; } = 0;

        [Column("lockout_until")]
        public DateTime? LockoutUntil { get; set; }

        // Sesión en múltiples dispositivos
        [Column("refresh_token")]
        public string RefreshToken { get; set; }

        [Column("refresh_token_expiry")]
        public DateTime? RefreshTokenExpiry { get; set; }
    }
}
