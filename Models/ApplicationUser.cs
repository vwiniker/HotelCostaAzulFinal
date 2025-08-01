using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HotelCostaAzulFinal.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Documento { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string TipoDocumento { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public bool EsAdmin { get; set; } = false;

        // Propiedades calculadas
        public string NombreCompleto => $"{Nombre} {Apellido}";

        // Relación con reservas
        public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }
}