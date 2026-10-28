using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using AerolineaVIP.Models;

namespace AerolineaVIP.Areas.Admin.Controllers
{
    // Pantalla 21: consultas y reportes administrativos (tablas con filtros, sin gráficos)
    [Authorize(Roles = RolesSistema.AdminYSuper)]
    public class ReportesController : ControladorAdminBase
    {
        // GET: Admin/Reportes
        public ActionResult Index()
        {
            return RedirectToAction("Ocupacion");
        }

        // GET: Admin/Reportes/Ocupacion
        public ActionResult Ocupacion(ReporteOcupacionViewModel filtro)
        {
            Reportes.Ocupacion(db, filtro);
            var rutas = db.Rutas.Include(r => r.Aeropuerto).Include(r => r.Aeropuerto1).OrderBy(r => r.RutaId).ToList();
            ViewBag.ListaRutas = new SelectList(rutas, "RutaId", "Descripcion", filtro.RutaId);
            ViewBag.ListaAeronaves = new SelectList(db.Aeronaves.OrderBy(a => a.Matricula).ToList(), "AeronaveId", "Matricula", filtro.AeronaveId);
            ViewBag.ListaEstados = new SelectList(db.EstadoVueloes.ToList(), "Codigo", "Descripcion", filtro.Estado);
            return View(filtro);
        }

        // GET: Admin/Reportes/Reservas
        public ActionResult Reservas(ReporteReservasViewModel filtro)
        {
            Reportes.ReservasPorEstado(db, filtro);
            return View(filtro);
        }

        // GET: Admin/Reportes/Compras
        public ActionResult Compras(ReporteComprasViewModel filtro)
        {
            Reportes.Compras(db, filtro);
            ViewBag.ListaEstados = new SelectList(db.EstadoCompras.ToList(), "Codigo", "Descripcion", filtro.EstadoCompra);
            return View(filtro);
        }
    }
}
