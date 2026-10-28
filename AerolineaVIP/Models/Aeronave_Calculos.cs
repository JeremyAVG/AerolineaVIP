using System;
using System.Collections.Generic;
using System.Linq;

namespace AerolineaVIP.Models
{
    public partial class Aeronave
    {
        public string Descripcion
        {
            get { return Matricula + " · " + ModeloAeronave.Fabricante + " " + ModeloAeronave.Nombre; }
        }

        public List<Vuelo> VuelosDesdeHoy
        {
            get { return VuelosDesde(DateTime.Today); }
        }

        public List<Vuelo> VuelosDesde(DateTime fecha)
        {
            return Vueloes.Where(v => v.FechaOperacion >= fecha.Date && v.EstadoVueloCodigo != "CANCELADO")
                .OrderBy(v => v.FechaOperacion).ThenBy(v => v.Frecuencia.HoraSalida)
                .ToList();
        }
    }
}
