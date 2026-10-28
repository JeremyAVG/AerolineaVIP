using System.Linq;

namespace AerolineaVIP.Models
{
    public partial class Compra
    {
        public Pago UltimoPago
        {
            get { return Pagoes.OrderByDescending(p => p.FechaPago).FirstOrDefault(); }
        }

        public string EstadoUltimoPago
        {
            get { return UltimoPago != null ? UltimoPago.EstadoPago.Descripcion : "Sin pagos"; }
        }

        public decimal TotalAprobado
        {
            get { return Pagoes.Where(p => p.EstadoPagoCodigo == "APROBADO").Sum(p => p.Importe); }
        }
    }
}
