using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AerolineaVIP.Models
{
    public class GenerarVuelosViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Indique la fecha de inicio.")]
        [DataType(DataType.Date)]
        [Display(Name = "Desde")]
        public DateTime? FechaInicio { get; set; }

        [Required(ErrorMessage = "Indique la fecha de fin.")]
        [DataType(DataType.Date)]
        [Display(Name = "Hasta")]
        public DateTime? FechaFin { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FechaInicio.HasValue && FechaFin.HasValue)
            {
                if (FechaFin < FechaInicio)
                {
                    yield return new ValidationResult("La fecha de fin no puede ser anterior a la de inicio.", new[] { "FechaFin" });
                }
                else if ((FechaFin.Value - FechaInicio.Value).TotalDays > 365)
                {
                    yield return new ValidationResult("El rango no puede superar 366 días.", new[] { "FechaFin" });
                }
            }
        }
    }

    public class FiltroVuelosViewModel
    {
        [DataType(DataType.Date)]
        [Display(Name = "Desde")]
        public DateTime? Desde { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Hasta")]
        public DateTime? Hasta { get; set; }

        [Display(Name = "Ruta")]
        public int? RutaId { get; set; }

        [Display(Name = "Estado")]
        public string Estado { get; set; }

        public List<Vuelo> Vuelos { get; set; }
        public Dictionary<int, vw_DisponibilidadVuelo> Disponibilidad { get; set; }
        public GenerarVuelosViewModel Generar { get; set; }

        public string TextoDisponibles(int vueloId)
        {
            if (Disponibilidad == null || !Disponibilidad.ContainsKey(vueloId) || Disponibilidad[vueloId].Capacidad == 0)
            {
                return "Sin aeronave";
            }
            var d = Disponibilidad[vueloId];
            return d.Disponibles + " de " + d.Capacidad;
        }
    }

    public class UsuarioGestion
    {
        public IEnumerable<System.Web.Mvc.SelectListItem> ListaRoles
        {
            get { return RolesSistema.Todos.Select(r => new System.Web.Mvc.SelectListItem { Value = r, Text = r, Selected = r == Rol }); }
        }
        public string Id { get; set; }
        public string UserName { get; set; }
        public string NombreCompleto { get; set; }
        public string Email { get; set; }
        public string Rol { get; set; }
        public bool Bloqueado { get; set; }

        public string EstadoTexto
        {
            get { return Bloqueado ? "Bloqueado" : "Activo"; }
        }
    }

    public class GestionViewModel
    {
        public string Pestana { get; set; }
        public string Buscar { get; set; }
        public string FiltroRol { get; set; }
        public string FiltroEstado { get; set; }
        public bool PuedeAsignarRoles { get; set; }
        public string UsuarioActualId { get; set; }

        public List<UsuarioGestion> Usuarios { get; set; }
        public List<Pasajero> Pasajeros { get; set; }
        public List<Reserva> Reservas { get; set; }
        public Dictionary<string, string> NombresUsuario { get; set; }

        public string ClasePestana(string pestana)
        {
            return Pestana == pestana ? "nav-link active" : "nav-link";
        }

        public string NombreUsuario(string usuarioId)
        {
            return NombresUsuario != null && NombresUsuario.ContainsKey(usuarioId) ? NombresUsuario[usuarioId] : "(usuario no encontrado)";
        }
    }

    public class ReporteOcupacionViewModel
    {
        public ReporteOcupacionViewModel()
        {
            Filas = new List<vw_DisponibilidadVuelo>();
        }

        [DataType(DataType.Date)]
        [Display(Name = "Desde")]
        public DateTime? Desde { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Hasta")]
        public DateTime? Hasta { get; set; }

        [Display(Name = "Ruta")]
        public int? RutaId { get; set; }

        [Display(Name = "Aeronave")]
        public int? AeronaveId { get; set; }

        [Display(Name = "Estado del vuelo")]
        public string Estado { get; set; }

        public DateTime FechaCorte { get; set; }
        public List<vw_DisponibilidadVuelo> Filas { get; set; }

        public int TotalCapacidad
        {
            get { return Filas.Sum(f => f.Capacidad); }
        }

        public int TotalOcupados
        {
            get { return Filas.Sum(f => f.Ocupados); }
        }

        public int TotalDisponibles
        {
            get { return Filas.Sum(f => f.Disponibles ?? 0); }
        }

        public decimal PorcentajeGeneral
        {
            get { return TotalCapacidad == 0 ? 0 : TotalOcupados * 100m / TotalCapacidad; }
        }
    }

    public class ReservasPorEstado
    {
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
        public int Cantidad { get; set; }
        public int Asientos { get; set; }
        public decimal Importe { get; set; }
    }

    public class ReporteReservasViewModel
    {
        public ReporteReservasViewModel()
        {
            Filas = new List<ReservasPorEstado>();
        }

        [DataType(DataType.Date)]
        [Display(Name = "Creadas desde")]
        public DateTime? Desde { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Creadas hasta")]
        public DateTime? Hasta { get; set; }

        public DateTime FechaCorte { get; set; }
        public List<ReservasPorEstado> Filas { get; set; }

        public int TotalReservas
        {
            get { return Filas.Sum(f => f.Cantidad); }
        }

        public int TotalAsientos
        {
            get { return Filas.Sum(f => f.Asientos); }
        }
    }

    public class PagosPorEstado
    {
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
        public int Cantidad { get; set; }
        public decimal Total { get; set; }
    }

    public class ReporteComprasViewModel
    {
        public ReporteComprasViewModel()
        {
            Compras = new List<Compra>();
            ResumenPagos = new List<PagosPorEstado>();
        }

        [DataType(DataType.Date)]
        [Display(Name = "Desde")]
        public DateTime? Desde { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Hasta")]
        public DateTime? Hasta { get; set; }

        [Display(Name = "Estado de la compra")]
        public string EstadoCompra { get; set; }

        public DateTime FechaCorte { get; set; }
        public List<Compra> Compras { get; set; }
        public List<PagosPorEstado> ResumenPagos { get; set; }

        public decimal TotalCompras
        {
            get { return Compras.Sum(c => c.Importe); }
        }

        public decimal TotalAprobado
        {
            get { return Compras.Sum(c => c.TotalAprobado); }
        }
    }
}
