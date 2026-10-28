using System;
using System.Configuration;
using System.Linq;

namespace AerolineaVIP.Models
{
    public partial class Vuelo
    {
        public DateTime Salida { get { return FechaOperacion.Date.Add(Frecuencia.HoraSalida); } }
        public DateTime Llegada { get { return FechaOperacion.Date.Add(Frecuencia.HoraLlegada); } }

        public string ValidarPreparacion(AerolineaVIPEntities db, int? aeronaveId)
        {
            if (!aeronaveId.HasValue || EstadoVueloCodigo == "CANCELADO") return null;
            var frecuencia = db.Frecuencias.Find(FrecuenciaId);
            if (frecuencia == null) return "Seleccione una frecuencia válida.";
            var aeronave = db.Aeronaves.Find(aeronaveId);
            if (aeronave == null || aeronave.EstadoAeronaveCodigo != "DISPONIBLE") return "La aeronave no está disponible.";
            int minutos;
            if (!int.TryParse(ConfigurationManager.AppSettings["MinutosPreparacion"], out minutos) || minutos < 60) minutos = 60;
            var salida = FechaOperacion.Date.Add(frecuencia.HoraSalida);
            var llegada = FechaOperacion.Date.Add(frecuencia.HoraLlegada);
            var otros = db.Vueloes.Include("Frecuencia.Ruta").Where(v => v.AeronaveId == aeronaveId
                && v.VueloId != VueloId && v.EstadoVueloCodigo != "CANCELADO").ToList();
            if (otros.Any(v => salida < v.Llegada.AddMinutes(minutos) && llegada.AddMinutes(minutos) > v.Salida))
                return "La aeronave requiere al menos " + minutos + " minutos de preparación entre vuelos.";
            var anterior = otros.Where(v => v.Llegada <= salida).OrderByDescending(v => v.Llegada).FirstOrDefault();
            var siguiente = otros.Where(v => v.Salida >= llegada).OrderBy(v => v.Salida).FirstOrDefault();
            if (anterior != null && anterior.Frecuencia.Ruta.AeropuertoDestinoId != frecuencia.Ruta.AeropuertoOrigenId
                || siguiente != null && frecuencia.Ruta.AeropuertoDestinoId != siguiente.Frecuencia.Ruta.AeropuertoOrigenId)
                return "La salida debe coincidir con el aeropuerto de llegada del vuelo anterior de la aeronave.";
            return null;
        }

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
