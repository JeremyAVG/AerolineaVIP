using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace AerolineaVIP.Models
{
    public class BusquedaVuelosViewModel
    {
        public BusquedaVuelosViewModel()
        {
            Origenes = new List<SelectListItem>();
            Destinos = new List<SelectListItem>();
            Sugerencias = new List<OpcionVuelo>();
        }
        public List<OpcionVuelo> Sugerencias { get; set; }
        [Required(ErrorMessage = "Seleccione el origen.")]
        [Display(Name = "Origen")]
        public int? OrigenId { get; set; }

        [Required(ErrorMessage = "Seleccione el destino.")]
        [Display(Name = "Destino")]
        public int? DestinoId { get; set; }

        [Required(ErrorMessage = "Indique la fecha de salida.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        [FechaNoAnteriorAHoy(ErrorMessage = "La fecha de salida no puede ser anterior a hoy.")]
        [Display(Name = "Fecha de salida")]
        public DateTime? Fecha { get; set; }

        [Required(ErrorMessage = "Indique la cantidad de pasajeros.")]
        [Range(1, 7, ErrorMessage = "La cantidad de pasajeros debe estar entre 1 y 7.")]
        [Display(Name = "Pasajeros")]
        public int? Pasajeros { get; set; }

        public IEnumerable<SelectListItem> Origenes { get; set; }
        public IEnumerable<SelectListItem> Destinos { get; set; }
    }

    public class HomeIndexViewModel
    {
        public BusquedaVuelosViewModel Busqueda { get; set; }
        public List<DestinoDisponible> Destinos { get; set; }
        public string CiudadOrigen { get; set; }
    }
}
