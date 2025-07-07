using VehicleService.Domain.Enums;

namespace VehicleService.Application.DTOs
{
    public class EstadoOperacionalDTO
    {
        public int EstadoId { get; set; }
        public int VehiculoId { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string RegistradoPor { get; set; } = string.Empty;
        public DateTime CreadoEn { get; set; }
    }
}
