using System.Collections.Generic;

namespace AerolineaVIP.Models
{
    public class DestinoDisponible
    {
        public int AeropuertoId { get; set; }
        public string Ciudad { get; set; }
        public string CodigoIATA { get; set; }
        public int? EscalaId { get; set; }
        public string Escala { get; set; }
        public List<byte> Dias { get; set; }

        public bool TieneEscala
        {
            get { return EscalaId.HasValue; }
        }

        public string DiasTexto
        {
            get { return Frecuencia.NombresDias(Dias); }
        }

        public string Texto
        {
            get
            {
                string texto = Ciudad + " (" + CodigoIATA + ")";
                return TieneEscala ? texto + " · escala en " + Escala : texto;
            }
        }
    }
}
