using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AerolineaVIP.Models;

namespace AerolineaVIP.Areas.Admin.Controllers
{
    [Authorize(Roles = RolesSistema.AdminYSuper)]
    public class RutasController : ControladorAdminBase
    {
        // GET: Admin/Rutas
        public ActionResult Index()
        {
            var rutas = db.Rutas.Include(r => r.Aeropuerto).Include(r => r.Aeropuerto1)
                .OrderBy(r => r.RutaId).ToList();
            return View(rutas);
        }

        // GET: Admin/Rutas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ruta ruta = db.Rutas.Find(id);
            if (ruta == null)
            {
                return HttpNotFound();
            }
            return View(ruta);
        }

        // GET: Admin/Rutas/Create
        public ActionResult Create()
        {
            var ruta = new Ruta { Activa = true };
            CargarListas(ruta);
            return View(ruta);
        }

        // POST: Admin/Rutas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "RutaId,AeropuertoOrigenId,AeropuertoDestinoId,TarifaReferencial,Activa")] Ruta ruta)
        {
            if (ModelState.IsValid)
            {
                db.Rutas.Add(ruta);
                if (GuardarCambios())
                {
                    TempData["Mensaje"] = "Ruta registrada correctamente.";
                    return RedirectToAction("Index");
                }
            }
            CargarListas(ruta);
            return View(ruta);
        }

        // GET: Admin/Rutas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ruta ruta = db.Rutas.Find(id);
            if (ruta == null)
            {
                return HttpNotFound();
            }
            CargarListas(ruta);
            return View(ruta);
        }

        // POST: Admin/Rutas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "RutaId,AeropuertoOrigenId,AeropuertoDestinoId,TarifaReferencial,Activa")] Ruta ruta)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ruta).State = EntityState.Modified;
                if (GuardarCambios())
                {
                    TempData["Mensaje"] = "Ruta actualizada correctamente.";
                    return RedirectToAction("Index");
                }
            }
            CargarListas(ruta);
            return View(ruta);
        }

        // GET: Admin/Rutas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ruta ruta = db.Rutas.Find(id);
            if (ruta == null)
            {
                return HttpNotFound();
            }
            return View(ruta);
        }

        // POST: Admin/Rutas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ruta ruta = db.Rutas.Find(id);
            if (ruta.Frecuencias.Any())
            {
                ModelState.AddModelError("", "No se puede eliminar la ruta porque tiene frecuencias asociadas. Puede marcarla como inactiva.");
                return View(ruta);
            }
            db.Rutas.Remove(ruta);
            if (GuardarCambios())
            {
                TempData["Mensaje"] = "Ruta eliminada.";
                return RedirectToAction("Index");
            }
            return View(ruta);
        }

        private void CargarListas(Ruta ruta)
        {
            var aeropuertos = db.Aeropuertoes.OrderBy(a => a.Localidad).ToList();
            ViewBag.ListaOrigen = new SelectList(aeropuertos, "AeropuertoId", "Descripcion", ruta.AeropuertoOrigenId);
            ViewBag.ListaDestino = new SelectList(aeropuertos, "AeropuertoId", "Descripcion", ruta.AeropuertoDestinoId);
        }
    }
}
