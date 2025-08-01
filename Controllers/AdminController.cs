using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HotelCostaAzulFinal.Data;
using HotelCostaAzulFinal.Models;

namespace HotelCostaAzulFinal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly HotelContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(HotelContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var model = new DashboardViewModel
                {
                    TotalHabitaciones = await _context.Habitaciones.CountAsync(),
                    HabitacionesDisponibles = await _context.Habitaciones.CountAsync(h => h.Disponible),
                    TotalReservas = await _context.Reservas.CountAsync(),
                    ReservasHoy = await _context.Reservas.CountAsync(r => r.FechaReserva.Date == DateTime.Today),
                    TotalClientes = await _context.Users.CountAsync(u => !u.EsAdmin),
                    ReservasConfirmadas = await _context.Reservas.CountAsync(r => r.Estado == "Confirmada"),
                    ReservasPendientes = await _context.Reservas.CountAsync(r => r.Estado == "Pendiente"),
                    IngresosTotales = await _context.Reservas
                        .Where(r => r.Estado == "Confirmada" || r.Estado == "Completada")
                        .SumAsync(r => r.Total),

                    // Últimas reservas con relaciones
                    UltimasReservas = await _context.Reservas
                        .Include(r => r.Usuario)
                        .Include(r => r.Habitacion)
                        .OrderByDescending(r => r.FechaReserva)
                        .Take(5)
                        .ToListAsync()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el dashboard");
                TempData["ErrorMessage"] = "Error al cargar el dashboard";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Admin/Usuarios
        public async Task<IActionResult> Usuarios()
        {
            var usuarios = await _context.Users
                .OrderBy(u => u.Apellido)
                .ThenBy(u => u.Nombre)
                .ToListAsync();

            return View(usuarios);
        }

        // GET: Admin/Habitaciones
        public async Task<IActionResult> Habitaciones()
        {
            var habitaciones = await _context.Habitaciones
                .OrderBy(h => h.Numero)
                .ToListAsync();

            return View(habitaciones);
        }

        // GET: Admin/Reservas
        public async Task<IActionResult> Reservas()
        {
            var reservas = await _context.Reservas
                .Include(r => r.Usuario)
                .Include(r => r.Habitacion)
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();

            return View(reservas);
        }

        // POST: Admin/CambiarEstadoReserva
        [HttpPost]
        public async Task<IActionResult> CambiarEstadoReserva(int id, string nuevoEstado)
        {
            try
            {
                var reserva = await _context.Reservas.FindAsync(id);
                if (reserva == null)
                {
                    return NotFound();
                }

                reserva.Estado = nuevoEstado;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Estado de reserva actualizado a: {nuevoEstado}";
                return RedirectToAction("Reservas");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado de reserva {ReservaId}", id);
                TempData["ErrorMessage"] = "Error al actualizar el estado de la reserva";
                return RedirectToAction("Reservas");
            }
        }
    }
}