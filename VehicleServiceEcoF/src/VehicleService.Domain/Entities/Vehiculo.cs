using VehicleService.Domain.Enums;
using VehicleService.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace VehicleService.Domain.Entities
{
    public class Vehiculo
    {
        public int VehiculoId { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public int TipoId { get; private set; }
        public int ModeloId { get; private set; }
        public string Placa { get; private set; } = string.Empty;
        public TipoMaquinaria TipoMaquinaria { get; private set; }
        public int AñoFabricacion { get; private set; }
        public DateTime FechaCompra { get; private set; }
        public decimal OdometroInicial { get; private set; }
        public decimal OdometroActual { get; private set; }
        public decimal CapacidadCombustible { get; private set; }
        public string CapacidadMotor { get; private set; } = string.Empty;
        public EstadoVehiculo Estado { get; private set; }
        public DateTime? FechaUltimoMantenimiento { get; private set; }
        public DateTime? FechaProximoMantenimiento { get; private set; }
        public DateTime CreadoEn { get; private set; } = DateTime.UtcNow;
        public DateTime ActualizadoEn { get; private set; } = DateTime.UtcNow;

        // Navigation properties 
        public virtual TipoVehiculo Tipo { get; set; } = null!;
        public virtual Modelo Modelo { get; set; } = null!;
        public virtual ICollection<EstadoOperacionalVehiculo> EstadosOperacionales { get; private set; } = new List<EstadoOperacionalVehiculo>();

        // Constructor privado para EF
        private Vehiculo()
        {
        }

        // Constructor interno para reconstrucción desde persistencia
        internal Vehiculo(
            int vehiculoId,
            string codigo,
            int tipoId,
            int modeloId,
            string placa,
            TipoMaquinaria tipoMaquinaria,
            int añoFabricacion,
            DateTime fechaCompra,
            decimal odometroInicial,
            decimal odometroActual,
            decimal capacidadCombustible,
            string capacidadMotor,
            EstadoVehiculo estado,
            DateTime? fechaUltimoMantenimiento,
            DateTime? fechaProximoMantenimiento,
            DateTime creadoEn,
            DateTime actualizadoEn,
            ICollection<EstadoOperacionalVehiculo> estadosOperacionales)
        {
            VehiculoId = vehiculoId;
            Codigo = codigo;
            TipoId = tipoId;
            ModeloId = modeloId;
            Placa = placa;
            TipoMaquinaria = tipoMaquinaria;
            AñoFabricacion = añoFabricacion;
            FechaCompra = fechaCompra;
            OdometroInicial = odometroInicial;
            OdometroActual = odometroActual;
            CapacidadCombustible = capacidadCombustible;
            CapacidadMotor = capacidadMotor;
            Estado = estado;
            FechaUltimoMantenimiento = fechaUltimoMantenimiento;
            FechaProximoMantenimiento = fechaProximoMantenimiento;
            CreadoEn = creadoEn;
            ActualizadoEn = actualizadoEn;
            EstadosOperacionales = estadosOperacionales ?? new List<EstadoOperacionalVehiculo>();

        }

        // Constructor interno - solo se usará a través del Factory
        internal Vehiculo(
            string codigo,
            int tipoId,
            int modeloId,
            string placa,
            TipoMaquinaria tipoMaquinaria,
            int añoFabricacion,
            DateTime fechaCompra,
            decimal odometroInicial,
            decimal capacidadCombustible,
            string capacidadMotor)
        {
            SetCodigo(codigo);
            SetTipoId(tipoId);
            SetModeloId(modeloId);
            SetPlaca(placa);
            SetTipoMaquinaria(tipoMaquinaria);
            SetAñoFabricacion(añoFabricacion);
            SetFechaCompra(fechaCompra);
            SetOdometros(odometroInicial, odometroInicial);
            SetCapacidadCombustible(capacidadCombustible);
            SetCapacidadMotor(capacidadMotor);
            Estado = EstadoVehiculo.Activo;
        }

        // Métodos de validación y actualización
        public void SetCodigo(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                throw new InvalidVehicleDataException("Codigo", "El código no puede estar vacío");

            var regex = new Regex(@"^[A-Z0-9-]{3,20}$");
            if (!regex.IsMatch(codigo))
                throw new InvalidVehicleDataException("Codigo", "El código debe tener entre 3-20 caracteres alfanuméricos y guiones");

            Codigo = codigo;
            ActualizarFechaModificacion();
        }

        public void SetPlaca(string placa)
        {
            if (string.IsNullOrWhiteSpace(placa))
                throw new InvalidVehicleDataException("Placa", "La placa no puede estar vacía");

            var regex = new Regex(@"^[A-Z0-9-]{6,10}$");
            if (!regex.IsMatch(placa))
                throw new InvalidVehicleDataException("Placa", "La placa debe tener formato válido (6-10 caracteres)");

            Placa = placa;
            ActualizarFechaModificacion();
        }

        public void SetTipoId(int tipoId)
        {
            if (tipoId <= 0)
                throw new InvalidVehicleDataException("TipoId", "El tipo de vehículo debe ser válido");

            TipoId = tipoId;
            ActualizarFechaModificacion();
        }

        public void SetModeloId(int modeloId)
        {
            if (modeloId <= 0)
                throw new InvalidVehicleDataException("ModeloId", "El modelo debe ser válido");

            ModeloId = modeloId;
            ActualizarFechaModificacion();
        }

        public void SetTipoMaquinaria(TipoMaquinaria tipoMaquinaria)
        {
            if (!Enum.IsDefined(typeof(TipoMaquinaria), tipoMaquinaria))
                throw new InvalidVehicleDataException("TipoMaquinaria", "Tipo de maquinaria no válido");

            TipoMaquinaria = tipoMaquinaria;
            ActualizarFechaModificacion();
        }

        public void SetAñoFabricacion(int año)
        {
            var añoActual = DateTime.Now.Year;
            if (año < 1990 || año > añoActual + 1)
                throw new InvalidVehicleDataException("AñoFabricacion", $"El año debe estar entre 1990 y {añoActual + 1}");

            AñoFabricacion = año;
            ActualizarFechaModificacion();
        }

        public void SetFechaCompra(DateTime fechaCompra)
        {
            if (fechaCompra > DateTime.Now)
                throw new InvalidVehicleDataException("FechaCompra", "La fecha de compra no puede ser futura");

            FechaCompra = fechaCompra;
            ActualizarFechaModificacion();
        }

        public void SetOdometros(decimal odometroInicial, decimal odometroActual)
        {
            if (odometroInicial < 0)
                throw new InvalidVehicleDataException("OdometroInicial", "El odómetro inicial no puede ser negativo");

            if (odometroActual < 0)
                throw new InvalidVehicleDataException("OdometroActual", "El odómetro actual no puede ser negativo");

            if (odometroActual < odometroInicial)
                throw new InvalidVehicleDataException("OdometroActual", "El odómetro actual no puede ser menor al inicial");

            OdometroInicial = odometroInicial;
            OdometroActual = odometroActual;
            ActualizarFechaModificacion();
        }

        public void ActualizarOdometro(decimal nuevoOdometro)
        {
            if (nuevoOdometro < OdometroActual)
                throw new InvalidVehicleDataException("OdometroActual", "El nuevo odómetro no puede ser menor al actual");

            OdometroActual = nuevoOdometro;
            ActualizarFechaModificacion();
        }

        public void SetCapacidadCombustible(decimal capacidad)
        {
            if (capacidad <= 0)
                throw new InvalidVehicleDataException("CapacidadCombustible", "La capacidad debe ser mayor a cero");

            if (capacidad > 1000)
                throw new InvalidVehicleDataException("CapacidadCombustible", "La capacidad excede el límite permitido");

            CapacidadCombustible = capacidad;
            ActualizarFechaModificacion();
        }

        public void SetCapacidadMotor(string capacidadMotor)
        {
            if (string.IsNullOrWhiteSpace(capacidadMotor))
                throw new InvalidVehicleDataException("CapacidadMotor", "La capacidad del motor no puede estar vacía");

            var regex = new Regex(@"^[\d.,]+\s?(L|CC|HP|KW)$", RegexOptions.IgnoreCase);
            if (!regex.IsMatch(capacidadMotor))
                throw new InvalidVehicleDataException("CapacidadMotor", "Formato de capacidad del motor inválido (ej: 2.0L, 1500CC)");

            CapacidadMotor = capacidadMotor;
            ActualizarFechaModificacion();
        }

        // Métodos para cambio de estado
        public void CambiarEstado(EstadoVehiculo nuevoEstado)
        {
            if (!PuedeCambiarEstado(Estado, nuevoEstado))
                throw new InvalidVehicleStateException(Estado.ToString(), nuevoEstado.ToString());

            Estado = nuevoEstado;
            ActualizarFechaModificacion();
        }

        public void CambiarEstadoConHistorial(EstadoVehiculo nuevoEstado, string motivo, string registradoPor)
        {
            if (!PuedeCambiarEstado(Estado, nuevoEstado))
                throw new InvalidVehicleStateException(Estado.ToString(), nuevoEstado.ToString());

            // Finalizar el estado actual si existe uno activo
            var estadoActual = EstadosOperacionales.FirstOrDefault(e => e.FechaFin == null);
            if (estadoActual != null)
            {
                estadoActual.FinalizarEstado(DateTime.UtcNow);
            }

            // Cambiar el estado del vehículo
            Estado = nuevoEstado;
            
            // Crear nuevo registro de estado operacional
            var nuevoEstadoOperacional = EstadoOperacionalVehiculo.CrearCambioEstado(
                VehiculoId, nuevoEstado, motivo, registradoPor);
            
            EstadosOperacionales.Add(nuevoEstadoOperacional);
            ActualizarFechaModificacion();
        }

        public void ActivarVehiculo()
        {
            if (Estado == EstadoVehiculo.Activo)
                throw new InvalidVehicleStateException("El vehículo ya está activo");

            if (Estado == EstadoVehiculo.Reparacion)
                throw new InvalidVehicleStateException("No se puede activar un vehículo en reparación directamente");

            CambiarEstado(EstadoVehiculo.Activo);
        }

        public void EnviarAMantenimiento()
        {
            if (Estado == EstadoVehiculo.Mantenimiento)
                throw new InvalidVehicleStateException("El vehículo ya está en mantenimiento");

            if (Estado == EstadoVehiculo.Reparacion)
                throw new InvalidVehicleStateException("No se puede enviar a mantenimiento un vehículo en reparación");

            CambiarEstado(EstadoVehiculo.Mantenimiento);
        }

        public void EnviarAReparacion()
        {
            CambiarEstado(EstadoVehiculo.Reparacion);
        }

        public void Inactivar()
        {
            if (Estado == EstadoVehiculo.Reservado)
                throw new InvalidVehicleStateException("No se puede inactivar un vehículo reservado");

            CambiarEstado(EstadoVehiculo.Inactivo);
        }

        public void Reservar()
        {
            if (Estado != EstadoVehiculo.Activo)
                throw new InvalidVehicleStateException("Solo se pueden reservar vehículos activos");

            CambiarEstado(EstadoVehiculo.Reservado);
        }

        public void LiberarReserva()
        {
            if (Estado != EstadoVehiculo.Reservado)
                throw new InvalidVehicleStateException("El vehículo no está reservado");

            CambiarEstado(EstadoVehiculo.Activo);
        }

        // Métodos para mantenimiento
        public void RegistrarMantenimiento()
        {
            FechaUltimoMantenimiento = DateTime.UtcNow;
            FechaProximoMantenimiento = DateTime.UtcNow.AddMonths(3);
            ActualizarFechaModificacion();
        }

        public void EstablecerProximoMantenimiento(DateTime fechaProximo)
        {
            if (fechaProximo <= DateTime.Now)
                throw new InvalidVehicleDataException("FechaProximoMantenimiento", "La fecha del próximo mantenimiento debe ser futura");

            FechaProximoMantenimiento = fechaProximo;
            ActualizarFechaModificacion();
        }

        // Método para verificar si puede cambiar de estado
        private bool PuedeCambiarEstado(EstadoVehiculo estadoActual, EstadoVehiculo estadoNuevo)
        {
            return estadoActual switch
            {
                EstadoVehiculo.Activo => estadoNuevo != EstadoVehiculo.Activo,
                EstadoVehiculo.Mantenimiento => estadoNuevo == EstadoVehiculo.Activo || estadoNuevo == EstadoVehiculo.Reparacion || estadoNuevo == EstadoVehiculo.Inactivo,
                EstadoVehiculo.Reparacion => estadoNuevo == EstadoVehiculo.Activo || estadoNuevo == EstadoVehiculo.Inactivo,
                EstadoVehiculo.Inactivo => estadoNuevo == EstadoVehiculo.Activo,
                EstadoVehiculo.Reservado => estadoNuevo == EstadoVehiculo.Activo || estadoNuevo == EstadoVehiculo.Inactivo,
                _ => false
            };
        }

        private void ActualizarFechaModificacion() => ActualizadoEn = DateTime.UtcNow;
        public void RegistrarMantenimiento(DateTime fechaProximoMantenimiento)
        {
            FechaUltimoMantenimiento = DateTime.UtcNow;
            if (fechaProximoMantenimiento <= DateTime.UtcNow)
                throw new InvalidVehicleDataException("FechaProximoMantenimiento", "La fecha del próximo mantenimiento debe ser futura");
            FechaProximoMantenimiento = fechaProximoMantenimiento;
            ActualizarFechaModificacion();
        }
    }

}