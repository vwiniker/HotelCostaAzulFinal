using System.ComponentModel.DataAnnotations;

namespace HotelCostaAzulFinal.Models
{
    public class ConsultaAyuda
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Ingrese un email válido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Ingrese un número de teléfono válido")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "El asunto es obligatorio")]
        [Display(Name = "Asunto")]
        public string Asunto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El mensaje es obligatorio")]
        [StringLength(1000, ErrorMessage = "El mensaje no puede tener más de 1000 caracteres")]
        [Display(Name = "Mensaje")]
        public string Mensaje { get; set; } = string.Empty;

        public string? RutaImagen { get; set; }
        public DateTime FechaEnvio { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public string? Respuesta { get; set; }
        public DateTime? FechaRespuesta { get; set; }
    }
}