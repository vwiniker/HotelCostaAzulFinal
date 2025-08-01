using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelCostaAzulFinal.Models
{
    public class Habitacion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Numero { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Tipo { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioPorNoche { get; set; }

        public int Capacidad { get; set; }

        [StringLength(500)]
        public string? Caracteristicas { get; set; }

        public bool Disponible { get; set; } = true;

        [StringLength(200)]
        public string? ImagenUrl { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Relación con reservas
        public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }
}