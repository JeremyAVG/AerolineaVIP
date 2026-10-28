using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AerolineaVIP.Models;

namespace AerolineaVIP.Areas.Admin.Controllers
{
    [Authorize(Roles = RolesSistema.AdminYSuper)]
    public class ItinerariosController : ControladorAdminBase
    {
        // GET: Admin/Itinerarios
        public ActionResult Index()
        {
            var itinerarios = db.Itinerarios.Include(i => i.EstadoItinerario)
                .Include(i => i.TramoItinerarios.Select(t => t.Vuelo.Frecuencia.Ruta.Aeropuerto))
                .Include(i => i.TramoItinerarios.Select(t => t.Vuelo.Frecuencia.Ruta.Aeropuerto1))
                .OrderByDescending(i => i.Referencia).ToList();
            return View(itinerarios);
        }

        // GET: Admin/Itinerarios/Details/5 (incluye sus tramos)
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Itinerario itinerario = db.Itinerarios.Find(id);
            if (itinerario == null)
            {
                return HttpNotFound();
            }
            return View(itinerario);
        }

        // GET: Admin/Itinerarios/Create
        public ActionResult Create()
        {
            var itinerario = new Itinerario { EstadoItinerarioCodigo = "ACTIVO" };
            CargarListas(itinerario);
            return View(itinerario);
        }

        // POST: Admin/Itinerarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ItinerarioId,Referencia,Nombre,EstadoItinerarioCodigo")] Itinerario itinerario)
        {
            if (ModelState.IsValid)
            {
                db.Itinerarios.Add(itinerario);
                if (GuardarCambios())
                {
                    TempData["Mensaje"] = "Itinerario registrado. Agregue ahora sus tramos.";
                    return RedirectToAction("Details", new { id = itinerario.ItinerarioId });
                }
            }
            CargarListas(itinerario);
            return View(itinerario);
        }

        // GET: Admin/Itinerarios/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Itinerario itinerario = db.Itinerarios.Find(id);
            if (itinerario == null)
            {
                return HttpNotFound();
            }
            CargarListas(itinerario);
            return View(itinerario);
        }

        // POST: Admin/Itinerarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ItinerarioId,Referencia,Nombre,EstadoItinerarioCodigo")] Itinerario itinerario)
        {
            if (ModelState.IsValid)
            {
                db.Entry(itinerario).State = EntityState.Modified;
                if (GuardarCambios())
                {
                    TempData["Mensaje"] = "Itinerario actualizado correctamente.";
                    return RedirectToAction("Index");
                }
            }
            CargarListas(itinerario);
            return View(itinerario);
        }

        // GET: Admin/Itinerarios/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Itinerario itinerario = db.Itinerarios.Find(id);
            if (itinerario == null)
            {
                return HttpNotFound();
            }
            return View(itinerario);
        }

        // POST: Admin/Itinerarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Itinerario itinerario = db.Itinerarios.Find(id);
            if (itinerario.TramoItinerarios.Any())
            {
                ModelState.AddModelError("", "No se puede eliminar el itinerario porque tiene tramos. Elimine primero sus tramos o márquelo como inactivo.");
                return View(itinerario);
            }
            db.Itinerarios.Remove(itinerario);
            if (GuardarCambios())
            {
                TempData["Mensaje"] = "Itinerario eliminado.";
                return RedirectToAction("Index");
            }
            return View(itinerario);
        }

        private void CargarListas(Itinerario itinerario)
        {
            ViewBag.ListaEstados = new SelectList(db.EstadoItinerarios.ToList(), "Codigo", "Descripcion", itinerario.EstadoItinerarioCodigo);
        }
    }
}
