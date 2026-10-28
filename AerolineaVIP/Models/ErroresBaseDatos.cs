using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace AerolineaVIP.Models
{
    // Convierte los errores de SQL Server (triggers, índices únicos, FK y CHECK) en mensajes para el usuario
    public static class ErroresBaseDatos
    {
        private static readonly Dictionary<string, string> MensajesPorRestriccion = new Dictionary<string, string>
        {
            { "UQ_Aeropuerto_CodigoIATA", "Ya existe un aeropuerto con ese código IATA." },
            { "UQ_Ruta_OrigenDestino", "Ya existe una ruta con ese origen y destino." },
            { "UQ_Frecuencia", "Ya existe una frecuencia para esa ruta, día y hora de salida." },
            { "UQ_Aeronave_Matricula", "Ya existe una aeronave con esa matrícula." },
            { "UQ_Vuelo_FrecuenciaFecha", "Ya existe un vuelo para esa frecuencia en esa fecha." },
            { "UQ_Itinerario_Referencia", "Ya existe un itinerario con esa referencia." },
            { "UQ_Tramo_Orden", "El itinerario ya tiene un tramo con ese orden." },
            { "UQ_Tramo_Vuelo", "Ese vuelo ya forma parte del itinerario." },
            { "UQ_Pasajero_Documento", "Ya existe un pasajero con ese tipo y número de documento." },
            { "CK_Ruta_OrigenDistintoDestino", "El origen y el destino deben ser distintos." },
            { "CK_Frecuencia_Horario", "La hora de llegada debe ser posterior a la hora de salida." },
            { "CK_Aeropuerto_CodigoIATA", "El código IATA debe tener tres letras mayúsculas." },
            { "CK_Pasajero_NumeroDocumento", "El número de documento debe tener al menos 5 caracteres." }
        };

        public static string ObtenerMensaje(Exception ex)
        {
            SqlException sql = BuscarSqlException(ex);
            if (sql == null)
            {
                return "No se pudo guardar la información. Revise los datos e intente nuevamente.";
            }

            // Mensajes lanzados por los triggers y procedimientos con THROW 5xxxx
            if (sql.Number >= 50000)
            {
                return sql.Message;
            }

            foreach (var item in MensajesPorRestriccion)
            {
                if (sql.Message.Contains(item.Key))
                {
                    return item.Value;
                }
            }

            switch (sql.Number)
            {
                case 2601:
                case 2627:
                    return "Ya existe un registro con esos datos.";
                case 547:
                    if (sql.Message.Contains("DELETE"))
                    {
                        return "No se puede eliminar el registro porque tiene información relacionada.";
                    }
                    if (sql.Message.Contains("CHECK"))
                    {
                        return "Los datos no cumplen las reglas de la base de datos.";
                    }
                    return "El registro hace referencia a datos que no existen.";
                default:
                    return "Error de base de datos: " + sql.Message;
            }
        }

        private static SqlException BuscarSqlException(Exception ex)
        {
            while (ex != null)
            {
                if (ex is SqlException)
                {
                    return (SqlException)ex;
                }
                ex = ex.InnerException;
            }
            return null;
        }
    }
}
