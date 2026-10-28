using System.Linq;

namespace AerolineaVIP.Models
{
    public partial class Pasajero
    {
        public static System.Collections.Generic.IEnumerable<System.Web.Mvc.SelectListItem> TiposDocumento(string seleccionado)
        {
            return new[] {
                new System.Web.Mvc.SelectListItem { Value = "CEDULA", Text = "Cédula", Selected = seleccionado == "CEDULA" },
                new System.Web.Mvc.SelectListItem { Value = "PASAPORTE", Text = "Pasaporte", Selected = seleccionado == "PASAPORTE" }
            };
        }
        public static string NombreTipoDocumento(string codigo)
        {
            return codigo == "CEDULA" ? "Cédula" : codigo == "PASAPORTE" ? "Pasaporte" : "Documento";
        }
        public string DocumentoTexto { get { return NombreTipoDocumento(TipoDocumentoCodigo) + " " + NumeroDocumento; } }
        public string NombreCompleto
        {
            get { return Nombres + " " + Apellidos; }
        }

        public bool TieneOperaciones
        {
            get { return ReservaPasajeroVueloes.Any(); }
        }

        public string ReservasTexto
        {
            get { return TieneOperaciones ? "Con reservas" : "Sin reservas"; }
        }
    }
}
