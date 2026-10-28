using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace AerolineaVIP.Models
{
    public partial class Reserva
    {
        public static List<PasoCompra> Pasos(int actual)
        {
            var textos = new[] { "1. Asientos", "2. Pasajeros", "3. Confirmar reserva", "4. Pago y confirmación" };
            return textos.Select((texto, indice) => new PasoCompra { Texto = texto,
                Clase = indice + 1 == actual ? "nav-link active" : "nav-link text-body-secondary",
                Actual = indice + 1 == actual ? "step" : null }).ToList();
        }

        public static void PrepararPasajeros(AerolineaVIPEntities db, SeleccionCompra seleccion, PasajerosCompra modelo)
        {
            if (modelo.Personas == null) modelo.Personas = new List<PasajeroCompra>();
            for (int p = 0; p < modelo.Personas.Count; p++)
            {
                var asientos = new List<string>();
                for (int t = 0; t < seleccion.Vuelos.Count; t++)
                {
                    if (seleccion.Asientos.Count <= t || seleccion.Asientos[t].Count <= p) continue;
                    int vuelo = seleccion.Vuelos[t];
                    byte numero = seleccion.Asientos[t][p];
                    string codigo = db.vw_EstadoAsientoVuelo.Where(a => a.VueloId == vuelo && a.NumeroAsiento == numero)
                        .Select(a => a.CodigoAsiento).FirstOrDefault();
                    asientos.Add((seleccion.Vuelos.Count > 1 ? "Tramo " + (t + 1) + ": " : "") + codigo);
                }
                if (modelo.Personas[p] == null) modelo.Personas[p] = new PasajeroCompra();
                modelo.Personas[p].Titulo = "Pasajero " + (p + 1) + " · Asiento " + string.Join(" / ", asientos);
            }
        }

        public FinalCompra Confirmacion()
        {
            var compra = Compras.OrderByDescending(c => c.FechaCompra).ThenByDescending(c => c.CompraId).FirstOrDefault();
            var pago = compra == null ? null : compra.Pagoes.OrderByDescending(p => p.PagoId).FirstOrDefault();
            bool pagada = compra != null && compra.EstadoCompraCodigo == "PAGADA";
            bool rechazado = pago != null && pago.EstadoPagoCodigo == "RECHAZADO";
            var modelo = new FinalCompra { ReservaId = ReservaId, Codigo = CodigoReserva, Pagada = pagada,
                PuedePagar = PuedePagar, Reintentar = PuedePagar && rechazado,
                Mensaje = EstadoReservaCodigo == "EXPIRADA" ? "La reserva ha expirado y sus asientos fueron liberados."
                    : EstadoReservaCodigo == "CANCELADA" ? "La reserva está cancelada."
                    : pagada ? "Compra confirmada. Su pago simulado fue aprobado."
                    : rechazado ? "El pago simulado fue rechazado. Puede reintentarlo mientras la reserva esté vigente."
                    : "Reserva registrada correctamente. Queda pendiente de pago.",
                ClaseMensaje = pagada ? "alert alert-success" : "alert alert-warning",
                Total = (compra != null ? compra.Importe : ReservaPasajeroVueloes.Sum(a => a.ImporteTramo)).ToString("0.00"),
                Metodo = pago == null ? "Sin pago registrado" : (pago.MetodoPagoCodigo == "TARJETA_DEBITO" ? "Tarjeta de débito" : "Tarjeta de crédito")
                    + " · " + pago.MarcaTarjeta.Nombre + " **** " + pago.UltimosCuatroDigitos,
                Operacion = pago == null ? "Sin operación" : pago.CodigoOperacion, Expiracion = ExpiracionTexto,
                Comprobantes = compra == null ? new List<Comprobante>() : compra.Comprobantes.OrderBy(c => c.TipoComprobanteCodigo).ToList() };
            modelo.Estados.Add(EstadoVisual("Reserva", EstadoReservaCodigo));
            modelo.Estados.Add(EstadoVisual("Compra", compra == null ? "SIN COMPRA" : compra.EstadoCompraCodigo));
            modelo.Estados.Add(EstadoVisual("Pago simulado", pago == null ? "SIN PAGO" : pago.EstadoPagoCodigo));
            var vuelos = ReservaPasajeroVueloes.Select(a => a.Vuelo).Distinct().OrderBy(v => v.Salida).ToList();
            foreach (var asignacion in ReservaPasajeroVueloes.OrderBy(a => a.Vuelo.Salida).ThenBy(a => a.PasajeroId))
            {
                var vuelo = asignacion.Vuelo;
                var asiento = vuelo.Aeronave.ModeloAeronave.Asientoes.FirstOrDefault(a => a.NumeroAsiento == asignacion.NumeroAsiento);
                modelo.Filas.Add(new FilaCompra { Tramo = "Tramo " + (vuelos.IndexOf(vuelo) + 1) + " · " + vuelo.CodigoVuelo,
                    Ruta = vuelo.Frecuencia.Ruta.Descripcion, Fecha = vuelo.FechaOperacion.ToString("dd/MM/yyyy"),
                    Horario = vuelo.Frecuencia.Horario, Pasajero = asignacion.Pasajero.Nombres + " " + asignacion.Pasajero.Apellidos,
                    Documento = asignacion.Pasajero.NumeroDocumento, Asiento = asiento == null ? "Sin asignar" : asiento.Codigo,
                    Importe = asignacion.ImporteTramo.ToString("0.00") });
            }
            return modelo;
        }

        private static EstadoCompraVisual EstadoVisual(string etiqueta, string estado)
        {
            return new EstadoCompraVisual { Etiqueta = etiqueta, Texto = estado,
                Clase = "badge " + (estado == "CONFIRMADA" || estado == "PAGADA" || estado == "APROBADO" ? "text-bg-success"
                    : estado == "PENDIENTE" ? "text-bg-warning" : estado == "RECHAZADO" || estado == "EXPIRADA" || estado == "CANCELADA" || estado == "ANULADA" ? "text-bg-danger" : "text-bg-secondary") };
        }
        public bool PuedePagar { get { return EstadoReservaCodigo == "PENDIENTE" && FechaExpiracion > System.DateTime.Now; } }
        public bool TieneOperaciones
        {
            get { return ReservaPasajeroVueloes.Any() || Compras.Any(); }
        }

        public string ExpiracionTexto
        {
            get { return FechaExpiracion.HasValue ? FechaExpiracion.Value.ToString("dd/MM/yyyy HH:mm") : "Sin fecha de expiración"; }
        }

        public int CantidadPasajeros
        {
            get { return ReservaPasajeroVueloes.Select(a => a.PasajeroId).Distinct().Count(); }
        }

        public decimal ImporteVigente
        {
            get
            {
                return ReservaPasajeroVueloes
                    .Where(a => a.EstadoAsignacionCodigo == "PENDIENTE" || a.EstadoAsignacionCodigo == "CONFIRMADA")
                    .Sum(a => a.ImporteTramo);
            }
        }

        public static void Expirar(AerolineaVIPEntities db)
        {
            var ahora = DateTime.Now;
            using (var transaccion = db.Database.BeginTransaction())
            {
                var vencidas = db.Reservas.Where(r => r.EstadoReservaCodigo == "PENDIENTE"
                    && r.FechaExpiracion <= ahora).ToList();
                foreach (var reserva in vencidas)
                {
                    reserva.EstadoReservaCodigo = "EXPIRADA";
                    foreach (var asiento in reserva.ReservaPasajeroVueloes.Where(a => a.EstadoAsignacionCodigo == "PENDIENTE"))
                        asiento.EstadoAsignacionCodigo = "LIBERADA";
                    foreach (var compra in reserva.Compras.Where(c => c.EstadoCompraCodigo == "PENDIENTE"))
                        compra.EstadoCompraCodigo = "ANULADA";
                }
                db.SaveChanges();
                transaccion.Commit();
            }
        }

        public static List<OpcionVuelo> Opciones(AerolineaVIPEntities db, DateTime fecha, int cantidad, bool soloDisponibles = true)
        {
            var vuelos = db.Vueloes.Include("Frecuencia.Ruta.Aeropuerto").Include("Frecuencia.Ruta.Aeropuerto1")
                .Include("Aeronave").Where(v => v.FechaOperacion == fecha && v.AeronaveId != null
                    && (v.EstadoVueloCodigo == "PROGRAMADO" || v.EstadoVueloCodigo == "CONFIRMADO")
                    && v.Frecuencia.Activa && v.Frecuencia.Ruta.Activa && v.Aeronave.EstadoAeronaveCodigo == "DISPONIBLE")
                .ToList().Where(v => v.Salida > DateTime.Now.AddHours(2)
                    && v.Frecuencia.Ruta.Aeropuerto.Activo && v.Frecuencia.Ruta.Aeropuerto1.Activo).ToList();
            var grupos = new List<List<Vuelo>>();
            if (fecha.DayOfWeek == DayOfWeek.Sunday)
            {
                var tramos = db.TramoItinerarios.Include("Itinerario").Where(t => t.Itinerario.EstadoItinerarioCodigo == "ACTIVO"
                    && t.Vuelo.FechaOperacion == fecha).ToList();
                foreach (var itinerario in tramos.GroupBy(t => t.ItinerarioId))
                    foreach (var orden in new[] { 1, 3 })
                    {
                        var par = itinerario.Where(t => t.Orden == orden || t.Orden == orden + 1).OrderBy(t => t.Orden).ToList();
                        if (par.Count != 2) continue;
                        var primero = vuelos.FirstOrDefault(v => v.VueloId == par[0].VueloId);
                        var segundo = vuelos.FirstOrDefault(v => v.VueloId == par[1].VueloId);
                        if (primero != null && segundo != null && ConexionValida(primero, segundo))
                            grupos.Add(new List<Vuelo> { primero, segundo });
                    }
            }
            else grupos.AddRange(vuelos.Where(v => v.Frecuencia.Ruta.AeropuertoOrigen.CodigoIATA != "GPS"
                && v.Frecuencia.Ruta.AeropuertoDestino.CodigoIATA != "GPS").Select(v => new List<Vuelo> { v }));
            var disponibilidad = db.vw_DisponibilidadVuelo.Where(v => v.FechaOperacion == fecha).ToList();
            return grupos.Select(g => new OpcionVuelo
            {
                Clave = string.Join(",", g.Select(v => v.VueloId)), Vuelos = g.Select(v => v.VueloId).ToList(),
                Descripcion = g.First().Frecuencia.Ruta.AeropuertoOrigen.Localidad + " → "
                    + g.Last().Frecuencia.Ruta.AeropuertoDestino.Localidad + (g.Count == 2 ? " · escala en Guayaquil" : " · directo"),
                Horario = string.Join(" / ", g.Select(v => v.Descripcion)),
                Tarifa = g.Sum(v => v.Frecuencia.Ruta.TarifaReferencial), Pasajeros = cantidad,
                Disponibles = g.Min(v => disponibilidad.Where(d => d.VueloId == v.VueloId).Select(d => d.Disponibles ?? 0).FirstOrDefault())
            }).Where(o => !soloDisponibles || o.Disponibles >= cantidad).ToList();
        }

        public static bool ConexionValida(Vuelo primero, Vuelo segundo)
        {
            return primero.Frecuencia.Ruta.AeropuertoDestino.CodigoIATA == "GYE"
                && primero.Frecuencia.Ruta.AeropuertoDestinoId == segundo.Frecuencia.Ruta.AeropuertoOrigenId
                && primero.AeronaveId.HasValue && primero.AeronaveId == segundo.AeronaveId
                && segundo.Salida >= primero.Llegada.AddMinutes(60);
        }

        public static List<Vuelo> ValidarSeleccion(AerolineaVIPEntities db, SeleccionCompra seleccion, bool asientos)
        {
            if (seleccion == null || seleccion.Cantidad < 1 || seleccion.Cantidad > 7 || seleccion.Vuelos.Count == 0)
                throw new InvalidOperationException("La selección venció. Busque nuevamente.");
            var primero = db.Vueloes.Find(seleccion.Vuelos[0]);
            if (primero == null || !Opciones(db, primero.FechaOperacion, seleccion.Cantidad)
                .Any(o => o.Vuelos.SequenceEqual(seleccion.Vuelos)))
                throw new InvalidOperationException("El vuelo o itinerario ya no está disponible para la venta.");
            var vuelos = seleccion.Vuelos.Select(id => db.Vueloes.Find(id)).ToList();
            if (asientos)
            {
                if (seleccion.Asientos.Count != vuelos.Count) throw new InvalidOperationException("Seleccione asientos en todos los tramos.");
                for (int i = 0; i < vuelos.Count; i++)
                {
                    var numeros = seleccion.Asientos[i];
                    int id = vuelos[i].VueloId;
                    var libres = db.vw_EstadoAsientoVuelo.Where(a => a.VueloId == id && a.EstadoVisual == "DISPONIBLE").Select(a => a.NumeroAsiento).ToList();
                    if (numeros.Count != seleccion.Cantidad || numeros.Distinct().Count() != numeros.Count || numeros.Any(n => !libres.Contains(n)))
                        throw new InvalidOperationException("Un asiento ya no está disponible. Vuelva a seleccionar los asientos.");
                }
            }
            return vuelos;
        }

        public static MapaCompra Mapa(AerolineaVIPEntities db, Vuelo vuelo, bool seleccionable)
        {
            var mapa = new MapaCompra { VueloId = vuelo.VueloId, Titulo = vuelo.Descripcion, Seleccionable = seleccionable,
                Codigo = vuelo.CodigoVuelo, Ruta = vuelo.Frecuencia.Ruta.Descripcion,
                Fecha = vuelo.FechaOperacion.ToString("dd/MM/yyyy"), Horario = vuelo.Frecuencia.Horario, Aeronave = vuelo.AeronaveTexto,
                Asientos = db.vw_EstadoAsientoVuelo.Where(a => a.VueloId == vuelo.VueloId).OrderBy(a => a.Fila).ThenBy(a => a.Lado)
                .ToList().Select(a => new AsientoCompra { Numero = a.NumeroAsiento, Codigo = a.CodigoAsiento,
                    Estado = a.EstadoVisual, Disponible = seleccionable && a.EstadoVisual == "DISPONIBLE",
                    Deshabilitado = !seleccionable || a.EstadoVisual != "DISPONIBLE",
                    Posicion = "--fila:" + (a.Fila >= 3 ? a.Fila + 1 : a.Fila) + ";--columna:" + (a.Lado == "I" ? "1" : "3")
                        + ";--columna-horizontal:" + (a.Fila >= 3 ? a.Fila + 1 : a.Fila) + ";--lado-horizontal:" + (a.Lado == "D" ? "1" : "3"),
                    DescripcionAccesible = "Asiento " + a.CodigoAsiento + ", fila " + a.Fila + ", lado "
                        + (a.Lado == "D" ? "derecho" : "izquierdo") + ", " + a.EstadoVisual.ToLowerInvariant(),
                    Color = a.ColorEstado == "Verde" ? "#198754" : a.ColorEstado == "Morado" ? "#7952b3" : "#dc3545" }).ToList() };
            mapa.Disponibles = mapa.Asientos.Count(a => a.Estado == "DISPONIBLE");
            mapa.Reservados = mapa.Asientos.Count(a => a.Estado == "RESERVADO");
            mapa.Ocupados = mapa.Asientos.Count(a => a.Estado == "OCUPADO");
            return mapa;
        }

        public static ResumenCompra Resumen(AerolineaVIPEntities db, SeleccionCompra seleccion)
        {
            var vuelos = seleccion.Vuelos.Select(id => db.Vueloes.Find(id)).ToList();
            var resumen = new ResumenCompra { Seleccion = seleccion,
                Total = vuelos.Sum(v => v.Frecuencia.Ruta.TarifaReferencial) * seleccion.Cantidad };
            for (int i = 0; i < vuelos.Count; i++)
            {
                resumen.Mapas.Add(Mapa(db, vuelos[i], true));
                resumen.Detalles.Add(vuelos[i].Descripcion + " · USD " + vuelos[i].Frecuencia.Ruta.TarifaReferencial.ToString("0.00") + " por pasajero");
                if (seleccion.Asientos.Count > i)
                {
                    resumen.Mapas[i].SeleccionTexto = string.Join(",", seleccion.Asientos[i]);
                    foreach (var asiento in resumen.Mapas[i].Asientos.Where(a => seleccion.Asientos[i].Contains(a.Numero) && a.Disponible))
                    {
                        asiento.Clase = "asiento-phenom seleccionado";
                        asiento.SeleccionadoTexto = "true";
                    }
                    for (int p = 0; p < seleccion.Asientos[i].Count; p++)
                    {
                        byte numero = seleccion.Asientos[i][p];
                        string codigo = resumen.Mapas[i].Asientos.Where(a => a.Numero == numero).Select(a => a.Codigo).FirstOrDefault();
                        resumen.Detalles.Add("Pasajero " + (p + 1) + ": " + codigo);
                        if (seleccion.Pasajeros.Count > p)
                            resumen.Filas.Add(new FilaCompra { Tramo = "Tramo " + (i + 1), Ruta = vuelos[i].Frecuencia.Ruta.Descripcion,
                                Pasajero = seleccion.Pasajeros[p].Nombres + " " + seleccion.Pasajeros[p].Apellidos,
                                Documento = seleccion.Pasajeros[p].TipoDocumento + " " + seleccion.Pasajeros[p].Documento,
                                Asiento = codigo, Importe = vuelos[i].Frecuencia.Ruta.TarifaReferencial.ToString("0.00") });
                    }
                }
            }
            return resumen;
        }

        public static Reserva Registrar(AerolineaVIPEntities db, SeleccionCompra seleccion, string clienteId)
        {
            using (var transaccion = db.Database.BeginTransaction())
            {
                var vuelos = ValidarSeleccion(db, seleccion, true);
                if (seleccion.Pasajeros.Count != seleccion.Cantidad || seleccion.Pasajeros.Select(p => p.TipoDocumento + ":" + p.Documento).Distinct().Count() != seleccion.Cantidad)
                    throw new InvalidOperationException("Complete los pasajeros sin repetir documentos.");
                var ahora = DateTime.Now;
                var limite = vuelos.Min(v => v.Salida).AddHours(-2);
                var reserva = new Reserva { UsuarioId = clienteId, FechaCreacion = ahora,
                    FechaExpiracion = limite < ahora.AddHours(24) ? limite : ahora.AddHours(24), EstadoReservaCodigo = "PENDIENTE" };
                db.Reservas.Add(reserva);
                for (int p = 0; p < seleccion.Cantidad; p++)
                {
                    var datos = seleccion.Pasajeros[p];
                    var pasajero = db.Pasajeroes.FirstOrDefault(a => a.TipoDocumentoCodigo == datos.TipoDocumento && a.NumeroDocumento == datos.Documento);
                    if (pasajero == null)
                    {
                        pasajero = new Pasajero { TipoDocumentoCodigo = datos.TipoDocumento, NumeroDocumento = datos.Documento,
                            Nombres = datos.Nombres.Trim(), Apellidos = datos.Apellidos.Trim(), FechaRegistro = ahora,
                            FechaNacimiento = datos.FechaNacimiento, Correo = datos.Correo, Telefono = datos.Telefono };
                        db.Pasajeroes.Add(pasajero);
                    }
                    else if (!string.Equals(pasajero.Nombres.Trim(), datos.Nombres.Trim(), StringComparison.OrdinalIgnoreCase)
                        || !string.Equals(pasajero.Apellidos.Trim(), datos.Apellidos.Trim(), StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("El documento ya existe con otros nombres. Consulte al counter.");
                    for (int i = 0; i < vuelos.Count; i++)
                    {
                        int vueloId = vuelos[i].VueloId;
                        if (pasajero.PasajeroId != 0 && db.ReservaPasajeroVueloes.Any(a => a.PasajeroId == pasajero.PasajeroId && a.VueloId == vueloId
                            && (a.EstadoAsignacionCodigo == "PENDIENTE" || a.EstadoAsignacionCodigo == "CONFIRMADA")))
                            throw new InvalidOperationException("Un pasajero ya tiene un asiento vigente en este vuelo.");
                        db.ReservaPasajeroVueloes.Add(new ReservaPasajeroVuelo { Reserva = reserva, Pasajero = pasajero,
                            VueloId = vueloId, NumeroAsiento = seleccion.Asientos[i][p], ImporteTramo = vuelos[i].Frecuencia.Ruta.TarifaReferencial,
                            EstadoAsignacionCodigo = "PENDIENTE", FechaAsignacion = ahora });
                    }
                }
                db.SaveChanges();
                transaccion.Commit();
                return reserva;
            }
        }
        }
}
