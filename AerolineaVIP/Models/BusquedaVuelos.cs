using System.Linq;
using System.Web.Mvc;

namespace AerolineaVIP.Models
{
    // Listas y validaciones de servidor del formulario "Encuentra tu vuelo"
    public static class BusquedaVuelos
    {
        public static void CargarListas(AerolineaVIPEntities db, BusquedaVuelosViewModel model)
        {
            var origenes = db.Rutas.Where(r => r.Activa && r.Aeropuerto1.Activo)
                .Select(r => r.Aeropuerto1).Distinct()
                .OrderBy(a => a.Localidad).ToList();
            model.Origenes = new SelectList(origenes, "AeropuertoId", "Descripcion", model.OrigenId);

            var destinos = model.OrigenId.HasValue
                ? Ruta.ObtenerDestinos(db, model.OrigenId.Value)
                : new System.Collections.Generic.List<DestinoDisponible>();
            model.Destinos = new SelectList(destinos, "AeropuertoId", "Texto", model.DestinoId);
        }

        // Devuelve el mensaje de error o null si la combinación origen/destino es válida
        public static string ValidarRuta(AerolineaVIPEntities db, BusquedaVuelosViewModel model)
        {
            if (model.OrigenId == model.DestinoId)
            {
                return "El origen y el destino deben ser distintos.";
            }
            var destinos = Ruta.ObtenerDestinos(db, model.OrigenId.Value);
            if (!destinos.Any(d => d.AeropuertoId == model.DestinoId))
            {
                return "No existe una ruta entre el origen y el destino seleccionados.";
            }
            return null;
        }
    }
}
