using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using HotelCostaAzulFinal.Data;
using HotelCostaAzulFinal.Models;

namespace HotelCostaAzulFinal.Controllers
{
    [Authorize]
    public class ReservasController : Controller
    {
        private readonly HotelContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ReservasController> _logger;

        public ReservasController(
            HotelContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<ReservasController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Reservas
        public async Task<IActionResult> Index()
        {
            try
            {
                var reservas = await _context.Reservas
                    .OrderByDescending(r => r.FechaReserva)
                    .ToListAsync();

                return View(reservas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar reservas");
                TempData["ErrorMessage"] = "Error al cargar las reservas";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Reservas/MisReservas
        public async Task<IActionResult> MisReservas()
        {
            try
            {
                var usuario = await _userManager.GetUserAsync(User);
                if (usuario == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var reservas = await _context.Reservas
                    .Where(r => r.UsuarioId == usuario.Id)
                    .OrderByDescending(r => r.FechaReserva)
                    .ToListAsync();

                return View(reservas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar mis reservas");
                TempData["ErrorMessage"] = "Error al cargar tus reservas";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Reservas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var reserva = await _context.Reservas
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (reserva == null)
                {
                    return NotFound();
                }

                // Verificar que el usuario puede ver esta reserva
                var usuario = await _userManager.GetUserAsync(User);
                if (usuario != null && !User.IsInRole("Admin") && reserva.UsuarioId != usuario.Id)
                {
                    return Forbid();
                }

                return View(reserva);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar detalles de reserva {ReservaId}", id);
                TempData["ErrorMessage"] = "Error al cargar los detalles de la reserva";
                return RedirectToAction("MisReservas");
            }
        }

        // GET: Reservas/Create
        public async Task<IActionResult> Create(int? habitacionId)
        {
            try
            {
                ViewData["Habitaciones"] = await _context.Habitaciones
                    .Where(h => h.Disponible)
                    .OrderBy(h => h.Numero)
                    .ToListAsync();

                var model = new Reserva();
                if (habitacionId.HasValue)
                {
                    model.HabitacionId = habitacionId.Value;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar formulario de reserva");
                TempData["ErrorMessage"] = "Error al cargar el formulario de reserva";
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Reservas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Reserva reserva)
        {
            try
            {
                var usuario = await _userManager.GetUserAsync(User);
                if (usuario == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Validaciones básicas
                if (reserva.FechaInicio >= reserva.FechaFin)
                {
                    ModelState.AddModelError("", "La fecha de fin debe ser posterior a la fecha de inicio");
                }

                if (reserva.FechaInicio < DateTime.Today)
                {
                    ModelState.AddModelError("", "La fecha de inicio no puede ser anterior a hoy");
                }

                if (ModelState.IsValid)
                {
                    // Obtener la habitación para calcular el precio
                    var habitacion = await _context.Habitaciones.FindAsync(reserva.HabitacionId);
                    if (habitacion == null)
                    {
                        ModelState.AddModelError("", "Habitación no encontrada");
                        ViewData["Habitaciones"] = await _context.Habitaciones.Where(h => h.Disponible).ToListAsync();
                        return View(reserva);
                    }

                    // Calcular el total
                    var noches = (reserva.FechaFin - reserva.FechaInicio).Days;
                    reserva.Total = habitacion.PrecioPorNoche * noches;
                    reserva.UsuarioId = usuario.Id;
                    reserva.FechaReserva = DateTime.Now;
                    reserva.Estado = "Pendiente";

                    _context.Reservas.Add(reserva);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Reserva creada exitosamente";
                    return RedirectToAction("Details", new { id = reserva.Id });
                }

                ViewData["Habitaciones"] = await _context.Habitaciones.Where(h => h.Disponible).ToListAsync();
                return View(reserva);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear reserva");
                TempData["ErrorMessage"] = "Error al crear la reserva";
                ViewData["Habitaciones"] = await _context.Habitaciones.Where(h => h.Disponible).ToListAsync();
                return View(reserva);
            }
        }

        // POST: Reservas/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var reserva = await _context.Reservas.FindAsync(id);
                if (reserva == null)
                {
                    return NotFound();
                }

                // Verificar permisos
                var usuario = await _userManager.GetUserAsync(User);
                if (usuario != null && !User.IsInRole("Admin") && reserva.UsuarioId != usuario.Id)
                {
                    return Forbid();
                }

                reserva.Estado = "Cancelada";
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Reserva cancelada exitosamente";
                return RedirectToAction("MisReservas");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar reserva {ReservaId}", id);
                TempData["ErrorMessage"] = "Error al cancelar la reserva";
                return RedirectToAction("MisReservas");
            }
        }
    }
}