using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AerolineaVIP.Models
{
    public class OpcionVuelo
    {
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public string Horario { get; set; }
        public decimal Tarifa { get; set; }
        public int Disponibles { get; set; }
        public int Pasajeros { get; set; }
        public List<int> Vuelos { get; set; }
    }
    public class ResultadosCompra
    {
        public List<OpcionVuelo> Opciones { get; set; } = new List<OpcionVuelo>();
    }
    [Serializable]
    public class SeleccionCompra
    {
        public string UsuarioId { get; set; }
        public string ClienteId { get; set; }
        public List<int> Vuelos { get; set; } = new List<int>();
        public int Cantidad { get; set; }
        public List<List<byte>> Asientos { get; set; } = new List<List<byte>>();
        public List<PasajeroCompra> Pasajeros { get; set; } = new List<PasajeroCompra>();
        public Guid Clave { get; set; } = Guid.NewGuid();
        public int? ReservaId { get; set; }
    }
    [Serializable]
    public class PasajeroCompra : IValidatableObject
    {
        [Required, RegularExpression("^(CEDULA|PASAPORTE)$")]
        public string TipoDocumento { get; set; }
        [Required, RegularExpression("^[A-Za-z0-9]{5,20}$")]
        public string Documento { get; set; }
        [Required, StringLength(100, MinimumLength = 2), RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ ]+$")]
        public string Nombres { get; set; }
        [Required, StringLength(100, MinimumLength = 2), RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ ]+$")]
        public string Apellidos { get; set; }
        [DataType(DataType.Date)]
        public DateTime? FechaNacimiento { get; set; }
        [EmailAddress, StringLength(256)]
        public string Correo { get; set; }
        [RegularExpression("^[0-9+ ]{7,20}$")]
        public string Telefono { get; set; }
        public string Titulo { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            if (FechaNacimiento.HasValue && FechaNacimiento.Value.Date > DateTime.Today)
                yield return new ValidationResult("La fecha de nacimiento no puede ser futura.", new[] { "FechaNacimiento" });
            if (TipoDocumento == "CEDULA" && !System.Text.RegularExpressions.Regex.IsMatch(Documento ?? "", "^[0-9]{10}$"))
                yield return new ValidationResult("La cédula debe tener 10 dígitos.", new[] { "Documento" });
        }
    }
    public class PasajerosCompra
    {
        public List<PasajeroCompra> Personas { get; set; } = new List<PasajeroCompra>();
        [StringLength(256)]
        public string UsuarioCliente { get; set; }
        public bool EsCounter { get; set; }
    }
    public class AsientoCompra
    {
        public byte Numero { get; set; }
        public string Codigo { get; set; }
        public string Estado { get; set; }
        public string Color { get; set; }
        public string Posicion { get; set; }
        public bool Disponible { get; set; }
        public bool Deshabilitado { get; set; }
        public string Clase { get; set; } = "asiento-phenom";
        public string SeleccionadoTexto { get; set; } = "false";
        public string DescripcionAccesible { get; set; }
    }
    public class MapaCompra
    {
        public int VueloId { get; set; }
        public string Titulo { get; set; }
        public bool Seleccionable { get; set; }
        public string SeleccionTexto { get; set; } = "";
        public List<AsientoCompra> Asientos { get; set; } = new List<AsientoCompra>();
        public string Codigo { get; set; }
        public string Ruta { get; set; }
        public string Fecha { get; set; }
        public string Horario { get; set; }
        public string Aeronave { get; set; }
        public int Disponibles { get; set; }
        public int Reservados { get; set; }
        public int Ocupados { get; set; }
    }
    public class ResumenCompra
    {
        public SeleccionCompra Seleccion { get; set; }
        public List<MapaCompra> Mapas { get; set; } = new List<MapaCompra>();
        public decimal Total { get; set; }
        public List<string> Detalles { get; set; } = new List<string>();
        public List<FilaCompra> Filas { get; set; } = new List<FilaCompra>();
        [DebeSerVerdadero(ErrorMessage = "Debe aceptar las condiciones de la reserva.")]
        public bool AceptaCondiciones { get; set; }
    }
    public class ConfirmacionCompra
    {
        public Guid Clave { get; set; }
        [DebeSerVerdadero(ErrorMessage = "Debe aceptar las condiciones de la reserva.")]
        public bool AceptaCondiciones { get; set; }
        [RegularExpression("^(reserva|pagar)$"), Required]
        public string Destino { get; set; }
    }
    public class PasoCompra
    {
        public string Texto { get; set; }
        public string Clase { get; set; }
        public string Actual { get; set; }
    }
    public class DisponibilidadCompra
    {
        public bool SinCupos { get; set; }
        public OpcionVuelo Opcion { get; set; }
        public List<MapaCompra> Mapas { get; set; }
        public string Calculado { get; set; }
        public int OrigenId { get; set; }
        public int DestinoId { get; set; }
        public string Fecha { get; set; }
    }
    public class FilaCompra
    {
        public string Tramo { get; set; }
        public string Ruta { get; set; }
        public string Fecha { get; set; }
        public string Horario { get; set; }
        public string Pasajero { get; set; }
        public string Documento { get; set; }
        public string Asiento { get; set; }
        public string Importe { get; set; }
    }
    public class EstadoCompraVisual
    {
        public string Etiqueta { get; set; }
        public string Texto { get; set; }
        public string Clase { get; set; }
    }
    public class FinalCompra
    {
        public int ReservaId { get; set; }
        public string Codigo { get; set; }
        public string Mensaje { get; set; }
        public string ClaseMensaje { get; set; }
        public string Total { get; set; }
        public string Metodo { get; set; }
        public string Operacion { get; set; }
        public string Expiracion { get; set; }
        public bool Pagada { get; set; }
        public bool Reintentar { get; set; }
        public bool PuedePagar { get; set; }
        public List<EstadoCompraVisual> Estados { get; set; } = new List<EstadoCompraVisual>();
        public List<FilaCompra> Filas { get; set; } = new List<FilaCompra>();
        public List<Comprobante> Comprobantes { get; set; } = new List<Comprobante>();
    }
    public class PagoCompra : IValidatableObject
    {
        public int ReservaId { get; set; }
        [Required, RegularExpression("^(VISA|MASTERCARD|AMEX|DINERS)$")]
        public string Marca { get; set; }
        [Required, RegularExpression("^[0-9]{14,16}$"), DataType(DataType.CreditCard)]
        public string Numero { get; set; }
        [Required, StringLength(100, MinimumLength = 2)]
        public string Titular { get; set; }
        [Required, RegularExpression("^(TARJETA_CREDITO|TARJETA_DEBITO)$")]
        public string Metodo { get; set; }
        [Required, RegularExpression("^(0[1-9]|1[0-2])/[0-9]{2}$", ErrorMessage = "Use MM/AA con un mes entre 01 y 12.")]
        public string Vencimiento { get; set; }
        [Required, RegularExpression("^[0-9]{3,4}$", ErrorMessage = "Ingrese solo 3 dígitos, o 4 para AMEX.")]
        public string CodigoSeguridad { get; set; }
        public Reserva Reserva { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            int longitud = Marca == "AMEX" ? 15 : Marca == "DINERS" ? 14 : 16;
            if (Numero != null && Numero.Length != longitud)
                yield return new ValidationResult("La longitud no corresponde a la marca seleccionada.", new[] { "Numero" });
            int longitudCodigo = Marca == "AMEX" ? 4 : 3;
            if (CodigoSeguridad != null && CodigoSeguridad.Length != longitudCodigo)
                yield return new ValidationResult("El código de seguridad debe tener " + longitudCodigo + " dígitos.", new[] { "CodigoSeguridad" });
            if (Vencimiento != null && System.Text.RegularExpressions.Regex.IsMatch(Vencimiento, "^(0[1-9]|1[0-2])/[0-9]{2}$"))
            {
                var fecha = new DateTime(2000 + int.Parse(Vencimiento.Substring(3)), int.Parse(Vencimiento.Substring(0, 2)), 1);
                if (fecha < new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1))
                    yield return new ValidationResult("La tarjeta está vencida.", new[] { "Vencimiento" });
            }
        }
    }
}
