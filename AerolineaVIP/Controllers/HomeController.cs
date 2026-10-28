using System.Linq;
using System.Web.Mvc;
using AerolineaVIP.Models;

namespace AerolineaVIP.Controllers
{
    public class HomeController : Controller
    {
        private AerolineaVIPEntities db = new AerolineaVIPEntities();

        public ActionResult Index()
        {
            var quito = db.Aeropuertoes.FirstOrDefault(a => a.CodigoIATA == "UIO" && a.Activo);
            var busqueda = new BusquedaVuelosViewModel { OrigenId = quito != null ? quito.AeropuertoId : (int?)null, Pasajeros = 1 };
            BusquedaVuelos.CargarListas(db, busqueda);

            var model = new HomeIndexViewModel
            {
                Busqueda = busqueda,
                CiudadOrigen = quito != null ? quito.Localidad : "",
                Destinos = quito != null ? Ruta.ObtenerDestinos(db, quito.AeropuertoId) : new System.Collections.Generic.List<DestinoDisponible>()
            };
            return View(model);
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
