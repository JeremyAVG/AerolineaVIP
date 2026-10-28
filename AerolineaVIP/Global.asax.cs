using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using AerolineaVIP.Models;

namespace AerolineaVIP
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Las tablas de Identity ya existen (Script_BD_AerolineaVIP.sql)
            Database.SetInitializer<ApplicationDbContext>(null);

            CrearRolesYUsuarios();
        }

        private void CrearRolesYUsuarios()
        {
            using (var contexto = new ApplicationDbContext())
            {
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(contexto));
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(contexto));

                foreach (var rol in RolesSistema.Todos)
                {
                    if (!roleManager.RoleExists(rol))
                    {
                        roleManager.Create(new IdentityRole(rol));
                    }
                }

                CrearUsuario(userManager, "superusuario", "superusuario@aerolineavip.test", "Super#2026", "Super", "Usuario", RolesSistema.SuperUsuario);
                CrearUsuario(userManager, "admin01", "admin01@aerolineavip.test", "Admin#2026", "Administrador", "General", RolesSistema.Administrador);
                CrearUsuario(userManager, "counter01", "counter01@aerolineavip.test", "Counter#2026", "Empleado", "Counter", RolesSistema.EmpleadoCounter);
                CrearUsuario(userManager, "cliente01", "cliente01@aerolineavip.test", "Cliente#2026", "Cliente", "Demo", RolesSistema.Cliente);
            }
        }

        private void CrearUsuario(UserManager<ApplicationUser> userManager, string nombreUsuario, string correo,
            string clave, string nombres, string apellidos, string rol)
        {
            var usuario = userManager.FindByName(nombreUsuario);
            if (usuario == null)
            {
                usuario = new ApplicationUser
                {
                    UserName = nombreUsuario,
                    Email = correo,
                    EmailConfirmed = true,
                    Nombres = nombres,
                    Apellidos = apellidos
                };
                var resultado = userManager.Create(usuario, clave);
                if (!resultado.Succeeded)
                {
                    throw new Exception("No se pudo crear el usuario " + nombreUsuario + ": " + string.Join(" ", resultado.Errors));
                }
            }

            if (!userManager.IsInRole(usuario.Id, rol))
            {
                userManager.AddToRole(usuario.Id, rol);
            }
        }
    }
}
