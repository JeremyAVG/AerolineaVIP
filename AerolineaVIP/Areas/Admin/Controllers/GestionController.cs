using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using AerolineaVIP.Models;

namespace AerolineaVIP.Areas.Admin.Controllers
{
    // Pantalla 20: gestión de usuarios, pasajeros y reservas
    [Authorize(Roles = RolesSistema.AdminYSuper)]
    public class GestionController : ControladorAdminBase
    {
        private ApplicationUserManager UserManager
        {
            get { return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
        }

        private ApplicationDbContext ContextoIdentity
        {
            get { return HttpContext.GetOwinContext().Get<ApplicationDbContext>(); }
        }

        // GET: Admin/Gestion?pestana=usuarios|pasajeros|reservas
        public ActionResult Index(string pestana, string buscar, string rol, string estado)
        {
            var model = new GestionViewModel
            {
                Pestana = string.IsNullOrEmpty(pestana) ? "usuarios" : pestana,
                Buscar = buscar,
                FiltroRol = rol,
                FiltroEstado = estado,
                PuedeAsignarRoles = User.IsInRole(RolesSistema.SuperUsuario),
                UsuarioActualId = User.Identity.GetUserId()
            };

            if (model.Pestana == "pasajeros")
            {
                var pasajeros = db.Pasajeroes.Include(p => p.TipoDocumentoIdentidad).Include(p => p.ReservaPasajeroVueloes);
                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    pasajeros = pasajeros.Where(p => p.NumeroDocumento.Contains(buscar) || p.Nombres.Contains(buscar) || p.Apellidos.Contains(buscar));
                }
                model.Pasajeros = pasajeros.OrderBy(p => p.Apellidos).ThenBy(p => p.Nombres).ToList();
            }
            else if (model.Pestana == "reservas")
            {
                var reservas = db.Reservas.Include(r => r.EstadoReserva).Include(r => r.ReservaPasajeroVueloes).Include(r => r.Compras);
                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    reservas = reservas.Where(r => r.CodigoReserva.Contains(buscar));
                }
                if (!string.IsNullOrEmpty(estado))
                {
                    reservas = reservas.Where(r => r.EstadoReservaCodigo == estado);
                }
                model.Reservas = reservas.OrderByDescending(r => r.FechaCreacion).ToList();
                model.NombresUsuario = ContextoIdentity.Users.ToDictionary(u => u.Id, u => u.UserName);
                ViewBag.ListaEstados = new SelectList(db.EstadoReservas.ToList(), "Codigo", "Descripcion", estado);
            }
            else
            {
                model.Pestana = "usuarios";
                model.Usuarios = ObtenerUsuarios(buscar, rol);
                ViewBag.ListaRoles = new SelectList(RolesSistema.Todos, rol);
            }
            return View(model);
        }

        // POST: Admin/Gestion/AsignarRol (solo SuperUsuario)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RolesSistema.SuperUsuario)]
        public ActionResult AsignarRol(string usuarioId, string rol)
        {
            if (!RolesSistema.Todos.Contains(rol))
            {
                TempData["Error"] = "Seleccione un rol válido.";
                return RedirectToAction("Index", new { pestana = "usuarios" });
            }
            if (usuarioId == User.Identity.GetUserId())
            {
                TempData["Error"] = "No puede cambiar su propio rol.";
                return RedirectToAction("Index", new { pestana = "usuarios" });
            }
            var usuario = UserManager.FindById(usuarioId);
            if (usuario == null)
            {
                return HttpNotFound();
            }

            var rolesActuales = UserManager.GetRoles(usuario.Id).ToArray();
            UserManager.RemoveFromRoles(usuario.Id, rolesActuales);
            var resultado = UserManager.AddToRole(usuario.Id, rol);
            if (resultado.Succeeded)
            {
                TempData["Mensaje"] = "Se asignó el rol " + rol + " al usuario " + usuario.UserName + ".";
            }
            else
            {
                UserManager.AddToRoles(usuario.Id, rolesActuales);
                TempData["Error"] = "No se pudo asignar el rol: " + string.Join(" ", resultado.Errors);
            }
            return RedirectToAction("Index", new { pestana = "usuarios" });
        }

        // GET: Admin/Gestion/Reserva/5
        public ActionResult Reserva(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reserva reserva = db.Reservas.Find(id);
            if (reserva == null)
            {
                return HttpNotFound();
            }
            CargarUsuario(reserva);
            return View(reserva);
        }

        // GET: Admin/Gestion/EliminarReserva/5
        public ActionResult EliminarReserva(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reserva reserva = db.Reservas.Find(id);
            if (reserva == null)
            {
                return HttpNotFound();
            }
            CargarUsuario(reserva);
            return View(reserva);
        }

        // POST: Admin/Gestion/EliminarReserva/5
        [HttpPost, ActionName("EliminarReserva")]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarReservaConfirmada(int id)
        {
            Reserva reserva = db.Reservas.Find(id);
            if (reserva == null)
            {
                return HttpNotFound();
            }
            if (reserva.TieneOperaciones)
            {
                ModelState.AddModelError("", "No se puede eliminar la reserva porque tiene asientos, compras o pagos asociados. Las reservas con operaciones se cancelan, no se eliminan.");
                CargarUsuario(reserva);
                return View(reserva);
            }
            db.Reservas.Remove(reserva);
            if (GuardarCambios())
            {
                TempData["Mensaje"] = "Reserva eliminada.";
                return RedirectToAction("Index", new { pestana = "reservas" });
            }
            CargarUsuario(reserva);
            return View(reserva);
        }

        private void CargarUsuario(Reserva reserva)
        {
            var usuario = UserManager.FindById(reserva.UsuarioId);
            ViewBag.Usuario = usuario != null ? usuario.UserName + " (" + usuario.Email + ")" : "(usuario no encontrado)";
        }

        private List<UsuarioGestion> ObtenerUsuarios(string buscar, string rol)
        {
            var nombresRoles = ContextoIdentity.Roles.ToDictionary(r => r.Id, r => r.Name);
            var usuarios = ContextoIdentity.Users.Include(u => u.Roles).OrderBy(u => u.UserName).ToList()
                .Select(u => new UsuarioGestion
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    NombreCompleto = (u.Nombres + " " + u.Apellidos).Trim(),
                    Email = u.Email,
                    Rol = string.Join(", ", u.Roles.Select(r => nombresRoles[r.RoleId])),
                    Bloqueado = u.LockoutEndDateUtc.HasValue && u.LockoutEndDateUtc > System.DateTime.UtcNow
                });

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string texto = buscar.Trim().ToLower();
                usuarios = usuarios.Where(u => u.UserName.ToLower().Contains(texto)
                    || (u.Email ?? "").ToLower().Contains(texto)
                    || u.NombreCompleto.ToLower().Contains(texto));
            }
            if (!string.IsNullOrEmpty(rol))
            {
                usuarios = usuarios.Where(u => u.Rol.Split(new[] { ", " }, System.StringSplitOptions.None).Contains(rol));
            }
            return usuarios.ToList();
        }
    }
}
