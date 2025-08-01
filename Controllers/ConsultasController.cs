using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HotelCostaAzulFinal.Data;
using HotelCostaAzulFinal.Models;

namespace HotelCostaAzulFinal.Controllers
{
    [Authorize]
    public class ConsultasController : Controller
    {
        private readonly HotelContext _context;
        private readonly ILogger<ConsultasController> _logger;

        public ConsultasController(HotelContext context, ILogger<ConsultasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Consultas
        public IActionResult Index()
        {
            return View();
        }

        // GET: Consultas/DisponibilidadHabitaciones
        public async Task<IActionResult> DisponibilidadHabitaciones(
            string? fechaInicio,
            string? fechaFin,
            string? tipoHabitacion,
            int? capacidadMinima,
            decimal? precioMaximo)
        {
            try
            {
                var consulta = new ConsultaDisponibilidad
                {
                    TipoHabitacion = tipoHabitacion,
                    CapacidadMinima = capacidadMinima,
                    PrecioMaximo = precioMaximo
                };

                // Convertir strings a DateTime si son válidas
                DateTime? fechaInicioDate = null;
                DateTime? fechaFinDate = null;

                if (!string.IsNullOrEmpty(fechaInicio) && DateTime.TryParse(fechaInicio, out var tempInicio))
                {
                    fechaInicioDate = tempInicio;
                    consulta.FechaInicio = fechaInicioDate;
                }

                if (!string.IsNullOrEmpty(fechaFin) && DateTime.TryParse(fechaFin, out var tempFin))
                {
                    fechaFinDate = tempFin;
                    consulta.FechaFin = fechaFinDate;
                }

                if (fechaInicioDate.HasValue && fechaFinDate.HasValue)
                {
                    if (fechaInicioDate >= fechaFinDate)
                    {
                        ModelState.AddModelError("", "La fecha de fin debe ser posterior a la fecha de inicio");
                        ViewBag.TiposHabitacion = await GetTiposHabitacion();
                        return View(consulta);
                    }

                    if (fechaInicioDate < DateTime.Today)
                    {
                        ModelState.AddModelError("", "La fecha de inicio no puede ser anterior a hoy");
                        ViewBag.TiposHabitacion = await GetTiposHabitacion();
                        return View(consulta);
                    }

                    // Obtener habitaciones ocupadas en el rango de fechas
                    var habitacionesOcupadas = await _context.Reservas
                        .Where(r => r.Estado != "Cancelada" &&
                                   r.FechaInicio < fechaFinDate &&
                                   r.FechaFin > fechaInicioDate)
                        .Select(r => r.HabitacionId)
                        .ToListAsync();

                    // Query base de habitaciones disponibles
                    var query = _context.Habitaciones
                        .Where(h => h.Disponible && !habitacionesOcupadas.Contains(h.Id));

                    // Aplicar filtros adicionales
                    if (!string.IsNullOrEmpty(tipoHabitacion))
                    {
                        query = query.Where(h => h.Tipo == tipoHabitacion);
                    }

                    if (capacidadMinima.HasValue)
                    {
                        query = query.Where(h => h.Capacidad >= capacidadMinima.Value);
                    }

                    if (precioMaximo.HasValue)
                    {
                        query = query.Where(h => h.PrecioPorNoche <= precioMaximo.Value);
                    }

                    consulta.HabitacionesDisponibles = await query
                        .OrderBy(h => h.PrecioPorNoche)
                        .ToListAsync();

                    // Calcular estadísticas
                    if (consulta.HabitacionesDisponibles.Any())
                    {
                        var noches = (fechaFinDate.Value - fechaInicioDate.Value).Days;
                        consulta.TotalNoches = noches;
                        consulta.PrecioMinimo = consulta.HabitacionesDisponibles.Min(h => h.PrecioPorNoche * noches);
                        consulta.PrecioMaximoTotal = consulta.HabitacionesDisponibles.Max(h => h.PrecioPorNoche * noches);
                        consulta.PrecioPromedio = consulta.HabitacionesDisponibles.Average(h => h.PrecioPorNoche * noches);
                    }
                }
                else
                {
                    // Sin fechas, mostrar todas las habitaciones disponibles
                    consulta.HabitacionesDisponibles = await _context.Habitaciones
                        .Where(h => h.Disponible)
                        .OrderBy(h => h.PrecioPorNoche)
                        .ToListAsync();
                }

                // Obtener tipos de habitación para el filtro
                ViewBag.TiposHabitacion = await GetTiposHabitacion();

                return View(consulta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en consulta de disponibilidad");
                TempData["ErrorMessage"] = "Error al realizar la consulta";
                ViewBag.TiposHabitacion = await GetTiposHabitacion();
                return View(new ConsultaDisponibilidad());
            }
        }

        // GET: Consultas/HistorialReservas
        public async Task<IActionResult> HistorialReservas(
            string? clienteEmail,
            string? estado,
            string? fechaDesde,
            string? fechaHasta,
            int? habitacionId)
        {
            try
            {
                var consulta = new ConsultaHistorialReservas
                {
                    ClienteEmail = clienteEmail,
                    Estado = estado,
                    HabitacionId = habitacionId
                };

                // Convertir strings a DateTime
                DateTime? fechaDesdeDate = null;
                DateTime? fechaHastaDate = null;

                if (!string.IsNullOrEmpty(fechaDesde) && DateTime.TryParse(fechaDesde, out var tempDesde))
                {
                    fechaDesdeDate = tempDesde;
                    consulta.FechaDesde = fechaDesdeDate;
                }

                if (!string.IsNullOrEmpty(fechaHasta) && DateTime.TryParse(fechaHasta, out var tempHasta))
                {
                    fechaHastaDate = tempHasta;
                    consulta.FechaHasta = fechaHastaDate;
                }

                var query = _context.Reservas
                    .Include(r => r.Usuario)
                    .Include(r => r.Habitacion)
                    .AsQueryable();

                // Aplicar filtros
                if (!string.IsNullOrEmpty(clienteEmail))
                {
                    query = query.Where(r => r.Usuario.Email.Contains(clienteEmail));
                }

                if (!string.IsNullOrEmpty(estado))
                {
                    query = query.Where(r => r.Estado == estado);
                }

                if (fechaDesdeDate.HasValue)
                {
                    query = query.Where(r => r.FechaReserva.Date >= fechaDesdeDate.Value.Date);
                }

                if (fechaHastaDate.HasValue)
                {
                    query = query.Where(r => r.FechaReserva.Date <= fechaHastaDate.Value.Date);
                }

                if (habitacionId.HasValue)
                {
                    query = query.Where(r => r.HabitacionId == habitacionId.Value);
                }

                // Solo admins pueden ver todas las reservas, los usuarios solo las suyas
                if (!User.IsInRole("Admin"))
                {
                    var userEmail = User.Identity?.Name;
                    query = query.Where(r => r.Usuario.Email == userEmail);
                }

                consulta.Reservas = await query
                    .OrderByDescending(r => r.FechaReserva)
                    .Take(100) // Limitar resultados
                    .ToListAsync();

                // Estadísticas
                if (consulta.Reservas.Any())
                {
                    consulta.TotalReservas = consulta.Reservas.Count;
                    consulta.TotalIngresos = consulta.Reservas
                        .Where(r => r.Estado == "Confirmada" || r.Estado == "Completada")
                        .Sum(r => r.Total);
                    var reservasCompletas = consulta.Reservas.Count(r => r.Estado == "Confirmada" || r.Estado == "Completada");
                    consulta.PromedioIngresoPorReserva = reservasCompletas > 0 ? consulta.TotalIngresos / reservasCompletas : 0;
                }

                // Datos para los filtros
                ViewBag.Estados = new[] { "Pendiente", "Confirmada", "Cancelada", "Completada" };
                ViewBag.Habitaciones = await _context.Habitaciones
                    .Select(h => new { h.Id, Nombre = $"{h.Numero} - {h.Tipo}" })
                    .ToListAsync();

                return View(consulta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en consulta de historial de reservas");
                TempData["ErrorMessage"] = "Error al realizar la consulta";
                return View(new ConsultaHistorialReservas());
            }
        }

        private async Task<List<string>> GetTiposHabitacion()
        {
            return await _context.Habitaciones
                .Select(h => h.Tipo)
                .Distinct()
                .ToListAsync();
        }
    }

    // Clases para las consultas
    public class ConsultaDisponibilidad
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string? TipoHabitacion { get; set; }
        public int? CapacidadMinima { get; set; }
        public decimal? PrecioMaximo { get; set; }
        public List<Habitacion> HabitacionesDisponibles { get; set; } = new();
        public int TotalNoches { get; set; }
        public decimal PrecioMinimo { get; set; }
        public decimal PrecioMaximoTotal { get; set; }
        public decimal PrecioPromedio { get; set; }
    }

    public class ConsultaHistorialReservas
    {
        public string? ClienteEmail { get; set; }
        public string? Estado { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int? HabitacionId { get; set; }
        public List<Reserva> Reservas { get; set; } = new();
        public int TotalReservas { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal PromedioIngresoPorReserva { get; set; }
    }
}