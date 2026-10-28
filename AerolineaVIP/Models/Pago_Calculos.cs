using System;
using System.Linq;

namespace AerolineaVIP.Models
{
    public partial class Pago
    {
        public static void Simular(AerolineaVIPEntities db, Reserva reserva, PagoCompra datos)
        {
            using (var transaccion = db.Database.BeginTransaction())
            {
                db.Entry(reserva).Reload();
                if (reserva.EstadoReservaCodigo == "CONFIRMADA") return;
                if (reserva.EstadoReservaCodigo != "PENDIENTE" || !reserva.FechaExpiracion.HasValue || reserva.FechaExpiracion <= DateTime.Now)
                    throw new InvalidOperationException("La reserva ya no está vigente para pagar.");
                var asignaciones = reserva.ReservaPasajeroVueloes.ToList();
                if (asignaciones.Count == 0 || asignaciones.Any(a => a.EstadoAsignacionCodigo != "PENDIENTE"
                    || a.Vuelo.EstadoVueloCodigo == "CANCELADO" || a.Vuelo.Salida <= DateTime.Now.AddHours(2)))
                    throw new InvalidOperationException("Los tramos de la reserva ya no están disponibles para pagar.");
                var compra = reserva.Compras.FirstOrDefault(c => c.EstadoCompraCodigo == "PENDIENTE" || c.EstadoCompraCodigo == "PAGADA");
                if (compra != null && compra.EstadoCompraCodigo == "PAGADA") return;
                if (compra == null)
                {
                    compra = new Compra { Reserva = reserva, FechaCompra = DateTime.Now, Importe = reserva.ImporteVigente,
                        MonedaCodigo = "USD", EstadoCompraCodigo = "PENDIENTE", ClaveIdempotencia = Guid.NewGuid() };
                    db.Compras.Add(compra);
                    db.SaveChanges();
                }
                string ultimos = datos.Numero.Substring(datos.Numero.Length - 4);
                bool aprobado = ultimos != "0000";
                int intento = db.Pagoes.Count(p => p.CompraId == compra.CompraId) + 1;
                db.Pagoes.Add(new Pago { Compra = compra, FechaPago = DateTime.Now, Importe = compra.Importe,
                    EstadoPagoCodigo = aprobado ? "APROBADO" : "RECHAZADO", MetodoPagoCodigo = datos.Metodo,
                    MarcaTarjetaCodigo = datos.Marca, UltimosCuatroDigitos = ultimos, NombreTitular = datos.Titular.Trim(),
                    CodigoOperacion = "SIM-" + reserva.CodigoReserva + "-" + intento.ToString("00"), EsSimulado = true,
                    MensajeResultado = aprobado ? "Pago simulado aprobado." : "Pago simulado rechazado: tarjeta terminada en 0000." });
                if (aprobado)
                {
                    compra.EstadoCompraCodigo = "PAGADA";
                    reserva.EstadoReservaCodigo = "CONFIRMADA";
                    foreach (var asiento in asignaciones) asiento.EstadoAsignacionCodigo = "CONFIRMADA";
                    foreach (string tipo in new[] { "CONFIRMACION", "FACTURA" })
                        db.Comprobantes.Add(new Comprobante { Compra = compra, TipoComprobanteCodigo = tipo,
                            NumeroComprobante = (tipo == "FACTURA" ? "ACAD-" : "CNF-") + compra.CompraId.ToString("000000"), FechaEmision = DateTime.Now,
                            EstadoComprobanteCodigo = "EMITIDO" });
                }
                db.SaveChanges();
                transaccion.Commit();
            }
        }
    }
}
