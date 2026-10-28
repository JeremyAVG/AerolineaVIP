using System;
using System.Linq;

namespace AerolineaVIP.Models
{
    public partial class CheckIn
    {
        public const string MensajeNeutral = "No se encontró una reserva con los datos indicados.";
        public static Reserva Localizar(AerolineaVIPEntities db, string codigo, string documento, string usuario, bool counter)
        {
            return db.Reservas.FirstOrDefault(r => r.CodigoReserva == codigo && (counter || r.UsuarioId == usuario)
                && r.ReservaPasajeroVueloes.Any(a => a.Pasajero.NumeroDocumento == documento));
        }
        public static string Elegibilidad(ReservaPasajeroVuelo asignacion, DateTime ahora)
        {
            if (asignacion.CheckIns.Any(c => c.EstadoCheckInCodigo == "COMPLETADO")) return "Check-in COMPLETADO";
            if (asignacion.EstadoAsignacionCodigo != "CONFIRMADA" || asignacion.Reserva.EstadoReservaCodigo != "CONFIRMADA") return "Requiere un asiento y una reserva confirmados.";
            if (asignacion.Vuelo.EstadoVueloCodigo != "PROGRAMADO" && asignacion.Vuelo.EstadoVueloCodigo != "CONFIRMADO") return "El vuelo no admite check-in.";
            if (ahora < asignacion.Vuelo.Salida.AddHours(-24)) return "Aún no abre: desde " + asignacion.Vuelo.Salida.AddHours(-24).ToString("dd/MM/yyyy HH:mm");
            if (ahora >= asignacion.Vuelo.Salida.AddHours(-1)) return "Cerrado: el check-in cierra una hora antes de la salida.";
            return null;
        }
        public static void Preparar(Reserva reserva, BusquedaCheckInViewModel modelo, bool counter)
        {
            modelo.PuedeAnular = counter;
            if (reserva == null) { modelo.Mensaje = MensajeNeutral; return; }
            var ahora = DateTime.Now;
            modelo.Filas = reserva.ReservaPasajeroVueloes.OrderBy(a => a.Vuelo.Salida).ThenBy(a => a.PasajeroId).Select(a =>
            {
                string motivo = Elegibilidad(a, ahora);
                var activo = a.CheckIns.FirstOrDefault(c => c.EstadoCheckInCodigo == "COMPLETADO");
                var asiento = a.Vuelo.Aeronave == null ? null : a.Vuelo.Aeronave.ModeloAeronave.Asientoes.FirstOrDefault(s => s.NumeroAsiento == a.NumeroAsiento);
                return new CheckInFila { AsignacionId = a.AsignacionId, CheckInId = activo == null ? (int?)null : activo.CheckInId,
                    Pasajero = a.Pasajero.Nombres + " " + a.Pasajero.Apellidos, Documento = a.Pasajero.DocumentoTexto,
                    Tramo = a.Vuelo.Descripcion, Asiento = asiento == null ? "Sin asiento" : asiento.Codigo,
                    Elegible = motivo == null, Estado = motivo ?? "Elegible para check-in", Anulable = counter && activo != null };
            }).ToList();
        }
        public static void Registrar(AerolineaVIPEntities db, Reserva reserva, int[] seleccionados)
        {
            if (seleccionados == null || seleccionados.Length == 0 || seleccionados.Distinct().Count() != seleccionados.Length)
                throw new InvalidOperationException("Seleccione al menos un pasajero elegible por tramo, sin duplicar selecciones.");
            using (var transaccion = db.Database.BeginTransaction())
            {
                var asignaciones = reserva.ReservaPasajeroVueloes.Where(a => seleccionados.Contains(a.AsignacionId)).ToList();
                if (asignaciones.Count != seleccionados.Length) throw new InvalidOperationException("La selección no corresponde a esta reserva.");
                var ahora = DateTime.Now;
                foreach (var asignacion in asignaciones)
                {
                    if (asignacion.CheckIns.Any(c => c.EstadoCheckInCodigo == "COMPLETADO")) continue;
                    string motivo = Elegibilidad(asignacion, ahora);
                    if (motivo != null) throw new InvalidOperationException(motivo);
                    db.CheckIns.Add(new CheckIn { AsignacionId = asignacion.AsignacionId, EstadoCheckInCodigo = "COMPLETADO", FechaRegistro = ahora });
                }
                db.SaveChanges();
                transaccion.Commit();
            }
        }
    }
}
