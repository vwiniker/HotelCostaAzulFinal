using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HotelCostaAzulFinal.Data;
using HotelCostaAzulFinal.Models;

namespace HotelCostaAzulFinal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportesController : Controller
    {
        private readonly HotelContext _context;
        private readonly ILogger<ReportesController> _logger;

        public ReportesController(HotelContext context, ILogger<ReportesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Reportes
        public IActionResult Index()
        {
            return View();
        }

        // GET: Reportes/ReservasPorPeriodo
        public async Task<IActionResult> ReservasPorPeriodo(string? fechaInicio, string? fechaFin)
        {
            try
            {
                // Valores por defecto si no se especifican fechas
                DateTime fechaInicioDate = DateTime.Today.AddMonths(-1);
                DateTime fechaFinDate = DateTime.Today;

                // Convertir strings a DateTime si son válidas
                if (!string.IsNullOrEmpty(fechaInicio) && DateTime.TryParse(fechaInicio, out var tempInicio))
                {
                    fechaInicioDate = tempInicio;
                }

                if (!string.IsNullOrEmpty(fechaFin) && DateTime.TryParse(fechaFin, out var tempFin))
                {
                    fechaFinDate = tempFin;
                }

                if (fechaInicioDate > fechaFinDate)
                {
                    TempData["ErrorMessage"] = "La fecha de inicio debe ser anterior a la fecha de fin";
                    return View("Index");
                }

                var reservas = await _context.Reservas
                    .Include(r => r.Usuario)
                    .Include(r => r.Habitacion)
                    .Where(r => r.FechaReserva.Date >= fechaInicioDate.Date &&
                               r.FechaReserva.Date <= fechaFinDate.Date)
                    .OrderByDescending(r => r.FechaReserva)
                    .ToListAsync();

                var reporte = new ReporteReservasPeriodo
                {
                    FechaInicio = fechaInicioDate,
                    FechaFin = fechaFinDate,
                    Reservas = reservas,
                    TotalReservas = reservas.Count,
                    TotalIngresos = reservas.Where(r => r.Estado == "Confirmada" || r.Estado == "Completada").Sum(r => r.Total),
                    ReservasPorEstado = reservas.GroupBy(r => r.Estado)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    PromedioIngresoPorReserva = reservas.Any() ?
                        reservas.Where(r => r.Estado == "Confirmada" || r.Estado == "Completada").Average(r => (double)r.Total) : 0
                };

                return View(reporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar reporte de reservas por período");
                TempData["ErrorMessage"] = "Error al generar el reporte";
                return View("Index");
            }
        }

        // GET: Reportes/OcupacionHabitaciones
        public async Task<IActionResult> OcupacionHabitaciones(int? mes, int? año)
        {
            try
            {
                mes ??= DateTime.Today.Month;
                año ??= DateTime.Today.Year;

                var fechaInicio = new DateTime(año.Value, mes.Value, 1);
                var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

                var habitaciones = await _context.Habitaciones.ToListAsync();
                var reservas = await _context.Reservas
                    .Include(r => r.Habitacion)
                    .Where(r => r.Estado != "Cancelada" &&
                               r.FechaInicio <= fechaFin &&
                               r.FechaFin >= fechaInicio)
                    .ToListAsync();

                var diasDelMes = fechaFin.Day;
                var reporteOcupacion = habitaciones.Select(h => new OcupacionHabitacion
                {
                    Habitacion = h,
                    ReservasEnPeriodo = reservas.Where(r => r.HabitacionId == h.Id).ToList(),
                    DiasOcupados = 0, // Se calculará abajo
                    TotalDiasDisponibles = diasDelMes,
                    PorcentajeOcupacion = 0
                }).ToList();

                // Calcular días ocupados y porcentaje de ocupación
                foreach (var item in reporteOcupacion)
                {
                    item.DiasOcupados = item.ReservasEnPeriodo
                        .Sum(r => Math.Min(r.FechaFin, fechaFin).Subtract(Math.Max(r.FechaInicio, fechaInicio)).Days);

                    item.PorcentajeOcupacion = item.TotalDiasDisponibles > 0
                        ? (double)item.DiasOcupados / item.TotalDiasDisponibles * 100
                        : 0;
                }

                var reporte = new ReporteOcupacionHabitaciones
                {
                    Mes = mes.Value,
                    Año = año.Value,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    OcupacionPorHabitacion = reporteOcupacion,
                    PromedioOcupacion = reporteOcupacion.Any() ? reporteOcupacion.Average(r => r.PorcentajeOcupacion) : 0,
                    TotalReservas = reservas.Count,
                    IngresosTotales = reservas.Where(r => r.Estado == "Confirmada" || r.Estado == "Completada").Sum(r => r.Total)
                };

                return View(reporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar reporte de ocupación");
                TempData["ErrorMessage"] = "Error al generar el reporte de ocupación";
                return View("Index");
            }
        }

        // GET: Reportes/ClientesFrecuentes
        public async Task<IActionResult> ClientesFrecuentes()
        {
            try
            {
                var clientesFrecuentes = await _context.Users
                    .Where(u => !u.EsAdmin)
                    .Include(u => u.Reservas)
                    .ThenInclude(r => r.Habitacion)
                    .Select(u => new ClienteFrecuente
                    {
                        Usuario = u,
                        TotalReservas = u.Reservas.Count,
                        TotalGastado = u.Reservas.Where(r => r.Estado == "Confirmada" || r.Estado == "Completada").Sum(r => r.Total),
                        UltimaReserva = u.Reservas.OrderByDescending(r => r.FechaReserva).FirstOrDefault(),
                        TipoHabitacionPreferida = u.Reservas.GroupBy(r => r.Habitacion.Tipo)
                            .OrderByDescending(g => g.Count())
                            .Select(g => g.Key)
                            .FirstOrDefault() ?? "N/A"
                    })
                    .Where(c => c.TotalReservas > 0)
                    .OrderByDescending(c => c.TotalReservas)
                    .ThenByDescending(c => c.TotalGastado)
                    .Take(20)
                    .ToListAsync();

                var reporte = new ReporteClientesFrecuentes
                {
                    ClientesFrecuentes = clientesFrecuentes,
                    TotalClientes = await _context.Users.CountAsync(u => !u.EsAdmin),
                    ClientesConReservas = clientesFrecuentes.Count,
                    PromedioReservasPorCliente = clientesFrecuentes.Any() ? clientesFrecuentes.Average(c => c.TotalReservas) : 0,
                    PromedioGastoPorCliente = clientesFrecuentes.Any() ? (double)clientesFrecuentes.Average(c => c.TotalGastado) : 0
                };

                return View(reporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar reporte de clientes frecuentes");
                TempData["ErrorMessage"] = "Error al generar el reporte de clientes frecuentes";
                return View("Index");
            }
        }

        // GET: Reportes/ExportarPDF
        public async Task<IActionResult> ExportarReservaPDF(int id)
        {
            try
            {
                var reserva = await _context.Reservas
                    .Include(r => r.Usuario)
                    .Include(r => r.Habitacion)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (reserva == null)
                {
                    return NotFound();
                }

                // Aquí puedes implementar la generación de PDF usando una librería como iTextSharp
                // Por ahora, retornamos un JSON con los datos
                var datosReserva = new
                {
                    reserva.Id,
                    Cliente = reserva.Usuario.NombreCompleto,
                    reserva.Usuario.Email,
                    reserva.Usuario.PhoneNumber,
                    Habitacion = $"{reserva.Habitacion.Numero} - {reserva.Habitacion.Tipo}",
                    reserva.FechaInicio,
                    reserva.FechaFin,
                    reserva.NumeroHuespedes,
                    reserva.Total,
                    reserva.Estado,
                    reserva.NotasEspeciales,
                    FechaGeneracion = DateTime.Now
                };

                return Json(datosReserva);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar reserva {ReservaId} a PDF", id);
                return BadRequest("Error al generar el PDF");
            }
        }
    }

    // Clases para los reportes
    public class ReporteReservasPeriodo
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public List<Reserva> Reservas { get; set; } = new();
        public int TotalReservas { get; set; }
        public decimal TotalIngresos { get; set; }
        public Dictionary<string, int> ReservasPorEstado { get; set; } = new();
        public double PromedioIngresoPorReserva { get; set; }
    }

    public class ReporteOcupacionHabitaciones
    {
        public int Mes { get; set; }
        public int Año { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public List<OcupacionHabitacion> OcupacionPorHabitacion { get; set; } = new();
        public double PromedioOcupacion { get; set; }
        public int TotalReservas { get; set; }
        public decimal IngresosTotales { get; set; }
    }

    public class OcupacionHabitacion
    {
        public Habitacion Habitacion { get; set; } = null!;
        public List<Reserva> ReservasEnPeriodo { get; set; } = new();
        public int DiasOcupados { get; set; }
        public int TotalDiasDisponibles { get; set; }
        public double PorcentajeOcupacion { get; set; }
    }

    public class ReporteClientesFrecuentes
    {
        public List<ClienteFrecuente> ClientesFrecuentes { get; set; } = new();
        public int TotalClientes { get; set; }
        public int ClientesConReservas { get; set; }
        public double PromedioReservasPorCliente { get; set; }
        public double PromedioGastoPorCliente { get; set; }
    }

    public class ClienteFrecuente
    {
        public ApplicationUser Usuario { get; set; } = null!;
        public int TotalReservas { get; set; }
        public decimal TotalGastado { get; set; }
        public Reserva? UltimaReserva { get; set; }
        public string TipoHabitacionPreferida { get; set; } = string.Empty;
    }
}