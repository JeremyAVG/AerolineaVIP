using System.Web.Mvc;
using AerolineaVIP.Models;

namespace AerolineaVIP.Areas.Admin.Controllers
{
    // Pantalla 17: solo accesos a los módulos (el dashboard está fuera del alcance de la primera entrega)
    [Authorize(Roles = RolesSistema.AdminYSuper)]
    public class PanelController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
