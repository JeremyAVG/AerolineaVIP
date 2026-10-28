using System.Linq;

namespace AerolineaVIP.Models
{
    public partial class Vuelo
    {
        public string CodigoVuelo
        {
            get { return "VIP" + VueloId.ToString("00000"); }
        }

        public string Descripcion
        {
            get { return CodigoVuelo + " · " + FechaOperacion.ToString("dd/MM/yyyy") + " · " + Frecuencia.Ruta.Descripcion + " " + Frecuencia.Horario; }
        }

        public string AeronaveTexto
        {
            get { return Aeronave == null ? "Sin asignar" : Aeronave.Matricula; }
        }

        public bool TieneReservas
        {
            get { return ReservaPasajeroVueloes.Any(); }
        }
    }
}
