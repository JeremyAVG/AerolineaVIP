using System.Web.Mvc;

namespace AerolineaVIP.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get { return "Admin"; }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            // /Admin/Index es la ruta sugerida para la pantalla 17
            context.MapRoute(
                "Admin_index",
                "Admin/Index",
                new { controller = "Panel", action = "Index" }
            );

            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { controller = "Panel", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
