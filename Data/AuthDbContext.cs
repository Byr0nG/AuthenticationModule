using AuthenticationModule.Models;
using Microsoft.EntityFrameworkCore;
namespace AuthenticationModule.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options)
           : base(options) { }

        // Cada DbSet = una tabla
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        // Polimorfismo: sobrescribimos el método base
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Índices únicos
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Role>().HasIndex(r => r.Name).IsUnique();
            // === Módulo de proyectos ===

            // Herencia TPT: cada tipo de proyecto tiene su propia tabla
            modelBuilder.Entity<Proyecto>().UseTptMappingStrategy();

            // Proyecto 1:N Estudiante (si se borra el proyecto, el estudiante queda libre)
            modelBuilder.Entity<Estudiante>()
                .HasOne(e => e.Proyecto)
                .WithMany(p => p.Estudiantes)
                .HasForeignKey(e => e.ProyectoId)
                .OnDelete(DeleteBehavior.SetNull);

            // Proyecto N:M Docente mediante Evaluacion
            modelBuilder.Entity<Evaluacion>()
                .HasOne(ev => ev.Proyecto)
                .WithMany(p => p.Evaluaciones)
                .HasForeignKey(ev => ev.ProyectoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Evaluacion>()
                .HasOne(ev => ev.Docente)
                .WithMany(d => d.Evaluaciones)
                .HasForeignKey(ev => ev.DocenteId)
                .OnDelete(DeleteBehavior.Restrict);

            SeedRoles(modelBuilder);
        }

        private void SeedRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    Id = 1,
                    Name = "Admin",
                    Description = "Administrator with full access",
                    Permissions = "{\"all\": true}"
                },
                new Role
                {
                    Id = 2,
                    Name = "User",
                    Description = "Standard user with limited access",
                    Permissions = "{\"read\": true, \"write\": false}"
                },
                new Role
                {
                    Id = 3,
                    Name = "Guest",
                    Description = "Guest user with read-only access",
                    Permissions = "{\"read\": true, \"write\": false, \"delete\": false}"
                }
            );
            // Los usuarios admin/user se siembran en Program.cs (Fase 9)
        }
        public DbSet<Proyecto> Proyectos { get; set; }
        public DbSet<ProyectoSociocomunitario> ProyectosSociocomunitarios { get; set; }
        public DbSet<ProyectoEmprendimiento> ProyectosEmprendimientos { get; set; }
        public DbSet<Estudiante> Estudiantes { get; set; }
        public DbSet<Docente> Docentes { get; set; }
        public DbSet<Evaluacion> Evaluaciones { get; set; }
    }
}
