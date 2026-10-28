using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AerolineaVIP.Models;

namespace AerolineaVIP.Areas.Admin.Controllers
{
    [Authorize(Roles = RolesSistema.AdminYSuper)]
    public class FrecuenciasController : ControladorAdminBase
    {
        // GET: Admin/Frecuencias?rutaId=1
        public ActionResult Index(int? rutaId)
        {
            var frecuencias = db.Frecuencias.Include(f => f.Ruta.Aeropuerto).Include(f => f.Ruta.Aeropuerto1);
            if (rutaId.HasValue)
            {
                frecuencias = frecuencias.Where(f => f.RutaId == rutaId);
            }
            ViewBag.ListaRutas = ListaRutas(rutaId);
            return View(frecuencias.OrderBy(f => f.RutaId).ThenBy(f => f.DiaSemana).ThenBy(f => f.HoraSalida).ToList());
        }

        // GET: Admin/Frecuencias/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Frecuencia frecuencia = db.Frecuencias.Find(id);
            if (frecuencia == null)
            {
                return HttpNotFound();
            }
            return View(frecuencia);
        }

        // GET: Admin/Frecuencias/Create
        public ActionResult Create()
        {
            var frecuencia = new Frecuencia { Activa = true };
            CargarListas(frecuencia);
            return View(frecuencia);
        }

        // POST: Admin/Frecuencias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FrecuenciaId,RutaId,DiaSemana,HoraSalida,HoraLlegada,Activa")] Frecuencia frecuencia)
        {
            if (ModelState.IsValid)
            {
                db.Frecuencias.Add(frecuencia);
                if (GuardarCambios())
                {
                    TempData["Mensaje"] = "Frecuencia registrada correctamente.";
                    return RedirectToAction("Index");
                }
            }
            CargarListas(frecuencia);
            return View(frecuencia);
        }

        // GET: Admin/Frecuencias/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Frecuencia frecuencia = db.Frecuencias.Find(id);
            if (frecuencia == null)
            {
                return HttpNotFound();
            }
            CargarListas(frecuencia);
            return View(frecuencia);
        }

        // POST: Admin/Frecuencias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "FrecuenciaId,RutaId,DiaSemana,HoraSalida,HoraLlegada,Activa")] Frecuencia frecuencia)
        {
            if (ModelState.IsValid)
            {
                db.Entry(frecuencia).State = EntityState.Modified;
                if (GuardarCambios())
                {
                    TempData["Mensaje"] = "Frecuencia actualizada correctamente.";
                    return RedirectToAction("Index");
                }
            }
            CargarListas(frecuencia);
            return View(frecuencia);
        }

        // GET: Admin/Frecuencias/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Frecuencia frecuencia = db.Frecuencias.Find(id);
            if (frecuencia == null)
            {
                return HttpNotFound();
            }
            return View(frecuencia);
        }

        // POST: Admin/Frecuencias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Frecuencia frecuencia = db.Frecuencias.Find(id);
            if (frecuencia.Vueloes.Any())
            {
                ModelState.AddModelError("", "No se puede eliminar la frecuencia porque tiene vuelos programados. Puede marcarla como inactiva.");
                return View(frecuencia);
            }
            db.Frecuencias.Remove(frecuencia);
            if (GuardarCambios())
            {
                TempData["Mensaje"] = "Frecuencia eliminada.";
                return RedirectToAction("Index");
            }
            return View(frecuencia);
        }

        private SelectList ListaRutas(int? seleccionada)
        {
            var rutas = db.Rutas.Include(r => r.Aeropuerto).Include(r => r.Aeropuerto1).OrderBy(r => r.RutaId).ToList();
            return new SelectList(rutas, "RutaId", "Descripcion", seleccionada);
        }

        private void CargarListas(Frecuencia frecuencia)
        {
            ViewBag.ListaRutas = ListaRutas(frecuencia.RutaId);
            ViewBag.ListaDias = new SelectList(Frecuencia.DiasSemana(), "Key", "Value", frecuencia.DiaSemana);
        }
    }
}
