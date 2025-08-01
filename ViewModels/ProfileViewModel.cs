using System.ComponentModel.DataAnnotations;

namespace HotelCostaAzulFinal.ViewModels
{
    // ProfileViewModel.cs
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Ingrese un email válido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50, ErrorMessage = "El apellido no puede tener más de 50 caracteres")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El documento es obligatorio")]
        [StringLength(20, ErrorMessage = "El documento no puede tener más de 20 caracteres")]
        [Display(Name = "Número de Documento")]
        public string Documento { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seleccione el tipo de documento")]
        [Display(Name = "Tipo de Documento")]
        public string TipoDocumento { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Ingrese un número de teléfono válido")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Display(Name = "Fecha de Registro")]
        public DateTime FechaRegistro { get; set; }
    }
}