using System.ComponentModel.DataAnnotations;

namespace AerolineaVIP.Models
{
    [MetadataType(typeof(TramoItinerarioMetadatos))]
    public partial class TramoItinerario
    {
    }

    public class TramoItinerarioMetadatos
    {
        [Required(ErrorMessage = "Seleccione el itinerario.")]
        [Display(Name = "Itinerario")]
        public int ItinerarioId { get; set; }

        [Required(ErrorMessage = "Seleccione el vuelo.")]
        [Display(Name = "Vuelo")]
        public int VueloId { get; set; }

        [Required(ErrorMessage = "Ingrese el orden del tramo.")]
        [Range(1, 10, ErrorMessage = "El orden debe estar entre {1} y {2}.")]
        [Display(Name = "Orden")]
        public byte Orden { get; set; }
    }
}
