using System.Collections.Generic;
using System.Linq;

namespace AerolineaVIP.Models
{
    public partial class Frecuencia
    {
        // DiaSemana: 1 = lunes ... 7 = domingo
        private static readonly string[] NombresDiaSemana = { "", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };

        public static string NombreDia(byte dia)
        {
            return dia >= 1 && dia <= 7 ? NombresDiaSemana[dia] : "";
        }

        public static string NombresDias(IEnumerable<byte> dias)
        {
            return string.Join(" · ", dias.OrderBy(d => d).Select(d => NombreDia(d)));
        }

        public static Dictionary<byte, string> DiasSemana()
        {
            var dias = new Dictionary<byte, string>();
            for (byte d = 1; d <= 7; d++)
            {
                dias.Add(d, NombresDiaSemana[d]);
            }
            return dias;
        }

        public string NombreDiaSemana
        {
            get { return NombreDia(DiaSemana); }
        }

        public string Horario
        {
            get { return HoraSalida.ToString(@"hh\:mm") + " – " + HoraLlegada.ToString(@"hh\:mm"); }
        }

        public string Descripcion
        {
            get { return Ruta.Descripcion + " · " + NombreDiaSemana + " " + Horario; }
        }
    }
}
