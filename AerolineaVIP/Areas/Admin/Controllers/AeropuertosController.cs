using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AerolineaVIP.Models;

namespace AerolineaVIP.Areas.Admin.Controllers
{
    [Authorize(Roles = RolesSistema.AdminYSuper)]
    public class AeropuertosController : ControladorAdminBase
    {
        // GET: Admin/Aeropuertos
        public ActionResult Index()
        {
            return View(db.Aeropuertoes.OrderBy(a => a.CodigoIATA).ToList());
        }

        // GET: Admin/Aeropuertos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Aeropuerto aeropuerto = db.Aeropuertoes.Find(id);
            if (aeropuerto == null)
            {
                return HttpNotFound();
            }
            return View(aeropuerto);
        }

        // GET: Admin/Aeropuertos/Create
        public ActionResult Create()
        {
            return View(new Aeropuerto { Activo = true });
        }

        // POST: Admin/Aeropuertos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AeropuertoId,CodigoIATA,Nombre,Localidad,Activo")] Aeropuerto aeropuerto)
        {
            if (ModelState.IsValid)
            {
                aeropuerto.CodigoIATA = aeropuerto.CodigoIATA.ToUpper();
                db.Aeropuertoes.Add(aeropuerto);
                if (GuardarCambios())
                {
                    TempData["Mensaje"] = "Aeropuerto registrado correctamente.";
                    return RedirectToAction("Index");
                }
            }
            return View(aeropuerto);
        }

        // GET: Admin/Aeropuertos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Aeropuerto aeropuerto = db.Aeropuertoes.Find(id);
            if (aeropuerto == null)
            {
                return HttpNotFound();
            }
            return View(aeropuerto);
        }

        // POST: Admin/Aeropuertos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AeropuertoId,CodigoIATA,Nombre,Localidad,Activo")] Aeropuerto aeropuerto)
        {
            if (ModelState.IsValid)
            {
                aeropuerto.CodigoIATA = aeropuerto.CodigoIATA.ToUpper();
                db.Entry(aeropuerto).State = EntityState.Modified;
                if (GuardarCambios())
                {
                    TempData["Mensaje"] = "Aeropuerto actualizado correctamente.";
                    return RedirectToAction("Index");
                }
            }
            return View(aeropuerto);
        }

        // GET: Admin/Aeropuertos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Aeropuerto aeropuerto = db.Aeropuertoes.Find(id);
            if (aeropuerto == null)
            {
                return HttpNotFound();
            }
            return View(aeropuerto);
        }

        // POST: Admin/Aeropuertos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Aeropuerto aeropuerto = db.Aeropuertoes.Find(id);
            if (aeropuerto.Rutas.Any() || aeropuerto.Rutas1.Any())
            {
                ModelState.AddModelError("", "No se puede eliminar el aeropuerto porque tiene rutas asociadas. Puede marcarlo como inactivo.");
                return View(aeropuerto);
            }
            db.Aeropuertoes.Remove(aeropuerto);
            if (GuardarCambios())
            {
                TempData["Mensaje"] = "Aeropuerto eliminado.";
                return RedirectToAction("Index");
            }
            return View(aeropuerto);
        }
    }
}
