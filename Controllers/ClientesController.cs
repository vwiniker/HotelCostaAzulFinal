using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HotelCostaAzulFinal.Data;
using HotelCostaAzulFinal.Models;

namespace HotelCostaAzulFinal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ClientesController : Controller
    {
        private readonly HotelContext _context;
        private readonly ILogger<ClientesController> _logger;

        public ClientesController(HotelContext context, ILogger<ClientesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Clientes
        public async Task<IActionResult> Index()
        {
            try
            {
                // Ahora usamos ApplicationUser en lugar de Cliente
                var usuarios = await _context.Users
                    .Where(u => !u.EsAdmin) // Solo clientes, no admins
                    .Include(u => u.Reservas)
                    .ThenInclude(r => r.Habitacion)
                    .OrderBy(u => u.Apellido)
                    .ThenBy(u => u.Nombre)
                    .ToListAsync();

                return View(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la lista de clientes");
                TempData["ErrorMessage"] = "Error al cargar la lista de clientes";
                return RedirectToAction("Dashboard", "Admin");
            }
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var usuario = await _context.Users
                    .Include(u => u.Reservas)
                    .ThenInclude(r => r.Habitacion)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (usuario == null)
                {
                    return NotFound();
                }

                return View(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar detalles del cliente {ClienteId}", id);
                TempData["ErrorMessage"] = "Error al cargar los detalles del cliente";
                return RedirectToAction("Index");
            }
        }

        // POST: Clientes/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            try
            {
                var usuario = await _context.Users.FindAsync(id);
                if (usuario == null)
                {
                    return NotFound();
                }

                // Alternar el estado de bloqueo del usuario
                usuario.LockoutEnabled = !usuario.LockoutEnabled;
                if (usuario.LockoutEnabled)
                {
                    usuario.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100); // Bloquear por mucho tiempo
                }
                else
                {
                    usuario.LockoutEnd = null; // Desbloquear
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = usuario.LockoutEnabled
                    ? $"Usuario {usuario.NombreCompleto} ha sido bloqueado"
                    : $"Usuario {usuario.NombreCompleto} ha sido desbloqueado";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado del usuario {UserId}", id);
                TempData["ErrorMessage"] = "Error al cambiar el estado del usuario";
                return RedirectToAction("Index");
            }
        }

        // GET: Clientes/Reservas/5
        public async Task<IActionResult> Reservas(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var usuario = await _context.Users
                    .Include(u => u.Reservas)
                    .ThenInclude(r => r.Habitacion)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (usuario == null)
                {
                    return NotFound();
                }

                ViewData["ClienteNombre"] = usuario.NombreCompleto;
                return View(usuario.Reservas.OrderByDescending(r => r.FechaReserva).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar reservas del cliente {ClienteId}", id);
                TempData["ErrorMessage"] = "Error al cargar las reservas del cliente";
                return RedirectToAction("Index");
            }
        }
    }
}