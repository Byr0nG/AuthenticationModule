using AuthenticationModule.Models;

namespace AuthenticationModule.Services.Projects
{
    /// <summary>
    /// Interfaz del repositorio de proyectos.
    /// Define las operaciones de acceso a datos para la entidad <see cref="Proyecto"/>.
    /// </summary>
    public interface IProyectoRepository
    {
        /// <summary>Obtiene todos los proyectos (sociocomunitarios y de emprendimiento).</summary>
        Task<List<Proyecto>> GetAllAsync();

        /// <summary>Obtiene un proyecto por su identificador, o null si no existe.</summary>
        Task<Proyecto> GetByIdAsync(int id);

        /// <summary>Obtiene todos los proyectos sociocomunitarios.</summary>
        Task<List<ProyectoSociocomunitario>> GetSociocomunitariosAsync();

        /// <summary>Obtiene todos los proyectos de emprendimiento.</summary>
        Task<List<ProyectoEmprendimiento>> GetEmprendimientosAsync();

        /// <summary>Crea un proyecto y devuelve su identificador.</summary>
        Task<int> CreateAsync(Proyecto proyecto);

        /// <summary>Actualiza un proyecto existente.</summary>
        Task<bool> UpdateAsync(Proyecto proyecto);

        /// <summary>Elimina un proyecto por su identificador.</summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Asigna un estudiante a un proyecto.
        /// Valida el máximo de estudiantes según el tipo de proyecto.
        /// </summary>
        Task<bool> AsignarEstudianteAsync(int proyectoId, int estudianteId);

        /// <summary>Valida que el proyecto tenga la cantidad de estudiantes permitida según su tipo.</summary>
        Task<bool> ValidarCantidadEstudiantesAsync(int proyectoId);
    }
}