using System.Collections.Generic;
using System.Linq;

namespace AerolineaVIP.Models
{
    public partial class Ruta
    {
        // El EDMX nombró Aeropuerto1 al origen (FK_Ruta_Origen) y Aeropuerto al destino (FK_Ruta_Destino)
        public Aeropuerto AeropuertoOrigen
        {
            get { return Aeropuerto1; }
        }

        public Aeropuerto AeropuertoDestino
        {
            get { return Aeropuerto; }
        }

        public string Descripcion
        {
            get { return AeropuertoOrigen.Localidad + " → " + AeropuertoDestino.Localidad; }
        }

        public List<byte> DiasActivos()
        {
            return Frecuencias.Where(f => f.Activa).Select(f => f.DiaSemana).Distinct().OrderBy(d => d).ToList();
        }

        // Días en que se puede viajar por "primera" y continuar por "segunda" el mismo día
        private static List<byte> DiasConConexion(AerolineaVIPEntities db, Ruta primera, Ruta segunda)
        {
            var dias = new List<byte>();
            if (primera.AeropuertoDestino.CodigoIATA != "GYE") return dias;
            foreach (var f1 in primera.Frecuencias.Where(f => f.Activa))
            {
                bool hayConexion = segunda.Frecuencias.Any(f2 => f2.Activa
                    && f2.DiaSemana == f1.DiaSemana
                    && f2.HoraSalida >= f1.HoraLlegada.Add(System.TimeSpan.FromMinutes(60))
                    && db.Vueloes.Any(v1 => v1.FrecuenciaId == f1.FrecuenciaId && v1.AeronaveId != null
                        && v1.EstadoVueloCodigo != "CANCELADO"
                        && db.Vueloes.Any(v2 => v2.FrecuenciaId == f2.FrecuenciaId && v2.FechaOperacion == v1.FechaOperacion
                            && v2.AeronaveId == v1.AeronaveId && v2.EstadoVueloCodigo != "CANCELADO")));
                if (hayConexion && !dias.Contains(f1.DiaSemana))
                {
                    dias.Add(f1.DiaSemana);
                }
            }
            return dias.OrderBy(d => d).ToList();
        }

        // Destinos a los que se puede viajar desde un origen: directos y con una escala
        public static List<DestinoDisponible> ObtenerDestinos(AerolineaVIPEntities db, int origenId)
        {
            var rutas = db.Rutas.Include("Frecuencias").Include("Aeropuerto").Include("Aeropuerto1")
                .Where(r => r.Activa && r.Aeropuerto.Activo && r.Aeropuerto1.Activo)
                .ToList();
            var directas = rutas.Where(r => r.AeropuertoOrigenId == origenId).ToList();

            var conEscala = new List<DestinoDisponible>();
            foreach (var primera in directas)
            {
                var siguientes = rutas.Where(r => r.AeropuertoOrigenId == primera.AeropuertoDestinoId
                    && r.AeropuertoDestinoId != origenId
                    && !directas.Any(d => d.AeropuertoDestinoId == r.AeropuertoDestinoId));
                foreach (var segunda in siguientes)
                {
                    var dias = DiasConConexion(db, primera, segunda);
                    if (dias.Any())
                    {
                        conEscala.Add(new DestinoDisponible
                        {
                            AeropuertoId = segunda.AeropuertoDestinoId,
                            Ciudad = segunda.AeropuertoDestino.Localidad,
                            CodigoIATA = segunda.AeropuertoDestino.CodigoIATA,
                            EscalaId = primera.AeropuertoDestinoId,
                            Escala = primera.AeropuertoDestino.Localidad,
                            Dias = dias
                        });
                    }
                }
            }

            var resultado = new List<DestinoDisponible>();
            foreach (var ruta in directas)
            {
                // Los días en que la ciudad es solo escala se muestran en el destino final
                var diasComoEscala = conEscala.Where(c => c.EscalaId == ruta.AeropuertoDestinoId).SelectMany(c => c.Dias);
                var dias = ruta.DiasActivos().Except(diasComoEscala).ToList();
                if (dias.Any())
                {
                    resultado.Add(new DestinoDisponible
                    {
                        AeropuertoId = ruta.AeropuertoDestinoId,
                        Ciudad = ruta.AeropuertoDestino.Localidad,
                        CodigoIATA = ruta.AeropuertoDestino.CodigoIATA,
                        Dias = dias
                    });
                }
            }
            resultado.AddRange(conEscala);
            return resultado;
        }
    }
}
