using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AerolineaVIP.Models
{
    [MetadataType(typeof(RutaMetadatos))]
    public partial class Ruta : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (AeropuertoOrigenId == AeropuertoDestinoId)
            {
                yield return new ValidationResult("El origen y el destino deben ser distintos.", new[] { "AeropuertoDestinoId" });
            }
        }
    }

    public class RutaMetadatos
    {
        [Required(ErrorMessage = "Seleccione el aeropuerto de origen.")]
        [Display(Name = "Origen")]
        public int AeropuertoOrigenId { get; set; }

        [Required(ErrorMessage = "Seleccione el aeropuerto de destino.")]
        [Display(Name = "Destino")]
        public int AeropuertoDestinoId { get; set; }

        [Required(ErrorMessage = "Ingrese la tarifa referencial.")]
        [Range(0, 99999, ErrorMessage = "La tarifa debe estar entre {1} y {2}.")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        [Display(Name = "Tarifa referencial (USD)")]
        public decimal TarifaReferencial { get; set; }

        [Display(Name = "Activa")]
        public bool Activa { get; set; }
    }
}
