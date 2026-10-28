using System;
using System.Linq;
using System.Web.Mvc;
using AerolineaVIP.Models;
using Microsoft.AspNet.Identity;

namespace AerolineaVIP.Controllers
{
    [Authorize(Roles = RolesSistema.ClienteCounterYSuper + "," + RolesSistema.Administrador)]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class ComprobantesController : Controller
    {
        private readonly AerolineaVIPEntities db = new AerolineaVIPEntities();
        public ActionResult Index(ComprobantesViewModel modelo)
        {
            modelo.Tipos = db.TipoComprobantes.Select(t => new SelectListItem { Value = t.Codigo, Text = t.Descripcion }).ToList();
            if (ModelState.IsValid) Comprobante.Listar(db, User.Identity.GetUserId(), modelo);
            return View(modelo);
        }
        private vw_ComprobanteDetalle Autorizado(int id)
        {
            string usuario = User.Identity.GetUserId();
            bool admin = User.IsInRole(RolesSistema.Administrador) || User.IsInRole(RolesSistema.SuperUsuario);
            var asistidas = User.IsInRole(RolesSistema.EmpleadoCounter) ? Reserva.ReservasAsistidas(Session, usuario) : new System.Collections.Generic.List<int>();
            return db.vw_ComprobanteDetalle.FirstOrDefault(c => c.ComprobanteId == id && (c.UsuarioId == usuario || asistidas.Contains(c.ReservaId) ||
                admin && db.ReservaPasajeroVueloes.Any(a => a.ReservaId == c.ReservaId && a.Vuelo.EstadoVueloCodigo == "CANCELADO")));
        }
        public ActionResult VistaPrevia(int id)
        {
            var detalle = Autorizado(id);
            if (detalle == null) return HttpNotFound();
            return View(Comprobante.Preparar(db, detalle));
        }
        public ActionResult Descargar(int id)
        {
            var detalle = Autorizado(id);
            if (detalle == null) return HttpNotFound();
            try
            {
                var modelo = Comprobante.Preparar(db, detalle);
                return File(Comprobante.GenerarPdf(modelo), "application/pdf", modelo.Numero + ".pdf");
            }
            catch (Exception)
            {
                TempData["Error"] = "No se pudo generar el PDF. Intente nuevamente.";
                return RedirectToAction("VistaPrevia", new { id });
            }
        }
        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
