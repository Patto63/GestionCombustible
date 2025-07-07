using VehicleService.Domain.Enums;

namespace VehicleService.Application.DTOs
{
    public class VehiculoResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public VehiculoDTO? Vehiculo { get; set; }
    }

    public class VehiculosResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public IEnumerable<VehiculoDTO> Vehiculos { get; set; } = new List<VehiculoDTO>();
    }

    public class VehiculosPagedResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public IEnumerable<VehiculoDTO> Vehiculos { get; set; } = new List<VehiculoDTO>();
        public int TotalRegistros { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public bool TienePaginaAnterior { get; set; }
        public bool TienePaginaSiguiente { get; set; }
    }

    public class VehiculoDetalleResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public VehiculoDetalleDTO? VehiculoDetalle { get; set; }
    }

    public class OperationResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }

    // Marca Response DTOs
    public class MarcaResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public MarcaDTO? Marca { get; set; }
    }

    public class MarcasResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public IEnumerable<MarcaDTO> Marcas { get; set; } = new List<MarcaDTO>();
    }

    // Modelo Response DTOs
    public class ModeloResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public ModeloDTO? Modelo { get; set; }
    }

    public class ModelosResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public IEnumerable<ModeloDTO> Modelos { get; set; } = new List<ModeloDTO>();
    }

    public class ModelosPagedResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public IEnumerable<ModeloDTO> Modelos { get; set; } = new List<ModeloDTO>();
        public int TotalRegistros { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public bool TienePaginaAnterior { get; set; }
        public bool TienePaginaSiguiente { get; set; }
    }

    // TipoVehiculo Response DTOs
    public class TipoVehiculoResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public TipoVehiculoDTO? TipoVehiculo { get; set; }
    }

    public class TiposVehiculoResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public IEnumerable<TipoVehiculoDTO> TiposVehiculo { get; set; } = new List<TipoVehiculoDTO>();
    }

    public class TiposVehiculoPagedResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public IEnumerable<TipoVehiculoDTO> TiposVehiculo { get; set; } = new List<TipoVehiculoDTO>();
        public int TotalRegistros { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public bool TienePaginaAnterior { get; set; }
        public bool TienePaginaSiguiente { get; set; }
    }

    // EstadoOperacional Response DTOs
    public class EstadoOperacionalResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public EstadoOperacionalDTO? EstadoOperacional { get; set; }
    }

    public class EstadosOperacionalesResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public IEnumerable<EstadoOperacionalDTO> EstadosOperacionales { get; set; } = new List<EstadoOperacionalDTO>();
    }

    // Report Response DTOs
    public class VehicleStatusReportResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public IEnumerable<VehiculoDTO> Vehiculos { get; set; } = new List<VehiculoDTO>();
        public Dictionary<string, int> Estadisticas { get; set; } = new Dictionary<string, int>();
    }

    public class VehicleStatusDetailReportResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public IEnumerable<VehiculoDetalleDTO> VehiculosDetalle { get; set; } = new List<VehiculoDetalleDTO>();
    }

    public class MaintenanceReportResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public IEnumerable<VehiculoDTO> VehiculosMantenimiento { get; set; } = new List<VehiculoDTO>();
        public Dictionary<string, object> Estadisticas { get; set; } = new Dictionary<string, object>();
    }

    public class VehiclesMaintenanceDueReportResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public IEnumerable<VehiculoDTO> VehiculosVencidos { get; set; } = new List<VehiculoDTO>();
        public int TotalVencidos { get; set; }
    }

    public class VehicleUsageReportResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public IEnumerable<dynamic> DatosUso { get; set; } = new List<dynamic>();
        public Dictionary<string, object> Estadisticas { get; set; } = new Dictionary<string, object>();
    }

    public class VehicleUsageDetailReportResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public IEnumerable<VehiculoDetalleDTO> DetallesUso { get; set; } = new List<VehiculoDetalleDTO>();
    }

    public class VehicleInventoryReportResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public IEnumerable<VehiculoDTO> Inventario { get; set; } = new List<VehiculoDTO>();
        public Dictionary<string, int> Totales { get; set; } = new Dictionary<string, int>();
    }

    public class VehicleInventoryByTypeReportResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public Dictionary<string, IEnumerable<VehiculoDTO>> InventarioPorTipo { get; set; } = new Dictionary<string, IEnumerable<VehiculoDTO>>();
    }

    public class VehicleInventoryByBrandReportResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public Dictionary<string, IEnumerable<VehiculoDTO>> InventarioPorMarca { get; set; } = new Dictionary<string, IEnumerable<VehiculoDTO>>();
    }

    public class OdometerReportResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public IEnumerable<dynamic> DatosOdometro { get; set; } = new List<dynamic>();
        public Dictionary<string, object> Estadisticas { get; set; } = new Dictionary<string, object>();
    }

    public class VehicleOdometerDetailReportResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public IEnumerable<VehiculoDetalleDTO> DetallesOdometro { get; set; } = new List<VehiculoDetalleDTO>();
    }

    public class VehicleAvailabilityReportResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public IEnumerable<VehiculoDTO> VehiculosDisponibles { get; set; } = new List<VehiculoDTO>();
        public Dictionary<string, int> EstadisticasDisponibilidad { get; set; } = new Dictionary<string, int>();
    }

    public class CustomVehicleReportResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public IEnumerable<dynamic> Datos { get; set; } = new List<dynamic>();
        public Dictionary<string, object> Metadatos { get; set; } = new Dictionary<string, object>();
    }

    public class VehicleCostReportResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public IEnumerable<dynamic> DatosCostos { get; set; } = new List<dynamic>();
        public Dictionary<string, decimal> TotalesCostos { get; set; } = new Dictionary<string, decimal>();
    }

    public class VehicleCostDetailReportResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public IEnumerable<dynamic> DetallesCostos { get; set; } = new List<dynamic>();
    }
}
