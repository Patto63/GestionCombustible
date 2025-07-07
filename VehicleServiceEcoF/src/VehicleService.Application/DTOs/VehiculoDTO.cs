using VehicleService.Domain.Enums;

namespace VehicleService.Application.DTOs
{
    public class VehiculoDTO
    {
        public int VehiculoId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public string TipoMaquinaria { get; set; } = string.Empty;
        public int AñoFabricacion { get; set; }
        public DateTime FechaCompra { get; set; }
        public decimal OdometroInicial { get; set; }
        public decimal OdometroActual { get; set; }
        public decimal CapacidadCombustible { get; set; }
        public string CapacidadMotor { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime? FechaUltimoMantenimiento { get; set; }
        public DateTime? FechaProximoMantenimiento { get; set; }
        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn { get; set; }

        // Información relacionada
        public TipoVehiculoDTO? Tipo { get; set; }
        public ModeloDTO? Modelo { get; set; }
        public EstadoOperacionalDTO? EstadoActual { get; set; }
    }

    public class VehiculoDetalleDTO : VehiculoDTO
    {
        public IEnumerable<EstadoOperacionalDTO> HistorialEstados { get; set; } = new List<EstadoOperacionalDTO>();
        public bool RequiereMantenimiento => FechaProximoMantenimiento.HasValue &&
                                            FechaProximoMantenimiento.Value <= DateTime.Now.AddDays(7);
        public bool MantenimientoVencido => FechaProximoMantenimiento.HasValue &&
                                           FechaProximoMantenimiento.Value < DateTime.Now;
        public decimal KilometrajeRecorrido => OdometroActual - OdometroInicial;
    }
}
