using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Web.Mvc;
using AerolineaVIP.Models;

namespace AerolineaVIP.Areas.Admin.Controllers
{
    // Base común del área administrativa: contexto de datos y guardado con mensajes amigables
    public abstract class ControladorAdminBase : Controller
    {
        protected AerolineaVIPEntities db = new AerolineaVIPEntities();

        // Guarda los cambios; si SQL Server los rechaza agrega el mensaje al ModelState y devuelve false
        protected bool GuardarCambios()
        {
            try
            {
                db.SaveChanges();
                return true;
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", ErroresBaseDatos.ObtenerMensaje(ex));
            }
            catch (DbEntityValidationException)
            {
                ModelState.AddModelError("", "Los datos no son válidos. Revise los campos del formulario.");
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError("", ErroresBaseDatos.ObtenerMensaje(ex));
            }
            return false;
        }

        // Para comandos SQL directos (procedimientos almacenados)
        protected string EjecutarComando(Func<int> comando)
        {
            try
            {
                comando();
                return null;
            }
            catch (Exception ex)
            {
                return ErroresBaseDatos.ObtenerMensaje(ex);
            }
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
