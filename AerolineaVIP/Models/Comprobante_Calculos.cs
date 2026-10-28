using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace AerolineaVIP.Models
{
    public partial class Comprobante
    {
        public static ComprobanteVista Preparar(AerolineaVIPEntities db, vw_ComprobanteDetalle detalle)
        {
            decimal importe = detalle.TotalOperacion;
            if (detalle.TipoComprobanteCodigo == "NOTA_CREDITO")
            {
                var reversado = db.Pagoes.FirstOrDefault(p => p.PagoId == detalle.PagoId && p.EstadoPagoCodigo == "REVERSADO");
                if (reversado == null) throw new InvalidOperationException("La nota de crédito no tiene una reversión registrada.");
                importe = reversado.Importe;
            }
            var filas = db.Reservas.Find(detalle.ReservaId).Confirmacion().Filas;
            return new ComprobanteVista { Id = detalle.ComprobanteId, Numero = detalle.NumeroComprobante, Tipo = detalle.TipoComprobante,
                Documentos = string.Join(" / ", filas.Select(f => f.Documento).Distinct()),
                Estado = detalle.EstadoComprobanteCodigo, Fecha = detalle.FechaEmision.ToString("dd/MM/yyyy HH:mm"),
                Emisor = detalle.EmisorNombre, Identificacion = detalle.EmisorIdentificacion, Direccion = detalle.EmisorDireccion,
                CodigoReserva = detalle.CodigoReserva, Importe = importe.ToString("0.00") + " " + detalle.MonedaCodigo,
                Metodo = (detalle.MetodoPago ?? "Sin pago") + " · " + detalle.MarcaTarjeta + " " + detalle.TarjetaEnmascarada,
                Operacion = detalle.CodigoOperacion, Filas = filas };
        }
        public static void Listar(AerolineaVIPEntities db, string usuario, ComprobantesViewModel modelo)
        {
            var consulta = db.vw_ComprobanteDetalle.Where(c => c.UsuarioId == usuario);
            if (modelo.Desde.HasValue) { var desde = modelo.Desde.Value.Date; consulta = consulta.Where(c => c.FechaEmision >= desde); }
            if (modelo.Hasta.HasValue) { var hasta = modelo.Hasta.Value.Date; consulta = consulta.Where(c => DbFunctions.TruncateTime(c.FechaEmision) <= hasta); }
            if (!string.IsNullOrEmpty(modelo.Tipo)) consulta = consulta.Where(c => c.TipoComprobanteCodigo == modelo.Tipo);
            modelo.Total = consulta.Count();
            modelo.Pagina = Math.Min(Math.Max(1, modelo.Pagina), Math.Max(1, (int)Math.Ceiling(modelo.Total / 10m)));
            modelo.Comprobantes = consulta.OrderByDescending(c => c.FechaEmision).ThenByDescending(c => c.ComprobanteId)
                .Skip((modelo.Pagina - 1) * 10).Take(10).ToList().Select(c => Preparar(db, c)).ToList();
            modelo.Paginas = Reserva.PaginasCliente(modelo.Total, modelo.Pagina);
        }
        public static byte[] GenerarPdf(ComprobanteVista modelo)
        {
            using (var documento = new PdfDocument())
            using (var memoria = new MemoryStream())
            {
                documento.Info.Title = modelo.Tipo + " " + modelo.Numero;
                documento.Info.Author = "Aerolínea VIP";
                documento.Info.Subject = ComprobanteVista.Leyenda;
                var lineas = new List<string> { "AEROLÍNEA VIP · COMPROBANTE ACADÉMICO", modelo.Tipo + " · " + modelo.Numero,
                    "Estado: " + modelo.Estado + " · Emisión: " + modelo.Fecha, modelo.Emisor, modelo.Identificacion, modelo.Direccion,
                    "Reserva: " + modelo.CodigoReserva, "Operación simulada: " + modelo.Operacion, modelo.Metodo, "" };
                foreach (var fila in modelo.Filas)
                {
                    lineas.Add(fila.Tramo + " · " + fila.Ruta);
                    lineas.Add(fila.Fecha + " · " + fila.Horario + " · " + fila.Pasajero + " · " + fila.Documento + " · Asiento " + fila.Asiento);
                }
                lineas.Add(""); lineas.Add("TOTAL DEL COMPROBANTE: " + modelo.Importe);
                lineas.Add("Sin impuestos. Sin validez tributaria. No representa un cobro ni un reembolso real.");
                lineas.Add(ComprobanteVista.Leyenda);
                var fuente = new XFont("Arial", 10, XFontStyle.Regular);
                {
                    XGraphics grafico = null;
                    double y = 50;
                    try
                    {
                        foreach (var linea in lineas)
                        {
                            string pendiente = "";
                            foreach (string palabra in (linea ?? "").Split(' '))
                            {
                                string candidata = pendiente.Length == 0 ? palabra : pendiente + " " + palabra;
                                if (grafico == null) { var pagina = documento.AddPage(); grafico = XGraphics.FromPdfPage(pagina); }
                                if (grafico.MeasureString(candidata, fuente).Width > 490 && pendiente.Length > 0)
                                { DibujarLinea(documento, ref grafico, fuente, pendiente, ref y); pendiente = palabra; }
                                else pendiente = candidata;
                            }
                            DibujarLinea(documento, ref grafico, fuente, pendiente, ref y);
                        }
                    }
                    finally { if (grafico != null) grafico.Dispose(); }
                }
                documento.Save(memoria, false);
                return memoria.ToArray();
            }
        }
        private static void DibujarLinea(PdfDocument documento, ref XGraphics grafico, XFont fuente, string texto, ref double y)
        {
            if (y > 760) { grafico.Dispose(); grafico = XGraphics.FromPdfPage(documento.AddPage()); y = 50; }
            grafico.DrawString(texto, fuente, XBrushes.Black, new XPoint(48, y));
            y += 18;
        }
    }
}
