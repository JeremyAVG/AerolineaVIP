using System.Web.Mvc;
using AerolineaVIP.Models;

namespace AerolineaVIP.Controllers
{
    [Authorize(Roles = RolesSistema.ClienteCounterYSuper)]
    public class ComprobantesController : Controller
    {
        // GET: Comprobantes (pantalla 16, se completa en una fase posterior)
        public ActionResult Index()
        {
            ViewBag.Title = "Comprobantes";
            ViewBag.Pantalla = "Pantalla 16. Visualización de comprobantes";
            return View("EnConstruccion");
        }
    }
}
