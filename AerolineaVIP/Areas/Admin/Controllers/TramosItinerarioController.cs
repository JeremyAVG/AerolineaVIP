using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AerolineaVIP.Models;

namespace AerolineaVIP.Areas.Admin.Controllers
{
    [Authorize(Roles = RolesSistema.AdminYSuper)]
    public class TramosItinerarioController : ControladorAdminBase
    {
        // GET: Admin/TramosItinerario?itinerarioId=1
        public ActionResult Index(int? itinerarioId)
        {
            var tramos = db.TramoItinerarios.Include(t => t.Itinerario)
                .Include(t => t.Vuelo.Frecuencia.Ruta.Aeropuerto).Include(t => t.Vuelo.Frecuencia.Ruta.Aeropuerto1);
            if (itinerarioId.HasValue)
            {
                tramos = tramos.Where(t => t.ItinerarioId == itinerarioId);
            }
            ViewBag.ListaItinerarios = ListaItinerarios(itinerarioId);
            return View(tramos.OrderByDescending(t => t.Itinerario.Referencia).ThenBy(t => t.Orden).ToList());
        }

        // GET: Admin/TramosItinerario/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TramoItinerario tramo = db.TramoItinerarios.Find(id);
            if (tramo == null)
            {
                return HttpNotFound();
            }
            return View(tramo);
        }

        // GET: Admin/TramosItinerario/Create?itinerarioId=1
        public ActionResult Create(int? itinerarioId)
        {
            var tramo = new TramoItinerario();
            if (itinerarioId.HasValue)
            {
                tramo.ItinerarioId = itinerarioId.Value;
                var ultimo = db.TramoItinerarios.Where(t => t.ItinerarioId == itinerarioId).Select(t => (int?)t.Orden).Max();
                tramo.Orden = (byte)((ultimo ?? 0) + 1);
            }
            CargarListas(tramo);
            return View(tramo);
        }

        // POST: Admin/TramosItinerario/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TramoItinerarioId,ItinerarioId,VueloId,Orden")] TramoItinerario tramo)
        {
            if (ModelState.IsValid)
            {
                db.TramoItinerarios.Add(tramo);
                if (GuardarCambios())
                {
                    TempData["Mensaje"] = "Tramo agregado correctamente.";
                    return RedirectToAction("Details", "Itinerarios", new { id = tramo.ItinerarioId });
                }
                db.TramoItinerarios.Remove(tramo);
            }
            CargarListas(tramo);
            return View(tramo);
        }

        // GET: Admin/TramosItinerario/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TramoItinerario tramo = db.TramoItinerarios.Find(id);
            if (tramo == null)
            {
                return HttpNotFound();
            }
            CargarListas(tramo);
            return View(tramo);
        }

        // POST: Admin/TramosItinerario/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TramoItinerarioId,ItinerarioId,VueloId,Orden")] TramoItinerario tramo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tramo).State = EntityState.Modified;
                if (GuardarCambios())
                {
                    TempData["Mensaje"] = "Tramo actualizado correctamente.";
                    return RedirectToAction("Details", "Itinerarios", new { id = tramo.ItinerarioId });
                }
                db.Entry(tramo).State = EntityState.Detached;
            }
            CargarListas(tramo);
            return View(tramo);
        }

        // GET: Admin/TramosItinerario/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TramoItinerario tramo = db.TramoItinerarios.Find(id);
            if (tramo == null)
            {
                return HttpNotFound();
            }
            return View(tramo);
        }

        // POST: Admin/TramosItinerario/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TramoItinerario tramo = db.TramoItinerarios.Find(id);
            int itinerarioId = tramo.ItinerarioId;
            db.TramoItinerarios.Remove(tramo);
            if (GuardarCambios())
            {
                TempData["Mensaje"] = "Tramo eliminado.";
                return RedirectToAction("Details", "Itinerarios", new { id = itinerarioId });
            }
            // El trigger rechazó la eliminación (por ejemplo, rompe la continuidad del itinerario)
            db.Entry(tramo).State = EntityState.Unchanged;
            return View(tramo);
        }

        private SelectList ListaItinerarios(int? seleccionado)
        {
            var itinerarios = db.Itinerarios.OrderByDescending(i => i.Referencia).ToList()
                .Select(i => new { i.ItinerarioId, Texto = i.Referencia + " · " + i.Nombre });
            return new SelectList(itinerarios, "ItinerarioId", "Texto", seleccionado);
        }

        private void CargarListas(TramoItinerario tramo)
        {
            ViewBag.ListaItinerarios = ListaItinerarios(tramo.ItinerarioId);

            // Vuelos desde hoy y, si existe, el que ya tiene el tramo
            DateTime hoy = DateTime.Today;
            var vuelos = db.Vueloes.Include(v => v.Frecuencia.Ruta.Aeropuerto).Include(v => v.Frecuencia.Ruta.Aeropuerto1)
                .Where(v => v.FechaOperacion >= hoy || v.VueloId == tramo.VueloId)
                .OrderBy(v => v.FechaOperacion).ThenBy(v => v.Frecuencia.HoraSalida).ToList();
            ViewBag.ListaVuelos = new SelectList(vuelos, "VueloId", "Descripcion", tramo.VueloId);
        }
    }
}
