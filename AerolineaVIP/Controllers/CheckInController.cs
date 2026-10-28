using System;
using System.Linq;
using System.Web.Mvc;
using AerolineaVIP.Models;
using Microsoft.AspNet.Identity;

namespace AerolineaVIP.Controllers
{
    [Authorize(Roles = RolesSistema.ClienteCounterYSuper)]
    public class CheckInController : Controller
    {
        private readonly AerolineaVIPEntities db = new AerolineaVIPEntities();
        private bool EsCounter { get { return User.IsInRole(RolesSistema.EmpleadoCounter) || User.IsInRole(RolesSistema.SuperUsuario); } }
        private Reserva Localizar(BusquedaCheckInViewModel modelo)
        {
            return CheckIn.Localizar(db, modelo.CodigoReserva, modelo.Documento, User.Identity.GetUserId(), EsCounter);
        }
        public ActionResult Index(BusquedaCheckInViewModel modelo)
        {
            Reserva.Expirar(db);
            if (Request.QueryString.Count == 0) ModelState.Clear();
            if (Request.QueryString.Count > 0 && ModelState.IsValid) CheckIn.Preparar(Localizar(modelo), modelo, EsCounter);
            return View(modelo);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Registrar(BusquedaCheckInViewModel modelo)
        {
            Reserva.Expirar(db);
            if (!ModelState.IsValid) return View("Index", modelo);
            var reserva = Localizar(modelo);
            if (reserva == null) { modelo.Mensaje = CheckIn.MensajeNeutral; return View("Index", modelo); }
            try
            {
                CheckIn.Registrar(db, reserva, modelo.Seleccionados == null ? null : modelo.Seleccionados.ToArray());
                TempData["Mensaje"] = "Check-in COMPLETADO para las selecciones indicadas.";
            }
            catch (Exception ex) { TempData["Error"] = ex is InvalidOperationException ? ex.Message : ErroresBaseDatos.ObtenerMensaje(ex); }
            return RedirectToAction("Index", new { modelo.CodigoReserva, modelo.Documento });
        }
        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = RolesSistema.EmpleadoCounter + "," + RolesSistema.SuperUsuario)]
        public ActionResult Anular(BusquedaCheckInViewModel modelo, int checkInId)
        {
            if (!ModelState.IsValid) return View("Index", modelo);
            var reserva = Localizar(modelo);
            var check = reserva == null ? null : db.CheckIns.FirstOrDefault(c => c.CheckInId == checkInId && c.ReservaPasajeroVuelo.ReservaId == reserva.ReservaId);
            if (check == null) { modelo.Mensaje = CheckIn.MensajeNeutral; return View("Index", modelo); }
            try
            {
                check.EstadoCheckInCodigo = "ANULADO";
                db.SaveChanges();
                TempData["Mensaje"] = "Check-in ANULADO por el counter.";
            }
            catch (Exception ex) { TempData["Error"] = ErroresBaseDatos.ObtenerMensaje(ex); }
            return RedirectToAction("Index", new { modelo.CodigoReserva, modelo.Documento });
        }
        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
