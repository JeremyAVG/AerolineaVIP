using System.Web.Mvc;
using AerolineaVIP.Models;

namespace AerolineaVIP.Controllers
{
    [Authorize(Roles = RolesSistema.ClienteCounterYSuper)]
    public class ReservasController : Controller
    {
        // GET: Reservas/MisReservas (pantalla 13, se completa en una fase posterior)
        public ActionResult MisReservas()
        {
            ViewBag.Title = "Mis reservas";
            ViewBag.Pantalla = "Pantalla 13. Consulta de reservas";
            return View("EnConstruccion");
        }

        // GET: Reservas/Asistencia (área de counter)
        [Authorize(Roles = RolesSistema.EmpleadoCounter + "," + RolesSistema.SuperUsuario)]
        public ActionResult Asistencia()
        {
            ViewBag.Title = "Asistencia al cliente";
            ViewBag.Pantalla = "Área de counter: consulta autorizada y asistencia (pantallas 8–15 y 20)";
            return View("EnConstruccion");
        }
    }
}
