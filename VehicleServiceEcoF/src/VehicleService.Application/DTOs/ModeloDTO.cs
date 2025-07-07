namespace VehicleService.Application.DTOs
{
    public class ModeloDTO
    {
        public int ModeloId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int MarcaId { get; set; }
        public string MarcaNombre { get; set; } = string.Empty;
        public MarcaDTO? Marca { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}
