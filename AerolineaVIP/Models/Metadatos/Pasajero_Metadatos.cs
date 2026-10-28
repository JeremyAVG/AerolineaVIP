using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AerolineaVIP.Models
{
    [MetadataType(typeof(PasajeroMetadatos))]
    public partial class Pasajero : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (TipoDocumentoCodigo == "CEDULA" && NumeroDocumento != null
                && (NumeroDocumento.Length != 10 || !NumeroDocumento.All(char.IsDigit)))
            {
                yield return new ValidationResult("La cédula debe tener 10 dígitos.", new[] { "NumeroDocumento" });
            }
            if (FechaNacimiento.HasValue && FechaNacimiento.Value.Date > DateTime.Today)
            {
                yield return new ValidationResult("La fecha de nacimiento no puede ser futura.", new[] { "FechaNacimiento" });
            }
        }
    }

    public class PasajeroMetadatos
    {
        [Required(ErrorMessage = "Seleccione el tipo de documento.")]
        [Display(Name = "Tipo de documento")]
        public string TipoDocumentoCodigo { get; set; }

        [Required(ErrorMessage = "Ingrese el número de documento.")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "El documento debe tener entre {2} y {1} caracteres.")]
        [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "El documento solo puede contener letras y números.")]
        [Display(Name = "Número de documento")]
        public string NumeroDocumento { get; set; }

        [Required(ErrorMessage = "Ingrese los nombres.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Los nombres deben tener entre {2} y {1} caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ ]+$", ErrorMessage = "Los nombres solo pueden contener letras y espacios.")]
        [Display(Name = "Nombres")]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "Ingrese los apellidos.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Los apellidos deben tener entre {2} y {1} caracteres.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ ]+$", ErrorMessage = "Los apellidos solo pueden contener letras y espacios.")]
        [Display(Name = "Apellidos")]
        public string Apellidos { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        [Display(Name = "Fecha de nacimiento")]
        public DateTime? FechaNacimiento { get; set; }

        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(256)]
        [Display(Name = "Correo")]
        public string Correo { get; set; }

        [RegularExpression("^[0-9+ ]{7,20}$", ErrorMessage = "El teléfono solo puede tener números, espacios y +, de 7 a 20 caracteres.")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        [Display(Name = "Fecha de registro")]
        public DateTime FechaRegistro { get; set; }
    }
}
