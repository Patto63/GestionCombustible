using Microsoft.EntityFrameworkCore;
using VehicleService.Domain.Entities;
using VehicleService.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace VehicleService.Persistence
{
    public class VehicleDbContext : DbContext
    {
        public VehicleDbContext(DbContextOptions<VehicleDbContext> options) : base(options) { }

        // DbSets con tipos específicos
        public DbSet<TipoVehiculo> TiposVehiculo { get; set; } = null!;
        public DbSet<Marca> Marcas { get; set; } = null!;
        public DbSet<Modelo> Modelos { get; set; } = null!;
        public DbSet<Vehiculo> Vehiculos { get; set; } = null!;
        public DbSet<EstadoOperacionalVehiculo> EstadosOperacionales { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureTiposVehiculo(modelBuilder);
            ConfigureMarcas(modelBuilder);
            ConfigureModelos(modelBuilder);
            ConfigureVehiculos(modelBuilder);
            ConfigureEstadosOperacionales(modelBuilder);

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
                switch (entry.Entity)
                {
                    case TipoVehiculo tipoVehiculo when entry.State == EntityState.Modified:
                        typeof(TipoVehiculo).GetProperty("ActualizadoEn")?.SetValue(tipoVehiculo, now);
                        break;
                    case Marca marca when entry.State == EntityState.Modified:
                        typeof(Marca).GetProperty("ActualizadoEn")?.SetValue(marca, now);
                        break;
                    case Modelo modelo when entry.State == EntityState.Modified:
                        typeof(Modelo).GetProperty("ActualizadoEn")?.SetValue(modelo, now);
                        break;
                    case Vehiculo vehiculo when entry.State == EntityState.Modified:
                        typeof(Vehiculo).GetProperty("ActualizadoEn")?.SetValue(vehiculo, now);
                        break;
                }
            }
        }

        #region Configuraciones de Entidades

        private static void ConfigureTiposVehiculo(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TipoVehiculo>(entity =>
            {
                entity.ToTable("TiposVehiculo");

                // Clave primaria 
                entity.HasKey(t => t.TipoVehiculoId);
                entity.Property(t => t.TipoVehiculoId)
                    .HasColumnName("TipoId")  
                    .ValueGeneratedOnAdd();

                // Propiedades
                entity.HasIndex(t => t.Nombre).IsUnique();
                entity.Property(t => t.Nombre)
                    .HasMaxLength(100)
                    .IsRequired();

               
                entity.Property(t => t.TipoMaquinariaVehiculoId)
                    .HasColumnName("TipoMaquinaria")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(t => t.Descripcion)
                    .HasMaxLength(500);

                // Configurar propiedades de auditoría
                entity.Property(t => t.CreadoEn).IsRequired();
                entity.Property(t => t.ActualizadoEn).IsRequired();

                // Relación con Vehiculos - RESTAURADA
                entity.HasMany(t => t.Vehiculos)
                    .WithOne(v => v.Tipo)
                    .HasForeignKey(v => v.TipoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureMarcas(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Marca>(entity =>
            {
                entity.ToTable("Marcas");

                entity.HasKey(m => m.MarcaId);
                entity.Property(m => m.MarcaId)
                    .HasColumnName("MarcaId")
                    .ValueGeneratedOnAdd();

                entity.HasIndex(m => m.Nombre).IsUnique();
                entity.Property(m => m.Nombre)
                    .HasMaxLength(100)
                    .IsRequired();

                // Configurar propiedades de auditoría
                entity.Property(m => m.CreadoEn).IsRequired();
                entity.Property(m => m.ActualizadoEn).IsRequired();

                // Relación con Modelos
                entity.HasMany(m => m.Modelos)
                    .WithOne(mo => mo.Marca)
                    .HasForeignKey(mo => mo.MarcaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureModelos(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Modelo>(entity =>
            {
                entity.ToTable("Modelos");

                entity.HasKey(m => m.ModeloId);
                entity.Property(m => m.ModeloId)
                    .HasColumnName("ModeloId")
                    .ValueGeneratedOnAdd();

                entity.Property(m => m.Nombre)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(m => m.Año).IsRequired();

                entity.Property(m => m.TipoCombustible)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(m => m.ConsumoEstandar)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                entity.Property(m => m.PorcentajeTolerancia)
                    .HasColumnType("decimal(5,2)")
                    .IsRequired();

                entity.Property(m => m.Descripcion)
                    .HasMaxLength(500);

                // Configurar propiedades de auditoría
                entity.Property(m => m.CreadoEn).IsRequired();
                entity.Property(m => m.ActualizadoEn).IsRequired();

                // Relaciones
                entity.HasOne(m => m.Marca)
                    .WithMany(ma => ma.Modelos)
                    .HasForeignKey(m => m.MarcaId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                // Relación con Vehiculos
                entity.HasMany(m => m.Vehiculos)
                    .WithOne(v => v.Modelo)
                    .HasForeignKey(v => v.ModeloId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Índice único compuesto
                entity.HasIndex(m => new { m.MarcaId, m.Nombre }).IsUnique();
            });
        }

        private static void ConfigureVehiculos(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vehiculo>(entity =>
            {
                entity.ToTable("Vehiculos");

                entity.HasKey(v => v.VehiculoId);
                entity.Property(v => v.VehiculoId)
                    .HasColumnName("VehiculoId")
                    .ValueGeneratedOnAdd();

                // Propiedades únicas
                entity.HasIndex(v => v.Codigo).IsUnique();
                entity.Property(v => v.Codigo)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.HasIndex(v => v.Placa).IsUnique();
                entity.Property(v => v.Placa)
                    .HasMaxLength(20)
                    .IsRequired();

                // Enum convertido a string
                entity.Property(v => v.TipoMaquinaria)
                    .HasConversion<string>()
                    .HasMaxLength(30)
                    .IsRequired();

                // Propiedades numéricas
                entity.Property(v => v.AñoFabricacion).IsRequired();
                entity.Property(v => v.FechaCompra).IsRequired();

                entity.Property(v => v.OdometroInicial)
                    .HasColumnType("decimal(12,2)")
                    .IsRequired();

                entity.Property(v => v.OdometroActual)
                    .HasColumnType("decimal(12,2)")
                    .IsRequired();

                entity.Property(v => v.CapacidadCombustible)
                    .HasColumnType("decimal(8,2)")
                    .IsRequired();

                entity.Property(v => v.CapacidadMotor)
                    .HasMaxLength(50)
                    .IsRequired();

                // Estado como enum convertido a string
                entity.Property(v => v.Estado)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();

                // Fechas de mantenimiento (opcionales)
                entity.Property(v => v.FechaUltimoMantenimiento);
                entity.Property(v => v.FechaProximoMantenimiento);

                // Configurar propiedades de auditoría
                entity.Property(v => v.CreadoEn).IsRequired();
                entity.Property(v => v.ActualizadoEn).IsRequired();

                // Relaciones CORREGIDAS
                entity.HasOne(v => v.Tipo)
                    .WithMany(t => t.Vehiculos)
                    .HasForeignKey(v => v.TipoId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                entity.HasOne(v => v.Modelo)
                    .WithMany(m => m.Vehiculos)
                    .HasForeignKey(v => v.ModeloId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                // Relación con EstadosOperacionales
                entity.HasMany(v => v.EstadosOperacionales)
                    .WithOne(e => e.Vehiculo)
                    .HasForeignKey(e => e.VehiculoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureEstadosOperacionales(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EstadoOperacionalVehiculo>(entity =>
            {
                entity.ToTable("EstadoOperacionalVehiculo");

                entity.HasKey(e => e.EstadoId);
                entity.Property(e => e.EstadoId)
                    .HasColumnName("EstadoId")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.VehiculoId).IsRequired();

                entity.Property(e => e.Estado)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(e => e.FechaInicio).IsRequired();
                entity.Property(e => e.FechaFin);

                entity.Property(e => e.Motivo)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(e => e.RegistradoPor)
                    .HasMaxLength(100)
                    .IsRequired();

                // Configurar propiedades de auditoría
                entity.Property(e => e.CreadoEn).IsRequired();

                // Relaciones
                entity.HasOne(e => e.Vehiculo)
                    .WithMany(v => v.EstadosOperacionales)
                    .HasForeignKey(e => e.VehiculoId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            var baseTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Semilla de datos para TiposVehiculo
            modelBuilder.Entity<TipoVehiculo>().HasData(
                new
                {
                    TipoVehiculoId = 1,
                    Nombre = "Automóvil",
                    TipoMaquinariaVehiculoId = "Ligera",
                    Descripcion = "Vehículo de transporte personal",
                    CreadoEn = baseTime,
                    ActualizadoEn = baseTime
                },
                new
                {
                    TipoVehiculoId = 2,
                    Nombre = "Motocicleta",
                    TipoMaquinariaVehiculoId = "Ligera",
                    Descripcion = "Vehículo de dos ruedas",
                    CreadoEn = baseTime,
                    ActualizadoEn = baseTime
                },
                new
                {
                    TipoVehiculoId = 3,
                    Nombre = "Camión",
                    TipoMaquinariaVehiculoId = "Pesada",
                    Descripcion = "Vehículo de carga pesada",
                    CreadoEn = baseTime,
                    ActualizadoEn = baseTime
                }
            );

            // Semilla de datos para Marcas
            modelBuilder.Entity<Marca>().HasData(
                new
                {
                    MarcaId = 1,
                    Nombre = "Toyota",
                    CreadoEn = baseTime,
                    ActualizadoEn = baseTime
                },
                new
                {
                    MarcaId = 2,
                    Nombre = "Honda",
                    CreadoEn = baseTime,
                    ActualizadoEn = baseTime
                },
                new
                {
                    MarcaId = 3,
                    Nombre = "Chevrolet",
                    CreadoEn = baseTime,
                    ActualizadoEn = baseTime
                }
            );

            // Semilla de datos para Modelos
            modelBuilder.Entity<Modelo>().HasData(
                new
                {
                    ModeloId = 1,
                    MarcaId = 1,
                    Nombre = "Corolla",
                    Año = 2023,
                    TipoCombustible = TipoCombustible.Gasolina,
                    ConsumoEstandar = 6.5m,
                    PorcentajeTolerancia = 10.0m,
                    Descripcion = "Sedán compacto",
                    CreadoEn = baseTime,
                    ActualizadoEn = baseTime
                },
                new
                {
                    ModeloId = 2,
                    MarcaId = 2,
                    Nombre = "Civic",
                    Año = 2023,
                    TipoCombustible = TipoCombustible.Gasolina,
                    ConsumoEstandar = 7.0m,
                    PorcentajeTolerancia = 12.0m,
                    Descripcion = "Sedán deportivo",
                    CreadoEn = baseTime,
                    ActualizadoEn = baseTime
                },
                new
                {
                    ModeloId = 3,
                    MarcaId = 3,
                    Nombre = "Silverado",
                    Año = 2023,
                    TipoCombustible = TipoCombustible.Diesel,
                    ConsumoEstandar = 12.0m,
                    PorcentajeTolerancia = 15.0m,
                    Descripcion = "Camioneta pickup",
                    CreadoEn = baseTime,
                    ActualizadoEn = baseTime
                }
            );

            // Semilla de datos para Vehiculos (agregando vehículo de prueba)
            modelBuilder.Entity<Vehiculo>().HasData(
                new
                {
                    VehiculoId = 1,
                    Codigo = "VH-2024-001",
                    TipoId = 1, // Automóvil
                    ModeloId = 1, // Toyota Corolla
                    Placa = "ABC-123",
                    TipoMaquinaria = TipoMaquinaria.Ligera,
                    AñoFabricacion = 2023,
                    FechaCompra = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                    OdometroInicial = 0m,
                    OdometroActual = 1500m,
                    CapacidadCombustible = 45.0m,
                    CapacidadMotor = "1.8L",
                    Estado = EstadoVehiculo.Activo,
                    CreadoEn = baseTime,
                    ActualizadoEn = baseTime
                }
            );

            // Semilla de datos para Estados Operacionales del vehículo de prueba
            modelBuilder.Entity<EstadoOperacionalVehiculo>().HasData(
                new
                {
                    EstadoId = 1,
                    VehiculoId = 1,
                    Estado = EstadoVehiculo.Activo,
                    FechaInicio = baseTime,
                    Motivo = "Vehículo puesto en servicio inicial",
                    RegistradoPor = "Sistema",
                    CreadoEn = baseTime
                }
            );
        }

        #endregion
    }
}