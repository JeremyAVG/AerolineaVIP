using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace AerolineaVIP.Models
{
    public class AsistenciaViewModel
    {
        [Required, StringLength(20, MinimumLength = 5), RegularExpression("^[A-Za-z0-9]+$"), Display(Name = "Código de reserva o documento del pasajero")]
        public string Buscar { get; set; }
        public List<ReservaAsistidaFila> Reservas { get; set; } = new List<ReservaAsistidaFila>();
        public string Mensaje { get; set; }
    }
    public class ReservaAsistidaFila
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Documentos { get; set; }
        public string DocumentoConsulta { get; set; }
        public string Estado { get; set; }
        public bool PuedePagar { get; set; }
    }
    public class NuevaCompraClienteViewModel
    {
        [Required, StringLength(256), Display(Name = "Nombre de usuario o correo del cliente")]
        public string Cliente { get; set; }
    }
    [Serializable]
    public class ClienteAsistido
    {
        public string UsuarioId { get; set; }
        public string NombreUsuario { get; set; }
        public string OperadorId { get; set; }
    }
    [Serializable]
    public class AccesoAsistencia
    {
        public string OperadorId { get; set; }
        public List<int> Reservas { get; set; } = new List<int>();
    }
    public class FiltroCliente : IValidatableObject
    {
        [DataType(DataType.Date)] public DateTime? Desde { get; set; }
        [DataType(DataType.Date)] public DateTime? Hasta { get; set; }
        [Range(1, int.MaxValue)] public int Pagina { get; set; } = 1;
        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            if (Desde.HasValue && Hasta.HasValue && Desde > Hasta)
                yield return new ValidationResult("La fecha desde debe ser anterior o igual a la fecha hasta.");
        }
    }
    public class PaginaCliente
    {
        public int Numero { get; set; }
        public string Clase { get; set; }
        public string Actual { get; set; }
    }
    public class MisReservasViewModel : FiltroCliente
    {
        [RegularExpression("^(PENDIENTE|CONFIRMADA|CANCELADA|EXPIRADA)$")]
        public string Estado { get; set; }
        public List<SelectListItem> Estados { get; set; } = new List<SelectListItem>();
        public List<ReservaClienteFila> Reservas { get; set; } = new List<ReservaClienteFila>();
        public List<PaginaCliente> Paginas { get; set; } = new List<PaginaCliente>();
        public int Total { get; set; }
    }
    public class ReservaClienteFila
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Fecha { get; set; }
        public string Documentos { get; set; }
        public string Ruta { get; set; }
        public EstadoCompraVisual Reserva { get; set; }
        public EstadoCompraVisual Compra { get; set; }
        public bool PuedePagar { get; set; }
        public bool PuedeCancelar { get; set; }
    }
    public class GestionReservaViewModel
    {
        public FinalCompra Detalle { get; set; }
        public bool PuedeCancelar { get; set; }
        public string Motivo { get; set; }
        public string Reembolso { get; set; }
        public string Advertencia { get; set; }
        public int ReservaId { get; set; }
        [DebeSerVerdadero(ErrorMessage = "Confirme que acepta cancelar la reserva completa.")]
        public bool ConfirmarCancelacion { get; set; }
    }
    public class BusquedaCheckInViewModel
    {
        [Required, RegularExpression("^RV[0-9]{8}$"), Display(Name = "Código de reserva")]
        public string CodigoReserva { get; set; }
        [Required, RegularExpression("^[A-Za-z0-9]{5,20}$"), Display(Name = "Documento del pasajero")]
        public string Documento { get; set; }
        public List<int> Seleccionados { get; set; } = new List<int>();
        public List<CheckInFila> Filas { get; set; } = new List<CheckInFila>();
        public string Mensaje { get; set; }
        public bool PuedeAnular { get; set; }
    }
    public class CheckInFila
    {
        public int AsignacionId { get; set; }
        public int? CheckInId { get; set; }
        public string Pasajero { get; set; }
        public string Documento { get; set; }
        public string Tramo { get; set; }
        public string Asiento { get; set; }
        public string Estado { get; set; }
        public bool Elegible { get; set; }
        public bool Anulable { get; set; }
    }
    public class ComprobantesViewModel : FiltroCliente
    {
        [RegularExpression("^(CONFIRMACION|FACTURA|NOTA_CREDITO)$")]
        public string Tipo { get; set; }
        public List<SelectListItem> Tipos { get; set; } = new List<SelectListItem>();
        public List<ComprobanteVista> Comprobantes { get; set; } = new List<ComprobanteVista>();
        public List<PaginaCliente> Paginas { get; set; } = new List<PaginaCliente>();
        public int Total { get; set; }
    }
    public class ComprobanteVista
    {
        public string Documentos { get; set; }
        public const string Leyenda = "Comprobante académico de una operación simulada.";
        public int Id { get; set; }
        public string Numero { get; set; }
        public string Tipo { get; set; }
        public string Estado { get; set; }
        public string Fecha { get; set; }
        public string Emisor { get; set; }
        public string Identificacion { get; set; }
        public string Direccion { get; set; }
        public string CodigoReserva { get; set; }
        public string Importe { get; set; }
        public string Metodo { get; set; }
        public string Operacion { get; set; }
        public List<FilaCompra> Filas { get; set; } = new List<FilaCompra>();
    }
}
