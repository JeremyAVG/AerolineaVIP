namespace AerolineaVIP.Models
{
    public partial class vw_DisponibilidadVuelo
    {
        public string RutaTexto
        {
            get { return Origen + " → " + Destino; }
        }

        public string Horario
        {
            get { return HoraSalida.ToString(@"hh\:mm") + " – " + HoraLlegada.ToString(@"hh\:mm"); }
        }

        public decimal PorcentajeOcupacion
        {
            get { return Capacidad == 0 ? 0 : Ocupados * 100m / Capacidad; }
        }
    }
}
