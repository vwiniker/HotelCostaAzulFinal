using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelCostaAzulFinal.Models
{
    public class Reserva
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty; // Relación con ApplicationUser

        [Required]
        public int HabitacionId { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public int NumeroHuespedes { get; set; } = 1;

        [StringLength(20)]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Confirmada, Cancelada, Completada

        [StringLength(500)]
        public string? NotasEspeciales { get; set; }

        public DateTime FechaReserva { get; set; } = DateTime.Now;

        // Relaciones
        [ForeignKey("UsuarioId")]
        public virtual ApplicationUser Usuario { get; set; } = null!;

        [ForeignKey("HabitacionId")]
        public virtual Habitacion Habitacion { get; set; } = null!;

        // Propiedades calculadas
        public int NochesEstadia => (FechaFin - FechaInicio).Days;

        public string EstadoFormateado => Estado switch
        {
            "Pendiente" => "⏳ Pendiente",
            "Confirmada" => "✅ Confirmada",
            "Cancelada" => "❌ Cancelada",
            "Completada" => "🏁 Completada",
            _ => Estado
        };
    }
}