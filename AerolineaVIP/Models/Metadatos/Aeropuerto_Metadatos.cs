using System.ComponentModel.DataAnnotations;

namespace AerolineaVIP.Models
{
    [MetadataType(typeof(AeropuertoMetadatos))]
    public partial class Aeropuerto
    {
    }

    public class AeropuertoMetadatos
    {
        [Required(ErrorMessage = "Ingrese el código IATA.")]
        [RegularExpression("^[A-Z]{3}$", ErrorMessage = "El código IATA debe tener tres letras mayúsculas.")]
        [Display(Name = "Código IATA")]
        public string CodigoIATA { get; set; }

        [Required(ErrorMessage = "Ingrese el nombre del aeropuerto.")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los {1} caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Ingrese la localidad.")]
        [StringLength(100, ErrorMessage = "La localidad no puede superar los {1} caracteres.")]
        [Display(Name = "Localidad")]
        public string Localidad { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; }
    }
}
