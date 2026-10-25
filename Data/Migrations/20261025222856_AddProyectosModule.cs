using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AuthenticationModule.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProyectosModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "docentes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellido_paterno = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellido_materno = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    especialidad = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_docentes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "proyectos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    problematica = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    objetivo_general = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    descripcion_prototipo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    recursos_tecnologias = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    tipo_proyecto = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proyectos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "estudiantes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellido_paterno = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellido_materno = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    curso = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    proyecto_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estudiantes", x => x.id);
                    table.ForeignKey(
                        name: "FK_estudiantes_proyectos_proyecto_id",
                        column: x => x.proyecto_id,
                        principalTable: "proyectos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "evaluaciones",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    proyecto_id = table.Column<int>(type: "integer", nullable: false),
                    docente_id = table.Column<int>(type: "integer", nullable: false),
                    observaciones = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    recomendaciones = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    fecha_evaluacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    nota = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evaluaciones", x => x.id);
                    table.ForeignKey(
                        name: "FK_evaluaciones_docentes_docente_id",
                        column: x => x.docente_id,
                        principalTable: "docentes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_evaluaciones_proyectos_proyecto_id",
                        column: x => x.proyecto_id,
                        principalTable: "proyectos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "proyectos_emprendimientos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    segmento_mercado = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    propuesta_valor = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    analisis_competencia = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    modelo_ingresos = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    estimacion_inversion_inicial = table.Column<decimal>(type: "numeric", nullable: false),
                    viabilidad_economica = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    estrategia_comercializacion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proyectos_emprendimientos", x => x.id);
                    table.ForeignKey(
                        name: "FK_proyectos_emprendimientos_proyectos_id",
                        column: x => x.id,
                        principalTable: "proyectos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "proyectos_sociocomunitarios",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    beneficiarios = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ubicacion_geografica = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    actores_involucrados = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    impacto_social_esperado = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    estrategias_sostenibilidad = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proyectos_sociocomunitarios", x => x.id);
                    table.ForeignKey(
                        name: "FK_proyectos_sociocomunitarios_proyectos_id",
                        column: x => x.id,
                        principalTable: "proyectos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_estudiantes_proyecto_id",
                table: "estudiantes",
                column: "proyecto_id");

            migrationBuilder.CreateIndex(
                name: "IX_evaluaciones_docente_id",
                table: "evaluaciones",
                column: "docente_id");

            migrationBuilder.CreateIndex(
                name: "IX_evaluaciones_proyecto_id",
                table: "evaluaciones",
                column: "proyecto_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "estudiantes");

            migrationBuilder.DropTable(
                name: "evaluaciones");

            migrationBuilder.DropTable(
                name: "proyectos_emprendimientos");

            migrationBuilder.DropTable(
                name: "proyectos_sociocomunitarios");

            migrationBuilder.DropTable(
                name: "docentes");

            migrationBuilder.DropTable(
                name: "proyectos");
        }
    }
}
