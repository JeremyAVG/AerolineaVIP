using System.ComponentModel.DataAnnotations;

namespace AerolineaVIP.Models
{
    [MetadataType(typeof(AeronaveMetadatos))]
    public partial class Aeronave
    {
    }

    public class AeronaveMetadatos
    {
        [Required(ErrorMessage = "Ingrese la matrícula.")]
        [StringLength(15, ErrorMessage = "La matrícula no puede superar los {1} caracteres.")]
        [RegularExpression("^[A-Z0-9-]+$", ErrorMessage = "Use solo letras mayúsculas, números y guion.")]
        [Display(Name = "Matrícula")]
        public string Matricula { get; set; }

        [Required(ErrorMessage = "Seleccione el modelo.")]
        [Display(Name = "Modelo")]
        public int ModeloAeronaveId { get; set; }

        [Required(ErrorMessage = "Seleccione el estado.")]
        [Display(Name = "Estado")]
        public string EstadoAeronaveCodigo { get; set; }
    }
}
