using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelCostaAzulFinal.Data;
using HotelCostaAzulFinal.Models;

namespace HotelCostaAzulFinal.Controllers
{
    public class HabitacionesWebController : Controller
    {
        private readonly HotelContext _context;
        private readonly ILogger<HabitacionesWebController> _logger;

        public HabitacionesWebController(HotelContext context, ILogger<HabitacionesWebController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: HabitacionesWeb
        public async Task<IActionResult> Index(string? tipo, decimal? precioMin, decimal? precioMax, int? capacidad)
        {
            try
            {
                var habitaciones = _context.Habitaciones.Where(h => h.Disponible);

                // Filtros
                if (!string.IsNullOrEmpty(tipo))
                {
                    habitaciones = habitaciones.Where(h => h.Tipo.Contains(tipo));
                }

                if (precioMin.HasValue)
                {
                    habitaciones = habitaciones.Where(h => h.PrecioPorNoche >= precioMin.Value);
                }

                if (precioMax.HasValue)
                {
                    habitaciones = habitaciones.Where(h => h.PrecioPorNoche <= precioMax.Value);
                }

                if (capacidad.HasValue)
                {
                    habitaciones = habitaciones.Where(h => h.Capacidad >= capacidad.Value);
                }

                var resultado = await habitaciones
                    .OrderBy(h => h.PrecioPorNoche)
                    .ToListAsync();

                // Pasar los filtros a la vista
                ViewData["TipoFiltro"] = tipo;
                ViewData["PrecioMinFiltro"] = precioMin;
                ViewData["PrecioMaxFiltro"] = precioMax;
                ViewData["CapacidadFiltro"] = capacidad;

                return View(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar habitaciones");
                TempData["ErrorMessage"] = "Error al cargar las habitaciones";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: HabitacionesWeb/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var habitacion = await _context.Habitaciones
                    .FirstOrDefaultAsync(h => h.Id == id);

                if (habitacion == null)
                {
                    return NotFound();
                }

                return View(habitacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar detalles de habitación {HabitacionId}", id);
                TempData["ErrorMessage"] = "Error al cargar los detalles de la habitación";
                return RedirectToAction("Index");
            }
        }

        // GET: HabitacionesWeb/Reservar/5
        public async Task<IActionResult> Reservar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var habitacion = await _context.Habitaciones
                    .FirstOrDefaultAsync(h => h.Id == id && h.Disponible);

                if (habitacion == null)
                {
                    TempData["ErrorMessage"] = "La habitación no está disponible";
                    return RedirectToAction("Index");
                }

                ViewData["Habitacion"] = habitacion;
                return RedirectToAction("Create", "Reservas", new { habitacionId = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al preparar reserva para habitación {HabitacionId}", id);
                TempData["ErrorMessage"] = "Error al procesar la reserva";
                return RedirectToAction("Index");
            }
        }

        // GET: HabitacionesWeb/Buscar
        public async Task<IActionResult> Buscar(DateTime? fechaInicio, DateTime? fechaFin, int? huespedes)
        {
            try
            {
                var habitaciones = _context.Habitaciones.Where(h => h.Disponible);

                if (huespedes.HasValue)
                {
                    habitaciones = habitaciones.Where(h => h.Capacidad >= huespedes.Value);
                }

                // Aquí podrías agregar lógica para verificar disponibilidad en las fechas
                // if (fechaInicio.HasValue && fechaFin.HasValue)
                // {
                //     var reservasConflicto = _context.Reservas
                //         .Where(r => r.Estado != "Cancelada" && 
                //                     r.FechaInicio < fechaFin && 
                //                     r.FechaFin > fechaInicio)
                //         .Select(r => r.HabitacionId);
                //     
                //     habitaciones = habitaciones.Where(h => !reservasConflicto.Contains(h.Id));
                // }

                var resultado = await habitaciones
                    .OrderBy(h => h.PrecioPorNoche)
                    .ToListAsync();

                ViewData["FechaInicio"] = fechaInicio;
                ViewData["FechaFin"] = fechaFin;
                ViewData["Huespedes"] = huespedes;

                return View("Index", resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda de habitaciones");
                TempData["ErrorMessage"] = "Error en la búsqueda";
                return RedirectToAction("Index");
            }
        }
    }
}