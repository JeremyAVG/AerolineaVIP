using System;
using System.Linq;
using System.Web.Mvc;
using AerolineaVIP.Models;
using Microsoft.AspNet.Identity;

namespace AerolineaVIP.Controllers
{
    [Authorize(Roles = RolesSistema.ClienteCounterYSuper + "," + RolesSistema.Administrador)]
    public class ReservasController : Controller
    {
        private readonly AerolineaVIPEntities db = new AerolineaVIPEntities();
        private bool EsAdmin { get { return User.IsInRole(RolesSistema.Administrador) || User.IsInRole(RolesSistema.SuperUsuario); } }
        private bool EsCounter { get { return User.IsInRole(RolesSistema.EmpleadoCounter); } }
        public ActionResult MisReservas(MisReservasViewModel modelo)
        {
            Reserva.Expirar(db);
            modelo.Estados = db.EstadoReservas.Select(e => new SelectListItem { Value = e.Codigo, Text = e.Descripcion }).ToList();
            if (ModelState.IsValid) Reserva.ListarPropias(db, User.Identity.GetUserId(), modelo, EsCounter);
            return View(modelo);
        }
        private Reserva Autorizada(int id)
        {
            string usuario = User.Identity.GetUserId();
            var asistidas = EsCounter ? Reserva.ReservasAsistidas(Session, usuario) : new System.Collections.Generic.List<int>();
            return db.Reservas.FirstOrDefault(r => r.ReservaId == id && (r.UsuarioId == usuario || asistidas.Contains(r.ReservaId) ||
                EsAdmin && r.ReservaPasajeroVueloes.Any(a => a.Vuelo.EstadoVueloCodigo == "CANCELADO")));
        }
        public ActionResult Gestion(int id)
        {
            Reserva.Expirar(db);
            var reserva = Autorizada(id);
            return reserva == null ? (ActionResult)HttpNotFound() : View(reserva.Gestion(EsAdmin, EsCounter));
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Cancelar(GestionReservaViewModel modelo)
        {
            Reserva.Expirar(db);
            var reserva = Autorizada(modelo.ReservaId);
            if (reserva == null) return HttpNotFound();
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Debe aceptar la cancelación completa antes de confirmar.";
                return RedirectToAction("Gestion", new { id = modelo.ReservaId });
            }
            try
            {
                reserva.Cancelar(db, EsAdmin, EsCounter);
                TempData["Mensaje"] = "Reserva cancelada. Todos sus asientos fueron liberados. " + reserva.Gestion(EsAdmin, EsCounter).Reembolso;
            }
            catch (Exception ex) { TempData["Error"] = ex is InvalidOperationException ? ex.Message : ErroresBaseDatos.ObtenerMensaje(ex); }
            return RedirectToAction("Gestion", new { id = modelo.ReservaId });
        }
        [Authorize(Roles = RolesSistema.AdminYSuper)]
        public ActionResult Cancelaciones()
        {
            Reserva.Expirar(db);
            var reservas = db.Reservas.Where(r => r.ReservaPasajeroVueloes.Any(a => a.Vuelo.EstadoVueloCodigo == "CANCELADO")
                && (r.EstadoReservaCodigo == "PENDIENTE" || r.EstadoReservaCodigo == "CONFIRMADA")).OrderByDescending(r => r.ReservaId).ToList();
            return View(reservas.Select(r => r.Gestion(true)).ToList());
        }
        [Authorize(Roles = RolesSistema.EmpleadoCounter)]
        public ActionResult Asistencia(AsistenciaViewModel modelo)
        {
            Reserva.Expirar(db);
            if (Request.QueryString.Count == 0) ModelState.Clear();
            else if (ModelState.IsValid)
            {
                Reserva.BuscarAsistencia(db, modelo);
                Session["AccesoAsistencia"] = new AccesoAsistencia { OperadorId = User.Identity.GetUserId(), Reservas = modelo.Reservas.Select(r => r.Id).ToList() };
            }
            return View(modelo);
        }
        [Authorize(Roles = RolesSistema.EmpleadoCounter)]
        public ActionResult NuevaCompra() { return View(new NuevaCompraClienteViewModel()); }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = RolesSistema.EmpleadoCounter)]
        public ActionResult NuevaCompra(NuevaCompraClienteViewModel modelo)
        {
            if (ModelState.IsValid)
            {
                var cliente = Reserva.BuscarCliente(modelo.Cliente);
                if (cliente == null) ModelState.AddModelError("Cliente", "Ingrese el usuario o correo de una cuenta existente con rol Cliente.");
                else
                {
                    cliente.OperadorId = User.Identity.GetUserId();
                    Session["ClienteCounter"] = cliente;
                    Session.Remove("Compra");
                    TempData["ClienteCompra"] = "Nueva compra para el cliente " + cliente.NombreUsuario + ".";
                    return RedirectToAction("Buscar", "Vuelos");
                }
            }
            return View(modelo);
        }
        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
