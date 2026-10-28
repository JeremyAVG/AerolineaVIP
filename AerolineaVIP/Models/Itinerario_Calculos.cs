using System.Collections.Generic;
using System.Linq;

namespace AerolineaVIP.Models
{
    public partial class Itinerario
    {
        public List<TramoItinerario> TramosOrdenados
        {
            get { return TramoItinerarios.OrderBy(t => t.Orden).ToList(); }
        }

        public string Recorrido
        {
            get
            {
                var tramos = TramosOrdenados;
                if (!tramos.Any())
                {
                    return "Sin tramos";
                }
                var ciudades = new List<string> { tramos.First().Vuelo.Frecuencia.Ruta.AeropuertoOrigen.CodigoIATA };
                ciudades.AddRange(tramos.Select(t => t.Vuelo.Frecuencia.Ruta.AeropuertoDestino.CodigoIATA));
                return string.Join(" → ", ciudades);
            }
        }
    }
}
