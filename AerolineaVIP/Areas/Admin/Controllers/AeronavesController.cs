using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AerolineaVIP.Models;

namespace AerolineaVIP.Areas.Admin.Controllers
{
    // Pantalla 19: administración de aeronaves
    [Authorize(Roles = RolesSistema.AdminYSuper)]
    public class AeronavesController : ControladorAdminBase
    {
        // GET: Admin/Aeronaves
        public ActionResult Index()
        {
            var aeronaves = db.Aeronaves.Include(a => a.ModeloAeronave).Include(a => a.EstadoAeronave)
                .OrderBy(a => a.Matricula).ToList();
            return View(aeronaves);
        }

        // GET: Admin/Aeronaves/Details/5 (distribución de asientos y asignaciones)
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Aeronave aeronave = db.Aeronaves.Find(id);
            if (aeronave == null)
            {
                return HttpNotFound();
            }
            ViewBag.Asientos = db.vw_AsientoAeronave.Where(a => a.AeronaveId == id)
                .OrderBy(a => a.NumeroAsiento).ToList();
            ViewBag.Vuelos = aeronave.VuelosDesdeHoy;
            return View(aeronave);
        }

        // GET: Admin/Aeronaves/Create
        public ActionResult Create()
        {
            var aeronave = new Aeronave { EstadoAeronaveCodigo = "DISPONIBLE" };
            CargarListas(aeronave);
            return View(aeronave);
        }

        // POST: Admin/Aeronaves/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AeronaveId,Matricula,ModeloAeronaveId,EstadoAeronaveCodigo")] Aeronave aeronave)
        {
            if (ModelState.IsValid)
            {
                aeronave.Matricula = aeronave.Matricula.ToUpper();
                db.Aeronaves.Add(aeronave);
                if (GuardarCambios())
                {
                    TempData["Mensaje"] = "Aeronave registrada correctamente.";
                    return RedirectToAction("Index");
                }
            }
            CargarListas(aeronave);
            return View(aeronave);
        }

        // GET: Admin/Aeronaves/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Aeronave aeronave = db.Aeronaves.Find(id);
            if (aeronave == null)
            {
                return HttpNotFound();
            }
            CargarListas(aeronave);
            return View(aeronave);
        }

        // POST: Admin/Aeronaves/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AeronaveId,Matricula,ModeloAeronaveId,EstadoAeronaveCodigo")] Aeronave aeronave)
        {
            if (ModelState.IsValid)
            {
                aeronave.Matricula = aeronave.Matricula.ToUpper();
                db.Entry(aeronave).State = EntityState.Modified;
                if (GuardarCambios())
                {
                    TempData["Mensaje"] = "Aeronave actualizada correctamente.";
                    return RedirectToAction("Index");
                }
            }
            CargarListas(aeronave);
            return View(aeronave);
        }

        // GET: Admin/Aeronaves/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Aeronave aeronave = db.Aeronaves.Find(id);
            if (aeronave == null)
            {
                return HttpNotFound();
            }
            return View(aeronave);
        }

        // POST: Admin/Aeronaves/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Aeronave aeronave = db.Aeronaves.Find(id);
            if (aeronave.Vueloes.Any())
            {
                ModelState.AddModelError("", "No se puede eliminar la aeronave porque tiene vuelos asignados. Puede cambiar su estado a no disponible.");
                return View(aeronave);
            }
            db.Aeronaves.Remove(aeronave);
            if (GuardarCambios())
            {
                TempData["Mensaje"] = "Aeronave eliminada.";
                return RedirectToAction("Index");
            }
            return View(aeronave);
        }

        private void CargarListas(Aeronave aeronave)
        {
            var modelos = db.ModeloAeronaves.ToList()
                .Select(m => new { m.ModeloAeronaveId, Texto = m.Fabricante + " " + m.Nombre + " (" + m.CapacidadAsientos + " asientos)" });
            ViewBag.ListaModelos = new SelectList(modelos, "ModeloAeronaveId", "Texto", aeronave.ModeloAeronaveId);
            ViewBag.ListaEstados = new SelectList(db.EstadoAeronaves.ToList(), "Codigo", "Descripcion", aeronave.EstadoAeronaveCodigo);
        }
    }
}
