using System.Linq;
using System.Web.Mvc;
using AerolineaVIP.Models;

namespace AerolineaVIP.Controllers
{
    public class VuelosController : Controller
    {
        private AerolineaVIPEntities db = new AerolineaVIPEntities();

        // GET: Vuelos/Buscar (pantalla 5)
        public ActionResult Buscar()
        {
            var quito = db.Aeropuertoes.FirstOrDefault(a => a.CodigoIATA == "UIO" && a.Activo);
            var model = new BusquedaVuelosViewModel { OrigenId = quito != null ? quito.AeropuertoId : (int?)null, Pasajeros = 1 };
            BusquedaVuelos.CargarListas(db, model);
            ViewBag.Destinos = DestinosDesdeQuito();
            return View(model);
        }

        // GET: Vuelos/Resultados (pantalla 6, se completa en la fase 3)
        public ActionResult Resultados(BusquedaVuelosViewModel model)
        {
            if (model.OrigenId.HasValue && model.DestinoId.HasValue)
            {
                string error = BusquedaVuelos.ValidarRuta(db, model);
                if (error != null)
                {
                    ModelState.AddModelError("DestinoId", error);
                }
            }

            if (!ModelState.IsValid)
            {
                BusquedaVuelos.CargarListas(db, model);
                ViewBag.Destinos = DestinosDesdeQuito();
                return View("Buscar", model);
            }

            ViewBag.Origen = db.Aeropuertoes.Find(model.OrigenId).Descripcion;
            ViewBag.Destino = db.Aeropuertoes.Find(model.DestinoId).Descripcion;
            return View(model);
        }

        // GET: Vuelos/Destinos?origenId=1 (usado por jQuery)
        public JsonResult Destinos(int origenId)
        {
            var destinos = Ruta.ObtenerDestinos(db, origenId)
                .Select(d => new { id = d.AeropuertoId, texto = d.Texto });
            return Json(destinos, JsonRequestBehavior.AllowGet);
        }

        private System.Collections.Generic.List<DestinoDisponible> DestinosDesdeQuito()
        {
            var quito = db.Aeropuertoes.FirstOrDefault(a => a.CodigoIATA == "UIO");
            return quito != null ? Ruta.ObtenerDestinos(db, quito.AeropuertoId) : new System.Collections.Generic.List<DestinoDisponible>();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
