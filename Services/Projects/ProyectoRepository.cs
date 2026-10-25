using AuthenticationModule.Models;
using AuthenticationModule.Services.Exceptions;
using Dapper;
using Npgsql;

namespace AuthenticationModule.Services.Projects
{
    /// <summary>Repositorio de proyectos (Dapper, herencia TPT: el tipo lo da la tabla derivada).</summary>
    public class ProyectoRepository : IProyectoRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<ProyectoRepository> _logger;

        public ProyectoRepository(IConfiguration configuration, ILogger<ProyectoRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        private NpgsqlConnection GetConnection() => new NpgsqlConnection(_connectionString);

        private const string SelectSocio = @"
            SELECT p.*, s.beneficiarios, s.ubicacion_geografica, s.actores_involucrados,
                   s.impacto_social_esperado, s.estrategias_sostenibilidad
            FROM proyectos p INNER JOIN proyectos_sociocomunitarios s ON p.id = s.id";

        private const string SelectEmpr = @"
            SELECT p.*, e.segmento_mercado, e.propuesta_valor, e.analisis_competencia, e.modelo_ingresos,
                   e.estimacion_inversion_inicial, e.viabilidad_economica, e.estrategia_comercializacion
            FROM proyectos p INNER JOIN proyectos_emprendimientos e ON p.id = e.id";

        private static (int min, int max) Limites(TipoProyecto tipo) => tipo switch
        {
            TipoProyecto.SocioComunitario => (2, 3),
            TipoProyecto.Emprendimiento => (1, 3),
            _ => throw new BusinessException("Tipo de proyecto no válido")
        };

        private async Task<List<T>> ListAsync<T>(string sql, object param = null)
        {
            try
            {
                using var c = GetConnection();
                return (await c.QueryAsync<T>(sql, param)).ToList();
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Error al consultar {Tipo}", typeof(T).Name);
                throw;
            }
        }

        private static async Task<TipoProyecto?> ObtenerTipoAsync(NpgsqlConnection c, int proyectoId, NpgsqlTransaction t = null)
        {
            var tipo = await c.QueryFirstOrDefaultAsync<int?>(
                "SELECT tipo_proyecto FROM proyectos WHERE id = @Id", new { Id = proyectoId }, t);
            return tipo.HasValue ? (TipoProyecto)tipo.Value : null;
        }

        // ===== CONSULTAS =====
        public Task<List<ProyectoSociocomunitario>> GetSociocomunitariosAsync() =>
            ListAsync<ProyectoSociocomunitario>(SelectSocio + " ORDER BY p.created_at DESC");

        public Task<List<ProyectoEmprendimiento>> GetEmprendimientosAsync() =>
            ListAsync<ProyectoEmprendimiento>(SelectEmpr + " ORDER BY p.created_at DESC");

        public async Task<List<Proyecto>> GetAllAsync()
        {
            var socios = await GetSociocomunitariosAsync();
            var emprs = await GetEmprendimientosAsync();
            return socios.Cast<Proyecto>().Concat(emprs).OrderByDescending(p => p.CreatedAt).ToList();
        }

        public async Task<Proyecto> GetByIdAsync(int id)
        {
            var socio = (await ListAsync<ProyectoSociocomunitario>(SelectSocio + " WHERE p.id = @Id", new { Id = id })).FirstOrDefault();
            if (socio != null) return socio;
            return (await ListAsync<ProyectoEmprendimiento>(SelectEmpr + " WHERE p.id = @Id", new { Id = id })).FirstOrDefault();
        }

        // ===== CREACIÓN =====
        public async Task<int> CreateAsync(Proyecto proyecto)
        {
            if (proyecto == null) throw new BusinessException("El proyecto no puede ser nulo");

            var tipo = proyecto switch
            {
                ProyectoSociocomunitario => TipoProyecto.SocioComunitario,
                ProyectoEmprendimiento => TipoProyecto.Emprendimiento,
                _ => throw new BusinessException("El tipo de proyecto no es válido. Debe ser ProyectoSociocomunitario o ProyectoEmprendimiento")
            };

            using var connection = GetConnection();
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                const string sqlBase = @"
                    INSERT INTO proyectos (titulo, problematica, objetivo_general, descripcion_prototipo,
                                           recursos_tecnologias, tipo_proyecto, created_at)
                    VALUES (@Titulo, @Problematica, @ObjetivoGeneral, @DescripcionPrototipo,
                            @RecursosTecnologias, @Tipo, @CreatedAt)
                    RETURNING id";

                var id = await connection.ExecuteScalarAsync<int>(sqlBase, new
                {
                    proyecto.Titulo,
                    proyecto.Problematica,
                    proyecto.ObjetivoGeneral,
                    proyecto.DescripcionPrototipo,
                    proyecto.RecursosTecnologias,
                    Tipo = (int)tipo,
                    proyecto.CreatedAt
                }, transaction);

                if (proyecto is ProyectoSociocomunitario s)
                {
                    await connection.ExecuteAsync(@"
                        INSERT INTO proyectos_sociocomunitarios
                            (id, beneficiarios, ubicacion_geografica, actores_involucrados, impacto_social_esperado, estrategias_sostenibilidad)
                        VALUES (@Id, @Beneficiarios, @UbicacionGeografica, @ActoresInvolucrados, @ImpactoSocialEsperado, @EstrategiasSostenibilidad)",
                        new { Id = id, s.Beneficiarios, s.UbicacionGeografica, s.ActoresInvolucrados, s.ImpactoSocialEsperado, s.EstrategiasSostenibilidad },
                        transaction);
                }
                else if (proyecto is ProyectoEmprendimiento e)
                {
                    await connection.ExecuteAsync(@"
                        INSERT INTO proyectos_emprendimientos
                            (id, segmento_mercado, propuesta_valor, analisis_competencia, modelo_ingresos,
                             estimacion_inversion_inicial, viabilidad_economica, estrategia_comercializacion)
                        VALUES (@Id, @SegmentoMercado, @PropuestaValor, @AnalisisCompetencia, @ModeloIngresos,
                                @EstimacionInversionInicial, @ViabilidadEconomica, @EstrategiaComercializacion)",
                        new
                        {
                            Id = id,
                            e.SegmentoMercado,
                            e.PropuestaValor,
                            e.AnalisisCompetencia,
                            e.ModeloIngresos,
                            e.EstimacionInversionInicial,
                            e.ViabilidadEconomica,
                            e.EstrategiaComercializacion
                        },
                        transaction);
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Proyecto creado con Id = {Id}", id);
                return id;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al crear proyecto");
                throw;
            }
        }

        // ===== ACTUALIZACIÓN =====
        public async Task<bool> UpdateAsync(Proyecto proyecto)
        {
            if (proyecto == null) throw new BusinessException("El proyecto no puede ser nulo");

            using var connection = GetConnection();
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                var rows = await connection.ExecuteAsync(@"
                    UPDATE proyectos SET titulo = @Titulo, problematica = @Problematica,
                        objetivo_general = @ObjetivoGeneral, descripcion_prototipo = @DescripcionPrototipo,
                        recursos_tecnologias = @RecursosTecnologias
                    WHERE id = @Id",
                    new
                    {
                        proyecto.Id,
                        proyecto.Titulo,
                        proyecto.Problematica,
                        proyecto.ObjetivoGeneral,
                        proyecto.DescripcionPrototipo,
                        proyecto.RecursosTecnologias
                    }, transaction);

                if (rows == 0) throw new NotFoundException("Proyecto", proyecto.Id);

                if (proyecto is ProyectoSociocomunitario s)
                {
                    await connection.ExecuteAsync(@"
                        UPDATE proyectos_sociocomunitarios SET beneficiarios = @Beneficiarios,
                            ubicacion_geografica = @UbicacionGeografica, actores_involucrados = @ActoresInvolucrados,
                            impacto_social_esperado = @ImpactoSocialEsperado, estrategias_sostenibilidad = @EstrategiasSostenibilidad
                        WHERE id = @Id",
                        new { s.Id, s.Beneficiarios, s.UbicacionGeografica, s.ActoresInvolucrados, s.ImpactoSocialEsperado, s.EstrategiasSostenibilidad },
                        transaction);
                }
                else if (proyecto is ProyectoEmprendimiento e)
                {
                    await connection.ExecuteAsync(@"
                        UPDATE proyectos_emprendimientos SET segmento_mercado = @SegmentoMercado,
                            propuesta_valor = @PropuestaValor, analisis_competencia = @AnalisisCompetencia,
                            modelo_ingresos = @ModeloIngresos, estimacion_inversion_inicial = @EstimacionInversionInicial,
                            viabilidad_economica = @ViabilidadEconomica, estrategia_comercializacion = @EstrategiaComercializacion
                        WHERE id = @Id",
                        new
                        {
                            e.Id,
                            e.SegmentoMercado,
                            e.PropuestaValor,
                            e.AnalisisCompetencia,
                            e.ModeloIngresos,
                            e.EstimacionInversionInicial,
                            e.ViabilidadEconomica,
                            e.EstrategiaComercializacion
                        },
                        transaction);
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Proyecto actualizado con Id = {Id}", proyecto.Id);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                if (ex is not NotFoundException) _logger.LogError(ex, "Error al actualizar proyecto {Id}", proyecto.Id);
                throw;
            }
        }

        // ===== ELIMINACIÓN =====
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                using var c = GetConnection();
                var rows = await c.ExecuteAsync("DELETE FROM proyectos WHERE id = @Id", new { Id = id });
                if (rows == 0) throw new NotFoundException("Proyecto", id);
                _logger.LogInformation("Proyecto eliminado con Id = {Id}", id);
                return true;
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Error al eliminar proyecto {Id}", id);
                throw;
            }
        }

        // ===== ASIGNACIÓN DE ESTUDIANTES =====
        public async Task<bool> AsignarEstudianteAsync(int proyectoId, int estudianteId)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                var tipo = await ObtenerTipoAsync(connection, proyectoId, transaction)
                           ?? throw new NotFoundException("Proyecto", proyectoId);

                var existe = await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM estudiantes WHERE id = @Id", new { Id = estudianteId }, transaction);
                if (existe == 0) throw new NotFoundException("Estudiante", estudianteId);

                var proyectoActual = await connection.QueryFirstOrDefaultAsync<int?>(
                    "SELECT proyecto_id FROM estudiantes WHERE id = @Id", new { Id = estudianteId }, transaction);
                if (proyectoActual.HasValue)
                    throw new BusinessException($"El estudiante {estudianteId} ya tiene asignado el proyecto {proyectoActual.Value}");

                var cantidad = await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM estudiantes WHERE proyecto_id = @ProyectoId", new { ProyectoId = proyectoId }, transaction);

                var (_, max) = Limites(tipo);
                if (cantidad >= max)
                    throw new BusinessException($"El proyecto ya tiene el máximo de {max} estudiantes permitidos");

                await connection.ExecuteAsync(
                    "UPDATE estudiantes SET proyecto_id = @ProyectoId WHERE id = @EstudianteId",
                    new { ProyectoId = proyectoId, EstudianteId = estudianteId }, transaction);

                await transaction.CommitAsync();
                _logger.LogInformation("Estudiante {EstudianteId} asignado al proyecto {ProyectoId}", estudianteId, proyectoId);
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ValidarCantidadEstudiantesAsync(int proyectoId)
        {
            try
            {
                using var connection = GetConnection();
                var tipo = await ObtenerTipoAsync(connection, proyectoId)
                           ?? throw new NotFoundException("Proyecto", proyectoId);

                var cantidad = await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM estudiantes WHERE proyecto_id = @ProyectoId", new { ProyectoId = proyectoId });

                var (min, max) = Limites(tipo);
                var esValido = cantidad >= min && cantidad <= max;
                if (!esValido)
                    _logger.LogWarning("Proyecto {Id} tiene {Cantidad} estudiantes (requerido: {Min}-{Max})", proyectoId, cantidad, min, max);
                return esValido;
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Error al validar cantidad de estudiantes del proyecto {Id}", proyectoId);
                throw;
            }
        }
    }
}