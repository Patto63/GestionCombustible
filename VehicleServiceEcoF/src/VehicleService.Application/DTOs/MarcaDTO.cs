namespace VehicleService.Application.DTOs
{
    public class MarcaDTO
    {
        public int MarcaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}
