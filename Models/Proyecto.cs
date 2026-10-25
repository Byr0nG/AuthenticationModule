using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthenticationModule.Models
{
    /// <summary>Tipo de proyecto.</summary>
    public enum TipoProyecto { SocioComunitario = 1, Emprendimiento = 2 }

    /// <summary>Clase base de un proyecto.</summary>
    [Table("proyectos")]
    public class Proyecto
    {
        [Key, Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [MinLength(10, ErrorMessage = "El título debe tener al menos 10 caracteres")]
        [MaxLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
        [Column("titulo")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "La problemática es obligatoria")]
        [MinLength(20, ErrorMessage = "Debe tener al menos 20 caracteres")]
        [MaxLength(2000, ErrorMessage = "No puede exceder 2000 caracteres")]
        [Column("problematica")]
        public string Problematica { get; set; }

        [Required(ErrorMessage = "El objetivo general es obligatorio")]
        [MinLength(20, ErrorMessage = "Debe tener al menos 20 caracteres")]
        [MaxLength(1000, ErrorMessage = "No puede exceder 1000 caracteres")]
        [Column("objetivo_general")]
        public string ObjetivoGeneral { get; set; }

        [Required(ErrorMessage = "La descripción del prototipo es obligatoria")]
        [MinLength(20, ErrorMessage = "Debe tener al menos 20 caracteres")]
        [MaxLength(2000, ErrorMessage = "No puede exceder 2000 caracteres")]
        [Column("descripcion_prototipo")]
        public string DescripcionPrototipo { get; set; }

        [Required(ErrorMessage = "Los recursos y tecnologías son obligatorios")]
        [MinLength(10, ErrorMessage = "Debe tener al menos 10 caracteres")]
        [MaxLength(1000, ErrorMessage = "No puede exceder 1000 caracteres")]
        [Column("recursos_tecnologias")]
        public string RecursosTecnologias { get; set; }

        [Required]
        [Column("tipo_proyecto")]
        public TipoProyecto TipoProyecto { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relaciones
        public virtual ICollection<Estudiante> Estudiantes { get; set; } = new List<Estudiante>();
        public virtual ICollection<Evaluacion> Evaluaciones { get; set; } = new List<Evaluacion>();
    }
}