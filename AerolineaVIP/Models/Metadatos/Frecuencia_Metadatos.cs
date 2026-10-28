using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AerolineaVIP.Models
{
    [MetadataType(typeof(FrecuenciaMetadatos))]
    public partial class Frecuencia : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (HoraLlegada <= HoraSalida)
            {
                yield return new ValidationResult("La hora de llegada debe ser posterior a la hora de salida.", new[] { "HoraLlegada" });
            }
        }
    }

    public class FrecuenciaMetadatos
    {
        [Required(ErrorMessage = "Seleccione la ruta.")]
        [Display(Name = "Ruta")]
        public int RutaId { get; set; }

        [Required(ErrorMessage = "Seleccione el día de operación.")]
        [Range(1, 7, ErrorMessage = "Seleccione un día válido.")]
        [Display(Name = "Día de operación")]
        public byte DiaSemana { get; set; }

        [Required(ErrorMessage = "Ingrese la hora de salida.")]
        [DisplayFormat(DataFormatString = @"{0:hh\:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Hora de salida")]
        public TimeSpan HoraSalida { get; set; }

        [Required(ErrorMessage = "Ingrese la hora de llegada.")]
        [DisplayFormat(DataFormatString = @"{0:hh\:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Hora de llegada")]
        public TimeSpan HoraLlegada { get; set; }

        [Display(Name = "Activa")]
        public bool Activa { get; set; }
    }
}
