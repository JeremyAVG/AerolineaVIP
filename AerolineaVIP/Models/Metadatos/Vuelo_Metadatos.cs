using System;
using System.ComponentModel.DataAnnotations;

namespace AerolineaVIP.Models
{
    [MetadataType(typeof(VueloMetadatos))]
    public partial class Vuelo
    {
    }

    public class VueloMetadatos
    {
        [Required(ErrorMessage = "Seleccione la frecuencia.")]
        [Display(Name = "Frecuencia")]
        public int FrecuenciaId { get; set; }

        [Required(ErrorMessage = "Ingrese la fecha de operación.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        [Display(Name = "Fecha de operación")]
        public DateTime FechaOperacion { get; set; }

        [Display(Name = "Aeronave asignada")]
        public int? AeronaveId { get; set; }

        [Required(ErrorMessage = "Seleccione el estado.")]
        [Display(Name = "Estado")]
        public string EstadoVueloCodigo { get; set; }
    }
}
