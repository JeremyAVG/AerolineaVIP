using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AerolineaVIP.Models;

namespace AerolineaVIP.Areas.Admin.Controllers
{
    // Catálogos de estados y tipos: solo lectura (se definen en el script de la base de datos)
    [Authorize(Roles = RolesSistema.AdminYSuper)]
    public class CatalogosController : ControladorAdminBase
    {
        // GET: Admin/Catalogos
        public ActionResult Index()
        {
            var catalogos = new Dictionary<string, List<KeyValuePair<string, string>>>
            {
                { "Estados de aeronave", db.EstadoAeronaves.ToList().Select(e => Par(e.Codigo, e.Descripcion)).ToList() },
                { "Estados de vuelo", db.EstadoVueloes.ToList().Select(e => Par(e.Codigo, e.Descripcion)).ToList() },
                { "Estados de itinerario", db.EstadoItinerarios.ToList().Select(e => Par(e.Codigo, e.Descripcion)).ToList() },
                { "Estados de reserva", db.EstadoReservas.ToList().Select(e => Par(e.Codigo, e.Descripcion)).ToList() },
                { "Estados de asignación de asiento", db.EstadoAsignacions.ToList().Select(e => Par(e.Codigo, e.Descripcion)).ToList() },
                { "Estados de compra", db.EstadoCompras.ToList().Select(e => Par(e.Codigo, e.Descripcion)).ToList() },
                { "Estados de pago", db.EstadoPagoes.ToList().Select(e => Par(e.Codigo, e.Descripcion)).ToList() },
                { "Estados de check-in", db.EstadoCheckIns.ToList().Select(e => Par(e.Codigo, e.Descripcion)).ToList() },
                { "Tipos de comprobante", db.TipoComprobantes.ToList().Select(e => Par(e.Codigo, e.Descripcion)).ToList() },
                { "Estados de comprobante", db.EstadoComprobantes.ToList().Select(e => Par(e.Codigo, e.Descripcion)).ToList() },
                { "Tipos de documento", db.TipoDocumentoIdentidads.ToList().Select(e => Par(e.Codigo, e.Descripcion)).ToList() },
                { "Monedas", db.Monedas.ToList().Select(e => Par(e.Codigo, e.Descripcion)).ToList() },
                { "Métodos de pago (simulados)", db.MetodoPagoes.ToList().Select(e => Par(e.Codigo, e.Nombre)).ToList() },
                { "Marcas de tarjeta (simuladas)", db.MarcaTarjetas.ToList().Select(e => Par(e.Codigo, e.Nombre)).ToList() }
            };
            return View(catalogos);
        }

        private static KeyValuePair<string, string> Par(string codigo, string descripcion)
        {
            return new KeyValuePair<string, string>(codigo, descripcion);
        }
    }
}
