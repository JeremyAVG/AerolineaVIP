using System.ComponentModel.DataAnnotations;

namespace AerolineaVIP.Models
{
    [MetadataType(typeof(ItinerarioMetadatos))]
    public partial class Itinerario
    {
    }

    public class ItinerarioMetadatos
    {
        [Required(ErrorMessage = "Ingrese la referencia.")]
        [StringLength(30, ErrorMessage = "La referencia no puede superar los {1} caracteres.")]
        [Display(Name = "Referencia")]
        public string Referencia { get; set; }

        [Required(ErrorMessage = "Ingrese el nombre del itinerario.")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los {1} caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Seleccione el estado.")]
        [Display(Name = "Estado")]
        public string EstadoItinerarioCodigo { get; set; }
    }
}
