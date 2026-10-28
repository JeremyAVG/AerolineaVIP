using System.Linq;

namespace AerolineaVIP.Models
{
    public partial class Pasajero
    {
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
