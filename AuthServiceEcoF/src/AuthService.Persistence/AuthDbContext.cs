using Microsoft.EntityFrameworkCore;
using AuthService.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Persistence;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    // DbSets con tipos específicos
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Rol> Roles { get; set; } = null!;
    public DbSet<RolUsuario> RolesUsuario { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsuarios(modelBuilder);
        ConfigureRoles(modelBuilder);
        ConfigureRolesUsuario(modelBuilder);
        SeedData(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Añadir comportamiento útil de EF Core en desarrollo
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.LogTo(Console.WriteLine);
            optionsBuilder.EnableSensitiveDataLogging(
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development");
        }
    }

    // Sobrescribir SaveChanges para manejar automáticamente propiedades de auditoría
    public override int SaveChanges()
    {
        UpdateAuditableEntities();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditableEntities()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is Usuario usuario && entry.State == EntityState.Modified)
            {
                // Usar reflexión para actualizar ActualizadoEn ya que el setter es privado
                typeof(Usuario).GetProperty("ActualizadoEn")?.SetValue(usuario, now);
            }
            else if (entry.Entity is Rol rol && entry.State == EntityState.Modified)
            {
                // Usar reflexión para actualizar ActualizadoEn ya que el setter es privado
                typeof(Rol).GetProperty("ActualizadoEn")?.SetValue(rol, now);
            }
        }
    }

    #region Configuraciones de Entidades

    private static void ConfigureUsuarios(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");

            entity.HasKey(u => u.UsuarioId);
            entity.Property(u => u.UsuarioId).HasColumnName("UsuarioId").ValueGeneratedOnAdd();

            entity.HasIndex(u => u.NombreUsuario).IsUnique();
            entity.Property(u => u.NombreUsuario).HasMaxLength(100).IsRequired();

            entity.HasIndex(u => u.CorreoElectronico).IsUnique();
            entity.Property(u => u.CorreoElectronico).HasMaxLength(100).IsRequired();

            entity.Property(u => u.HashContrasena).IsRequired();
            entity.Property(u => u.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Apellido).HasMaxLength(100).IsRequired();

            // CAMBIO: Comentar temporalmente el filtro global para evitar problemas
            // entity.HasQueryFilter(u => u.EstaActivo);

            // Configurar propiedades de auditoría
            entity.Property(u => u.CreadoEn).IsRequired();
            entity.Property(u => u.ActualizadoEn).IsRequired();
        });
    }

    private static void ConfigureRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("Roles");

            entity.HasKey(r => r.RolId);
            entity.Property(r => r.RolId).HasColumnName("RolId").ValueGeneratedOnAdd();

            entity.HasIndex(r => r.Nombre).IsUnique();
            entity.Property(r => r.Nombre).HasMaxLength(50).IsRequired();
            entity.Property(r => r.Descripcion).HasMaxLength(200);

            // Configurar propiedades de auditoría
            entity.Property(r => r.CreadoEn).IsRequired();
            entity.Property(r => r.ActualizadoEn).IsRequired();
        });
    }

    private static void ConfigureRolesUsuario(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RolUsuario>(entity =>
        {
            entity.ToTable("RolesUsuario");

            entity.HasKey(ru => ru.RolUsuarioId);
            entity.Property(ru => ru.RolUsuarioId).HasColumnName("RolUsuarioId").ValueGeneratedOnAdd();

            entity.HasIndex(ru => new { ru.UsuarioId, ru.RolId }).IsUnique();

            entity.HasOne(ru => ru.Usuario)
                .WithMany(u => u.RolesUsuario)
                .HasForeignKey(ru => ru.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity.HasOne(ru => ru.Rol)
                .WithMany(r => r.RolesUsuario)
                .HasForeignKey(ru => ru.RolId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity.Property(ru => ru.CreadoEn).IsRequired();
        });
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var baseTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Semilla de datos de rol
        modelBuilder.Entity<Rol>().HasData(
            new
            {
                RolId = 1,
                Nombre = "Administrador",
                Descripcion = "Acceso total al sistema",
                CreadoEn = baseTime,
                ActualizadoEn = baseTime
            },
            new
            {
                RolId = 2,
                Nombre = "Operador",
                Descripcion = "Control limitado de módulos",
                CreadoEn = baseTime,
                ActualizadoEn = baseTime
            },
            new
            {
                RolId = 3,
                Nombre = "Supervisor",
                Descripcion = "Supervisa sin modificar",
                CreadoEn = baseTime,
                ActualizadoEn = baseTime
            }
        );
        //ssemilla rolus
        modelBuilder.Entity<RolUsuario>().HasData(
            new
            {
                RolUsuarioId = 1,
                UsuarioId = 1,
                RolId = 1,
                CreadoEn = baseTime
            }
            );
        //semila admin
        modelBuilder.Entity<Usuario>().HasData(
            new
            {
                UsuarioId = 1,
                NombreUsuario = "Administrador",
                CorreoElectronico = "administrador@hotmail.com",
                HashContrasena = "$2a$12$wZszxjW5OJ4JpqVygftlPO51.xjbuTS1MHvo3JEXdMbuwElbkjyiS",
                Nombre = "Administrador",
                Apellido = "Sistema",
                EstaActivo = true,
                UltimoAcceso = baseTime,
                CreadoEn = baseTime,
                ActualizadoEn = baseTime
            }
            );
    }
    
    #endregion
}