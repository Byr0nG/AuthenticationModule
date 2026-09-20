using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthenticationModule.Models
{
    [Table("roles")]
    public class Role
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        // Permisos como JSON (flexibilidad)
        [Column("permissions")]
        public string Permissions { get; set; }

        // Un rol tiene muchos usuarios
        public virtual ICollection<User> Users { get; set; }
    }
}
