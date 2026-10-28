using System;
using System.Data.Entity;
using System.Linq;

namespace AerolineaVIP.Models
{
    // Consultas de la pantalla 21 (consultas y reportes administrativos)
    public static class Reportes
    {
        public static void Ocupacion(AerolineaVIPEntities db, ReporteOcupacionViewModel f)
        {
            var filas = db.vw_DisponibilidadVuelo.AsQueryable();
            if (f.Desde.HasValue)
            {
                filas = filas.Where(d => d.FechaOperacion >= f.Desde.Value);
            }
            if (f.Hasta.HasValue)
            {
                filas = filas.Where(d => d.FechaOperacion <= f.Hasta.Value);
            }
            if (f.RutaId.HasValue)
            {
                filas = filas.Where(d => db.Vueloes.Any(v => v.VueloId == d.VueloId && v.Frecuencia.RutaId == f.RutaId));
            }
            if (f.AeronaveId.HasValue)
            {
                filas = filas.Where(d => db.Vueloes.Any(v => v.VueloId == d.VueloId && v.AeronaveId == f.AeronaveId));
            }
            if (!string.IsNullOrEmpty(f.Estado))
            {
                filas = filas.Where(d => d.EstadoVueloCodigo == f.Estado);
            }
            f.Filas = filas.OrderBy(d => d.FechaOperacion).ThenBy(d => d.HoraSalida).ToList();
            f.FechaCorte = DateTime.Now;
        }

        public static void ReservasPorEstado(AerolineaVIPEntities db, ReporteReservasViewModel f)
        {
            var consulta = db.Reservas.Include(r => r.ReservaPasajeroVueloes).AsQueryable();
            if (f.Desde.HasValue)
            {
                consulta = consulta.Where(r => r.FechaCreacion >= f.Desde.Value);
            }
            if (f.Hasta.HasValue)
            {
                DateTime hastaFin = f.Hasta.Value.Date.AddDays(1);
                consulta = consulta.Where(r => r.FechaCreacion < hastaFin);
            }
            var reservas = consulta.ToList();

            f.Filas = db.EstadoReservas.OrderBy(e => e.Codigo).ToList()
                .Select(e => new ReservasPorEstado
                {
                    Codigo = e.Codigo,
                    Descripcion = e.Descripcion,
                    Cantidad = reservas.Count(r => r.EstadoReservaCodigo == e.Codigo),
                    Asientos = reservas.Where(r => r.EstadoReservaCodigo == e.Codigo).Sum(r => r.ReservaPasajeroVueloes.Count),
                    Importe = reservas.Where(r => r.EstadoReservaCodigo == e.Codigo).Sum(r => r.ReservaPasajeroVueloes.Sum(a => a.ImporteTramo))
                }).ToList();
            f.FechaCorte = DateTime.Now;
        }

        public static void Compras(AerolineaVIPEntities db, ReporteComprasViewModel f)
        {
            var consulta = db.Compras.Include(c => c.Reserva).Include(c => c.EstadoCompra)
                .Include(c => c.Pagoes.Select(p => p.EstadoPago))
                .Include(c => c.Pagoes.Select(p => p.MarcaTarjeta));
            if (f.Desde.HasValue)
            {
                consulta = consulta.Where(c => c.FechaCompra >= f.Desde.Value);
            }
            if (f.Hasta.HasValue)
            {
                DateTime hastaFin = f.Hasta.Value.Date.AddDays(1);
                consulta = consulta.Where(c => c.FechaCompra < hastaFin);
            }
            if (!string.IsNullOrEmpty(f.EstadoCompra))
            {
                consulta = consulta.Where(c => c.EstadoCompraCodigo == f.EstadoCompra);
            }
            f.Compras = consulta.OrderByDescending(c => c.FechaCompra).ToList();

            var pagos = f.Compras.SelectMany(c => c.Pagoes).ToList();
            f.ResumenPagos = db.EstadoPagoes.OrderBy(e => e.Codigo).ToList()
                .Select(e => new PagosPorEstado
                {
                    Codigo = e.Codigo,
                    Descripcion = e.Descripcion,
                    Cantidad = pagos.Count(p => p.EstadoPagoCodigo == e.Codigo),
                    Total = pagos.Where(p => p.EstadoPagoCodigo == e.Codigo).Sum(p => p.Importe)
                }).ToList();
            f.FechaCorte = DateTime.Now;
        }
    }
}
