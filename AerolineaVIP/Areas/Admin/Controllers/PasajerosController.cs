using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AerolineaVIP.Models;

namespace AerolineaVIP.Areas.Admin.Controllers
{
    [Authorize(Roles = RolesSistema.AdminYSuper)]
    public class PasajerosController : ControladorAdminBase
    {
        // GET: Admin/Pasajeros?buscar=
        public ActionResult Index(string buscar)
        {
            var pasajeros = db.Pasajeroes.Include(p => p.TipoDocumentoIdentidad);
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar = buscar.Trim();
                pasajeros = pasajeros.Where(p => p.NumeroDocumento.Contains(buscar) || p.Nombres.Contains(buscar) || p.Apellidos.Contains(buscar));
            }
            ViewBag.Buscar = buscar;
            return View(pasajeros.OrderBy(p => p.Apellidos).ThenBy(p => p.Nombres).ToList());
        }

        // GET: Admin/Pasajeros/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pasajero pasajero = db.Pasajeroes.Find(id);
            if (pasajero == null)
            {
                return HttpNotFound();
            }
            return View(pasajero);
        }

        // GET: Admin/Pasajeros/Create
        public ActionResult Create()
        {
            var pasajero = new Pasajero { TipoDocumentoCodigo = "CEDULA" };
            CargarListas(pasajero);
            return View(pasajero);
        }

        // POST: Admin/Pasajeros/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PasajeroId,TipoDocumentoCodigo,NumeroDocumento,Nombres,Apellidos,FechaNacimiento,Correo,Telefono")] Pasajero pasajero)
        {
            if (ModelState.IsValid)
            {
                pasajero.FechaRegistro = DateTime.Now;
                db.Pasajeroes.Add(pasajero);
                if (GuardarCambios())
                {
                    TempData["Mensaje"] = "Pasajero registrado correctamente.";
                    return RedirectToAction("Index");
                }
                db.Pasajeroes.Remove(pasajero);
            }
            CargarListas(pasajero);
            return View(pasajero);
        }

        // GET: Admin/Pasajeros/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pasajero pasajero = db.Pasajeroes.Find(id);
            if (pasajero == null)
            {
                return HttpNotFound();
            }
            CargarListas(pasajero);
            return View(pasajero);
        }

        // POST: Admin/Pasajeros/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PasajeroId,TipoDocumentoCodigo,NumeroDocumento,Nombres,Apellidos,FechaNacimiento,Correo,Telefono")] Pasajero pasajero)
        {
            if (ModelState.IsValid)
            {
                db.Entry(pasajero).State = EntityState.Modified;
                // La fecha de registro no se modifica
                db.Entry(pasajero).Property(p => p.FechaRegistro).IsModified = false;
                if (GuardarCambios())
                {
                    TempData["Mensaje"] = "Pasajero actualizado correctamente.";
                    return RedirectToAction("Index");
                }
            }
            CargarListas(pasajero);
            return View(pasajero);
        }

        // GET: Admin/Pasajeros/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pasajero pasajero = db.Pasajeroes.Find(id);
            if (pasajero == null)
            {
                return HttpNotFound();
            }
            return View(pasajero);
        }

        // POST: Admin/Pasajeros/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Pasajero pasajero = db.Pasajeroes.Find(id);
            if (pasajero.TieneOperaciones)
            {
                ModelState.AddModelError("", "No se puede eliminar el pasajero porque tiene reservas o asientos asociados.");
                return View(pasajero);
            }
            db.Pasajeroes.Remove(pasajero);
            if (GuardarCambios())
            {
                TempData["Mensaje"] = "Pasajero eliminado.";
                return RedirectToAction("Index");
            }
            return View(pasajero);
        }

        private void CargarListas(Pasajero pasajero)
        {
            ViewBag.ListaTiposDocumento = new SelectList(db.TipoDocumentoIdentidads.ToList(), "Codigo", "Descripcion", pasajero.TipoDocumentoCodigo);
        }
    }
}
