using System.Linq;

namespace AerolineaVIP.Models
{
    public partial class Reserva
    {
        public bool TieneOperaciones
        {
            get { return ReservaPasajeroVueloes.Any() || Compras.Any(); }
        }

        public string ExpiracionTexto
        {
            get { return FechaExpiracion.HasValue ? FechaExpiracion.Value.ToString("dd/MM/yyyy HH:mm") : "Sin fecha de expiración"; }
        }

        public int CantidadPasajeros
        {
            get { return ReservaPasajeroVueloes.Select(a => a.PasajeroId).Distinct().Count(); }
        }

        public decimal ImporteVigente
        {
            get
            {
                return ReservaPasajeroVueloes
                    .Where(a => a.EstadoAsignacionCodigo == "PENDIENTE" || a.EstadoAsignacionCodigo == "CONFIRMADA")
                    .Sum(a => a.ImporteTramo);
            }
        }
    }
}
