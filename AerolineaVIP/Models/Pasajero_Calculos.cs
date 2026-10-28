using System.Linq;

namespace AerolineaVIP.Models
{
    public partial class Pasajero
    {
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
