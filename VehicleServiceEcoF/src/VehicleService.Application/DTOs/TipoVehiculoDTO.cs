namespace VehicleService.Application.DTOs
{
    public class TipoVehiculoDTO
    {
        public int TipoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}
