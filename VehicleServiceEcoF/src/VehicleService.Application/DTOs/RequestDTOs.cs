using VehicleService.Domain.Enums;

namespace VehicleService.Application.DTOs
{
    public class CrearVehiculoRequest
    {
        public string Codigo { get; set; } = string.Empty;
        public int TipoId { get; set; }
        public int ModeloId { get; set; }
        public string Placa { get; set; } = string.Empty;
        public int TipoMaquinaria { get; set; } // 1=Ligera, 2=Pesada
        public int AñoFabricacion { get; set; }
        public DateTime FechaCompra { get; set; }
        public decimal OdometroInicial { get; set; }
        public decimal CapacidadCombustible { get; set; }
        public string CapacidadMotor { get; set; } = string.Empty;
    }

    public class ActualizarVehiculoRequest
    {
        public string? Codigo { get; set; }
        public int? TipoId { get; set; }
        public int? ModeloId { get; set; }
        public string? Placa { get; set; }
        public int? TipoMaquinaria { get; set; }
        public int? AñoFabricacion { get; set; }
        public DateTime? FechaCompra { get; set; }
        public decimal? CapacidadCombustible { get; set; }
        public string? CapacidadMotor { get; set; }
    }

    public class CambiarEstadoVehiculoRequest
    {
        public int EstadoVehiculo { get; set; } // 1=Activo, 2=Mantenimiento, etc.
        public string Motivo { get; set; } = string.Empty;
        public string RegistradoPor { get; set; } = string.Empty;
    }

    public class ActualizarOdometroRequest
    {
        public decimal NuevoOdometro { get; set; }
    }

    public class RegistrarMantenimientoRequest
    {
        public DateTime? FechaProximoMantenimiento { get; set; }
    }

    public class FiltroVehiculosRequest
    {
        public string? Codigo { get; set; }
        public string? Placa { get; set; }
        public int? TipoId { get; set; }
        public int? ModeloId { get; set; }
        public int? EstadoVehiculo { get; set; }
        public int? TipoMaquinaria { get; set; }
        public DateTime? FechaCompraDesde { get; set; }
        public DateTime? FechaCompraHasta { get; set; }
        public bool? RequiereMantenimiento { get; set; }
        public bool? MantenimientoVencido { get; set; }
        public int Pagina { get; set; } = 1;
        public int TamañoPagina { get; set; } = 10;
    }

    // Marca DTOs
    public class CrearMarcaRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }

    public class ActualizarMarcaRequest
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
    }

    // Modelo DTOs
    public class CrearModeloRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public int MarcaId { get; set; }
        public string? Descripcion { get; set; }
    }

    public class ActualizarModeloRequest
    {
        public string? Nombre { get; set; }
        public int? MarcaId { get; set; }
        public string? Descripcion { get; set; }
    }

    public class FiltroModelosRequest
    {
        public string? Nombre { get; set; }
        public int? MarcaId { get; set; }
        public int Pagina { get; set; } = 1;
        public int TamañoPagina { get; set; } = 10;
    }

    // TipoVehiculo DTOs
    public class CrearTipoVehiculoRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }

    public class ActualizarTipoVehiculoRequest
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
    }

    public class FiltroTiposVehiculoRequest
    {
        public string? Nombre { get; set; }
        public int Pagina { get; set; } = 1;
        public int TamañoPagina { get; set; } = 10;
    }

    // EstadoOperacional DTOs
    public class CrearEstadoOperacionalRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }

    public class ActualizarEstadoOperacionalRequest
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
    }

    // Validation DTOs
    public class VehiculoDataRequest
    {
        public string? Codigo { get; set; }
        public string? Placa { get; set; }
        public int? TipoId { get; set; }
        public int? ModeloId { get; set; }
    }

    // Report DTOs
    public class CustomReportRequest
    {
        public Dictionary<string, string> Filtros { get; set; } = new Dictionary<string, string>();
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }

    public class ReportFilterRequest
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string? Filtro { get; set; }
    }
}
