using HotelCostaAzulFinal.Models;

namespace HotelCostaAzulFinal.Models
{
    public class DashboardViewModel
    {
        public int TotalHabitaciones { get; set; }
        public int HabitacionesDisponibles { get; set; }
        public int TotalClientes { get; set; }
        public int TotalReservas { get; set; }
        public int ReservasHoy { get; set; }
        public int ReservasConfirmadas { get; set; }
        public int ReservasPendientes { get; set; }
        public decimal IngresosTotales { get; set; }
        public List<Reserva> UltimasReservas { get; set; } = new();
        public List<HabitacionTipo> HabitacionesPorTipo { get; set; } = new();
        public List<ReservaMes> ReservasPorMes { get; set; } = new();
    }

    public class HabitacionTipo
    {
        public string Tipo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    public class ReservaMes
    {
        public int Mes { get; set; }
        public int Cantidad { get; set; }
    }
}