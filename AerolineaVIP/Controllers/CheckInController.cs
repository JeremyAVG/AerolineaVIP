using System.Web.Mvc;
using AerolineaVIP.Models;

namespace AerolineaVIP.Controllers
{
    [Authorize(Roles = RolesSistema.ClienteCounterYSuper)]
    public class CheckInController : Controller
    {
        // GET: CheckIn (pantalla 15, se completa en una fase posterior)
        public ActionResult Index()
        {
            ViewBag.Title = "Check-in";
            ViewBag.Pantalla = "Pantalla 15. Check-in";
            return View("EnConstruccion");
        }
    }
}
