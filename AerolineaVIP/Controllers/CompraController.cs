using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AerolineaVIP.Models;
using Microsoft.AspNet.Identity;

namespace AerolineaVIP.Controllers
{
    [Authorize(Roles = RolesSistema.ClienteCounterYSuper)]
    public class CompraController : Controller
    {
        private readonly AerolineaVIPEntities db = new AerolineaVIPEntities();
        private SeleccionCompra Seleccion
        {
            get
            {
                var seleccion = Session["Compra"] as SeleccionCompra;
                return seleccion != null && seleccion.UsuarioId == User.Identity.GetUserId() ? seleccion : null;
            }
            set { Session["Compra"] = value; }
        }
        private bool EsCounter { get { return User.IsInRole(RolesSistema.EmpleadoCounter); } }
        protected override void OnActionExecuting(ActionExecutingContext contexto)
        {
            string accion = contexto.ActionDescriptor.ActionName;
            ViewBag.Pasos = Reserva.Pasos(accion == "Asientos" ? 1 : accion == "Pasajeros" ? 2 : accion == "Confirmar" ? 3 : 4);
            base.OnActionExecuting(contexto);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Iniciar(string clave, int cantidad = 1)
        {
            try
            {
                Reserva.Expirar(db);
                if (!ModelState.IsValid || cantidad < 1 || cantidad > 7) throw new InvalidOperationException("Cantidad de pasajeros inválida.");
                var ids = (clave ?? "").Split(',').Select(int.Parse).ToList();
                var seleccion = new SeleccionCompra { UsuarioId = User.Identity.GetUserId(), Vuelos = ids, Cantidad = cantidad };
                Reserva.ValidarSeleccion(db, seleccion, false);
                Seleccion = seleccion;
                return RedirectToAction("Asientos");
            }
            catch (Exception ex) { TempData["ErrorCompra"] = Mensaje(ex); return RedirectToAction("Buscar", "Vuelos"); }
        }

        public ActionResult Asientos()
        {
            if (Seleccion == null) return RedirectToAction("Buscar", "Vuelos");
            if (Seleccion.ReservaId.HasValue) return RedirectToAction("Pagar", new { id = Seleccion.ReservaId });
            try
            {
                Reserva.Expirar(db);
                Reserva.ValidarSeleccion(db, Seleccion, false);
                return View(Reserva.Resumen(db, Seleccion));
            }
            catch (Exception ex) { TempData["ErrorCompra"] = Mensaje(ex); return RedirectToAction("Buscar", "Vuelos"); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Asientos(string[] asientos)
        {
            if (Seleccion == null) return RedirectToAction("Buscar", "Vuelos");
            if (Seleccion.ReservaId.HasValue) return RedirectToAction("Pagar", new { id = Seleccion.ReservaId });
            try
            {
                if (!ModelState.IsValid) throw new InvalidOperationException("Selección inválida.");
                Seleccion.Asientos = (asientos ?? new string[0]).Select(t => t.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(byte.Parse).ToList()).ToList();
                Reserva.Expirar(db);
                Reserva.ValidarSeleccion(db, Seleccion, true);
                return RedirectToAction("Pasajeros");
            }
            catch (Exception ex) { TempData["ErrorCompra"] = Mensaje(ex); return RedirectToAction("Asientos"); }
        }

        public ActionResult Pasajeros()
        {
            if (Seleccion == null) return RedirectToAction("Buscar", "Vuelos");
            var personas = Seleccion.Pasajeros.Count == Seleccion.Cantidad ? Seleccion.Pasajeros
                : Enumerable.Range(0, Seleccion.Cantidad).Select(p => new PasajeroCompra()).ToList();
            var modelo = new PasajerosCompra { Personas = personas, EsCounter = EsCounter };
            Reserva.PrepararPasajeros(db, Seleccion, modelo);
            return View(modelo);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Pasajeros(PasajerosCompra model)
        {
            if (Seleccion == null) return RedirectToAction("Buscar", "Vuelos");
            if (Seleccion.ReservaId.HasValue) return RedirectToAction("Pagar", new { id = Seleccion.ReservaId });
            model.EsCounter = EsCounter;
            if (model.Personas == null || model.Personas.Count != Seleccion.Cantidad)
                ModelState.AddModelError("", "Complete un pasajero por asiento.");
            else if (model.Personas.Any(p => p == null) || model.Personas.Select(p => p.TipoDocumento + ":" + p.Documento).Distinct().Count() != model.Personas.Count)
                ModelState.AddModelError("", "Los documentos no deben repetirse.");
            string clienteId = User.Identity.GetUserId();
            try
            {
                using (var identidad = new ApplicationDbContext())
                {
                    if (EsCounter)
                    {
                        var cliente = identidad.Users.FirstOrDefault(u => u.UserName == model.UsuarioCliente);
                        var rol = identidad.Roles.FirstOrDefault(r => r.Name == RolesSistema.Cliente);
                        if (cliente == null || rol == null || !cliente.Roles.Any(r => r.RoleId == rol.Id))
                            ModelState.AddModelError("UsuarioCliente", "Seleccione una cuenta con rol Cliente.");
                        else clienteId = cliente.Id;
                    }
                    else if (!identidad.Users.Any(u => u.Id == clienteId)) ModelState.AddModelError("", "La cuenta ya no existe.");
                }
                if (ModelState.IsValid)
                {
                    Reserva.Expirar(db);
                    Reserva.ValidarSeleccion(db, Seleccion, true);
                    Seleccion.Pasajeros = model.Personas;
                    Seleccion.ClienteId = clienteId;
                    return RedirectToAction("Confirmar");
                }
            }
            catch (Exception ex) { ModelState.AddModelError("", Mensaje(ex)); }
            Reserva.PrepararPasajeros(db, Seleccion, model);
            return View(model);
        }

        public ActionResult Confirmar()
        {
            if (Seleccion == null) return RedirectToAction("Buscar", "Vuelos");
            if (Seleccion.ReservaId.HasValue) return RedirectToAction("Pagar", new { id = Seleccion.ReservaId });
            if (Seleccion.Pasajeros.Count != Seleccion.Cantidad || string.IsNullOrEmpty(Seleccion.ClienteId)) return RedirectToAction("Pasajeros");
            try
            {
                Reserva.Expirar(db);
                Reserva.ValidarSeleccion(db, Seleccion, true);
                return View(Reserva.Resumen(db, Seleccion));
            }
            catch (Exception ex) { TempData["ErrorCompra"] = Mensaje(ex); return RedirectToAction("Asientos"); }
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("Confirmar")]
        public ActionResult Registrar(ConfirmacionCompra model)
        {
            if (Seleccion == null) return RedirectToAction("Buscar", "Vuelos");
            if (Seleccion.ReservaId.HasValue) return RedirectToAction(model.Destino == "pagar" ? "Pagar" : "Final", new { id = Seleccion.ReservaId });
            try
            {
                if (!ModelState.IsValid)
                    return View("Confirmar", Reserva.Resumen(db, Seleccion));
                if (model.Clave != Seleccion.Clave || string.IsNullOrEmpty(Seleccion.ClienteId))
                    throw new InvalidOperationException("Complete los pasos anteriores.");
                using (var identidad = new ApplicationDbContext())
                    if (!identidad.Users.Any(u => u.Id == Seleccion.ClienteId)) throw new InvalidOperationException("La cuenta del cliente ya no existe.");
                Reserva.Expirar(db);
                var reserva = Reserva.Registrar(db, Seleccion, Seleccion.ClienteId);
                Seleccion.ReservaId = reserva.ReservaId;
                return RedirectToAction(model.Destino == "pagar" ? "Pagar" : "Final", new { id = reserva.ReservaId });
            }
            catch (Exception ex) { TempData["ErrorCompra"] = Mensaje(ex); return RedirectToAction("Confirmar"); }
        }

        private Reserva Autorizada(int id)
        {
            var reserva = db.Reservas.Find(id);
            if (reserva == null) return null;
            bool counter = EsCounter && Seleccion != null && Seleccion.ReservaId == id && Seleccion.ClienteId == reserva.UsuarioId;
            return reserva.UsuarioId == User.Identity.GetUserId() || counter ? reserva : null;
        }

        public ActionResult Pagar(int id)
        {
            Reserva.Expirar(db);
            var reserva = Autorizada(id);
            if (reserva == null) return HttpNotFound();
            if (reserva.EstadoReservaCodigo != "PENDIENTE") return RedirectToAction("Final", new { id });
            return View(new PagoCompra { ReservaId = id, Reserva = reserva });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Pagar(PagoCompra model)
        {
            Reserva.Expirar(db);
            var reserva = Autorizada(model.ReservaId);
            if (reserva == null) return HttpNotFound();
            if (ModelState.IsValid)
            {
                try { Pago.Simular(db, reserva, model); return RedirectToAction("Final", new { id = reserva.ReservaId }); }
                catch (Exception ex) { ModelState.AddModelError("", Mensaje(ex)); }
            }
            ModelState.SetModelValue("Numero", new ValueProviderResult(null, "", System.Globalization.CultureInfo.InvariantCulture));
            ModelState.SetModelValue("Vencimiento", new ValueProviderResult(null, "", System.Globalization.CultureInfo.InvariantCulture));
            ModelState.SetModelValue("CodigoSeguridad", new ValueProviderResult(null, "", System.Globalization.CultureInfo.InvariantCulture));
            model.Numero = null;
            model.Vencimiento = null;
            model.CodigoSeguridad = null;
            model.Reserva = reserva;
            return View(model);
        }

        public ActionResult Final(int id)
        {
            Reserva.Expirar(db);
            var reserva = Autorizada(id);
            return reserva == null ? (ActionResult)HttpNotFound() : View(reserva.Confirmacion());
        }
        public ActionResult VerReserva(int id)
        {
            Reserva.Expirar(db);
            var reserva = Autorizada(id);
            return reserva == null ? (ActionResult)HttpNotFound() : View(reserva.Confirmacion());
        }
        public ActionResult Comprobante(int id)
        {
            Reserva.Expirar(db);
            var reserva = Autorizada(id);
            if (reserva == null) return HttpNotFound();
            var modelo = reserva.Confirmacion();
            return modelo.Pagada ? View(modelo) : (ActionResult)HttpNotFound();
        }
        private static string Mensaje(Exception ex)
        {
            return ex is InvalidOperationException ? ex.Message : ex is FormatException || ex is OverflowException
                ? "La selección no es válida." : ErroresBaseDatos.ObtenerMensaje(ex);
        }
        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
