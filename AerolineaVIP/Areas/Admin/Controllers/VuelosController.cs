using System;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AerolineaVIP.Models;

namespace AerolineaVIP.Areas.Admin.Controllers
{
    // Pantalla 18: administración de vuelos e itinerarios
    [Authorize(Roles = RolesSistema.AdminYSuper)]
    public class VuelosController : ControladorAdminBase
    {
        // GET: Admin/Vuelos
        public ActionResult Index(FiltroVuelosViewModel filtro)
        {
            if (!filtro.Desde.HasValue && !filtro.Hasta.HasValue && Request.QueryString.Count == 0)
            {
                filtro.Desde = DateTime.Today;
            }
            filtro.Generar = filtro.Generar ?? new GenerarVuelosViewModel { FechaInicio = DateTime.Today, FechaFin = DateTime.Today.AddDays(27) };
            CargarIndice(filtro);
            return View(filtro);
        }

        // POST: Admin/Vuelos/Generar (ejecuta dbo.usp_GenerarVuelos)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Generar([Bind(Prefix = "Generar")] GenerarVuelosViewModel generar)
        {
            if (!ModelState.IsValid)
            {
                var filtro = new FiltroVuelosViewModel { Desde = DateTime.Today, Generar = generar };
                CargarIndice(filtro);
                return View("Index", filtro);
            }

            int antes = db.Vueloes.Count();
            string error = EjecutarComando(() => db.Database.ExecuteSqlCommand(
                "EXEC dbo.usp_GenerarVuelos @FechaInicio, @FechaFin",
                new SqlParameter("@FechaInicio", SqlDbType.Date) { Value = generar.FechaInicio.Value },
                new SqlParameter("@FechaFin", SqlDbType.Date) { Value = generar.FechaFin.Value }));

            if (error != null)
            {
                ModelState.AddModelError("", error);
                var filtro = new FiltroVuelosViewModel { Desde = DateTime.Today, Generar = generar };
                CargarIndice(filtro);
                return View("Index", filtro);
            }

            int generados = db.Vueloes.Count() - antes;
            TempData["Mensaje"] = generados == 0
                ? "No se generaron vuelos nuevos: ya existían todos los vuelos de las frecuencias activas en ese rango."
                : "Se generaron " + generados + " vuelos. Recuerde asignarles una aeronave antes de comercializarlos.";
            return RedirectToAction("Index", new { Desde = generar.FechaInicio.Value.ToString("yyyy-MM-dd"), Hasta = generar.FechaFin.Value.ToString("yyyy-MM-dd") });
        }

        // GET: Admin/Vuelos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vuelo vuelo = db.Vueloes.Find(id);
            if (vuelo == null)
            {
                return HttpNotFound();
            }
            return View(vuelo);
        }

        // GET: Admin/Vuelos/Create
        public ActionResult Create()
        {
            var vuelo = new Vuelo { FechaOperacion = DateTime.Today, EstadoVueloCodigo = "PROGRAMADO" };
            CargarListas(vuelo);
            return View(vuelo);
        }

        // POST: Admin/Vuelos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "VueloId,FrecuenciaId,FechaOperacion,AeronaveId,EstadoVueloCodigo")] Vuelo vuelo)
        {
            if (ModelState.IsValid)
            {
                db.Vueloes.Add(vuelo);
                if (GuardarCambios())
                {
                    TempData["Mensaje"] = "Vuelo registrado correctamente.";
                    return RedirectToAction("Index");
                }
                db.Vueloes.Remove(vuelo);
            }
            CargarListas(vuelo);
            return View(vuelo);
        }

        // GET: Admin/Vuelos/Edit/5 (asignación de aeronave y estado)
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vuelo vuelo = db.Vueloes.Find(id);
            if (vuelo == null)
            {
                return HttpNotFound();
            }
            CargarListas(vuelo);
            return View(vuelo);
        }

        // POST: Admin/Vuelos/Edit/5
        // Solo se reciben la aeronave y el estado; la frecuencia y la fecha no se modifican aquí
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int vueloId, int? aeronaveId, string estadoVueloCodigo)
        {
            Vuelo vuelo = db.Vueloes.Find(vueloId);
            if (vuelo == null)
            {
                return HttpNotFound();
            }
            if (!db.EstadoVueloes.Any(e => e.Codigo == estadoVueloCodigo))
            {
                ModelState.AddModelError("EstadoVueloCodigo", "Seleccione un estado válido.");
            }
            if (aeronaveId.HasValue && !db.Aeronaves.Any(a => a.AeronaveId == aeronaveId))
            {
                ModelState.AddModelError("AeronaveId", "Seleccione una aeronave válida.");
            }

            if (ModelState.IsValid)
            {
                vuelo.AeronaveId = aeronaveId;
                vuelo.EstadoVueloCodigo = estadoVueloCodigo;
                if (GuardarCambios())
                {
                    TempData["Mensaje"] = "Vuelo " + vuelo.CodigoVuelo + " actualizado correctamente.";
                    return RedirectToAction("Index", new { Desde = vuelo.FechaOperacion.ToString("yyyy-MM-dd") });
                }
                // Se descartan los cambios rechazados para mostrar la asignación vigente
                db.Entry(vuelo).Reload();
                ModelState.Remove("aeronaveId");
                ModelState.Remove("estadoVueloCodigo");
                ViewBag.Rechazado = true;
            }
            CargarListas(vuelo);
            return View(vuelo);
        }

        // GET: Admin/Vuelos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vuelo vuelo = db.Vueloes.Find(id);
            if (vuelo == null)
            {
                return HttpNotFound();
            }
            return View(vuelo);
        }

        // POST: Admin/Vuelos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Vuelo vuelo = db.Vueloes.Find(id);
            if (vuelo.TieneReservas || vuelo.TramoItinerarios.Any())
            {
                ModelState.AddModelError("", "No se puede eliminar el vuelo porque tiene reservas o forma parte de un itinerario. Puede cambiar su estado a cancelado.");
                return View(vuelo);
            }
            db.Vueloes.Remove(vuelo);
            if (GuardarCambios())
            {
                TempData["Mensaje"] = "Vuelo eliminado.";
                return RedirectToAction("Index");
            }
            return View(vuelo);
        }

        private void CargarIndice(FiltroVuelosViewModel filtro)
        {
            var vuelos = db.Vueloes.Include(v => v.Frecuencia.Ruta.Aeropuerto).Include(v => v.Frecuencia.Ruta.Aeropuerto1)
                .Include(v => v.Aeronave).Include(v => v.EstadoVuelo);
            if (filtro.Desde.HasValue)
            {
                vuelos = vuelos.Where(v => v.FechaOperacion >= filtro.Desde.Value);
            }
            if (filtro.Hasta.HasValue)
            {
                vuelos = vuelos.Where(v => v.FechaOperacion <= filtro.Hasta.Value);
            }
            if (filtro.RutaId.HasValue)
            {
                vuelos = vuelos.Where(v => v.Frecuencia.RutaId == filtro.RutaId);
            }
            if (!string.IsNullOrEmpty(filtro.Estado))
            {
                vuelos = vuelos.Where(v => v.EstadoVueloCodigo == filtro.Estado);
            }
            filtro.Vuelos = vuelos.OrderBy(v => v.FechaOperacion).ThenBy(v => v.Frecuencia.HoraSalida).ToList();

            var ids = filtro.Vuelos.Select(v => v.VueloId).ToList();
            filtro.Disponibilidad = db.vw_DisponibilidadVuelo.Where(d => ids.Contains(d.VueloId)).ToList()
                .ToDictionary(d => d.VueloId);

            var rutas = db.Rutas.Include(r => r.Aeropuerto).Include(r => r.Aeropuerto1).OrderBy(r => r.RutaId).ToList();
            ViewBag.ListaRutas = new SelectList(rutas, "RutaId", "Descripcion", filtro.RutaId);
            ViewBag.ListaEstados = new SelectList(db.EstadoVueloes.ToList(), "Codigo", "Descripcion", filtro.Estado);
        }

        private void CargarListas(Vuelo vuelo)
        {
            var frecuencias = db.Frecuencias.Include(f => f.Ruta.Aeropuerto).Include(f => f.Ruta.Aeropuerto1)
                .OrderBy(f => f.RutaId).ThenBy(f => f.DiaSemana).ThenBy(f => f.HoraSalida).ToList();
            ViewBag.ListaFrecuencias = new SelectList(frecuencias, "FrecuenciaId", "Descripcion", vuelo.FrecuenciaId);

            // Aeronaves disponibles y, si existe, la que ya tiene asignada el vuelo
            var aeronaves = db.Aeronaves.Include(a => a.ModeloAeronave)
                .Where(a => a.EstadoAeronaveCodigo == "DISPONIBLE" || a.AeronaveId == vuelo.AeronaveId)
                .OrderBy(a => a.Matricula).ToList();
            ViewBag.ListaAeronaves = new SelectList(aeronaves, "AeronaveId", "Descripcion", vuelo.AeronaveId);
            ViewBag.ListaEstados = new SelectList(db.EstadoVueloes.ToList(), "Codigo", "Descripcion", vuelo.EstadoVueloCodigo);
        }
    }
}
