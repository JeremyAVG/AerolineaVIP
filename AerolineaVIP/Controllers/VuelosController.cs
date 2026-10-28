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
            Reserva.Expirar(db);
            var quito = db.Aeropuertoes.FirstOrDefault(a => a.CodigoIATA == "UIO" && a.Activo);
            var model = new BusquedaVuelosViewModel { OrigenId = quito != null ? quito.AeropuertoId : (int?)null, Pasajeros = 1 };
            BusquedaVuelos.CargarListas(db, model);
            var proximaFecha = db.Vueloes.Where(v => v.FechaOperacion >= System.DateTime.Today && v.AeronaveId != null)
                .OrderBy(v => v.FechaOperacion).Select(v => (System.DateTime?)v.FechaOperacion).FirstOrDefault();
            if (proximaFecha.HasValue) model.Sugerencias = Reserva.Opciones(db, proximaFecha.Value, 1).Take(4).ToList();
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

            Reserva.Expirar(db);
            var opciones = Reserva.Opciones(db, model.Fecha.Value.Date, model.Pasajeros.Value);
            opciones = opciones.Where(o => db.Vueloes.Find(o.Vuelos.First()).Frecuencia.Ruta.AeropuertoOrigenId == model.OrigenId
                && db.Vueloes.Find(o.Vuelos.Last()).Frecuencia.Ruta.AeropuertoDestinoId == model.DestinoId).ToList();
            return View(new ResultadosCompra { Opciones = opciones });
        }

        public ActionResult Disponibilidad(string clave, int cantidad = 1)
        {
            if (cantidad < 1 || cantidad > 7) return new HttpStatusCodeResult(400);
            Reserva.Expirar(db);
            var valores = (clave ?? "").Split(',');
            int primero;
            if (!int.TryParse(valores[0], out primero)) return HttpNotFound();
            var vuelo = db.Vueloes.Find(primero);
            if (vuelo == null) return HttpNotFound();
            var opcion = Reserva.Opciones(db, vuelo.FechaOperacion, cantidad, false).FirstOrDefault(o => o.Clave == clave);
            if (opcion == null) return HttpNotFound();
            var ultimo = db.Vueloes.Find(opcion.Vuelos.Last());
            return View(new DisponibilidadCompra { Opcion = opcion, SinCupos = opcion.Disponibles < cantidad,
                Mapas = opcion.Vuelos.Select(id => Reserva.Mapa(db, db.Vueloes.Find(id), false)).ToList(),
                Calculado = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                OrigenId = vuelo.Frecuencia.Ruta.AeropuertoOrigenId, DestinoId = ultimo.Frecuencia.Ruta.AeropuertoDestinoId,
                Fecha = vuelo.FechaOperacion.ToString("yyyy-MM-dd") });
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
