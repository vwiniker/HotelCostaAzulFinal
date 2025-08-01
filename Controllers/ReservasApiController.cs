using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using HotelCostaAzulFinal.Data;
using HotelCostaAzulFinal.Models;

namespace HotelCostaAzulFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservasApiController : ControllerBase
    {
        private readonly HotelContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ReservasApiController> _logger;

        public ReservasApiController(
            HotelContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<ReservasApiController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: api/ReservasApi
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<object>>> GetReservas()
        {
            try
            {
                var reservas = await _context.Reservas
                    .Include(r => r.Usuario)
                    .Include(r => r.Habitacion)
                    .Select(r => new
                    {
                        r.Id,
                        r.FechaInicio,
                        r.FechaFin,
                        r.NumeroHuespedes,
                        r.Total,
                        r.Estado,
                        r.FechaReserva,
                        r.NotasEspeciales,
                        Usuario = new
                        {
                            r.Usuario.Id,
                            r.Usuario.Nombre,
                            r.Usuario.Apellido,
                            r.Usuario.Email
                        },
                        Habitacion = new
                        {
                            r.Habitacion.Id,
                            r.Habitacion.Numero,
                            r.Habitacion.Tipo,
                            r.Habitacion.PrecioPorNoche
                        }
                    })
                    .OrderByDescending(r => r.FechaReserva)
                    .ToListAsync();

                return Ok(reservas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservas");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        // GET: api/ReservasApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetReserva(int id)
        {
            try
            {
                var reserva = await _context.Reservas
                    .Include(r => r.Usuario)
                    .Include(r => r.Habitacion)
                    .Where(r => r.Id == id)
                    .Select(r => new
                    {
                        r.Id,
                        r.FechaInicio,
                        r.FechaFin,
                        r.NumeroHuespedes,
                        r.Total,
                        r.Estado,
                        r.FechaReserva,
                        r.NotasEspeciales,
                        Usuario = new
                        {
                            r.Usuario.Id,
                            r.Usuario.Nombre,
                            r.Usuario.Apellido,
                            r.Usuario.Email,
                            r.Usuario.PhoneNumber
                        },
                        Habitacion = new
                        {
                            r.Habitacion.Id,
                            r.Habitacion.Numero,
                            r.Habitacion.Tipo,
                            r.Habitacion.Descripcion,
                            r.Habitacion.PrecioPorNoche,
                            r.Habitacion.Capacidad,
                            r.Habitacion.Caracteristicas
                        }
                    })
                    .FirstOrDefaultAsync();

                if (reserva == null)
                {
                    return NotFound(new { mensaje = $"Reserva con ID {id} no encontrada" });
                }

                return Ok(reserva);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reserva {ReservaId}", id);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        // POST: api/ReservasApi
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<object>> PostReserva(CreateReservaDto reservaDto)
        {
            try
            {
                // Obtener el usuario actual
                var usuario = await _userManager.GetUserAsync(User);
                if (usuario == null)
                {
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });
                }

                // Validar que la habitación existe y está disponible
                var habitacion = await _context.Habitaciones.FindAsync(reservaDto.HabitacionId);
                if (habitacion == null)
                {
                    return BadRequest(new { mensaje = "Habitación no encontrada" });
                }

                if (!habitacion.Disponible)
                {
                    return BadRequest(new { mensaje = "Habitación no está disponible" });
                }

                // Validar fechas
                if (reservaDto.FechaInicio >= reservaDto.FechaFin)
                {
                    return BadRequest(new { mensaje = "La fecha de fin debe ser posterior a la fecha de inicio" });
                }

                if (reservaDto.FechaInicio < DateTime.Today)
                {
                    return BadRequest(new { mensaje = "La fecha de inicio no puede ser anterior a hoy" });
                }

                // Verificar disponibilidad en las fechas solicitadas
                var conflicto = await _context.Reservas.AnyAsync(r =>
                    r.HabitacionId == reservaDto.HabitacionId &&
                    r.Estado != "Cancelada" &&
                    r.FechaInicio < reservaDto.FechaFin &&
                    r.FechaFin > reservaDto.FechaInicio);

                if (conflicto)
                {
                    return BadRequest(new { mensaje = "La habitación no está disponible en las fechas seleccionadas" });
                }

                // Calcular el total
                var noches = (reservaDto.FechaFin - reservaDto.FechaInicio).Days;
                var total = habitacion.PrecioPorNoche * noches;

                var reserva = new Reserva
                {
                    UsuarioId = usuario.Id,
                    HabitacionId = reservaDto.HabitacionId,
                    FechaInicio = reservaDto.FechaInicio,
                    FechaFin = reservaDto.FechaFin,
                    NumeroHuespedes = reservaDto.NumeroHuespedes,
                    Total = total,
                    Estado = "Pendiente",
                    NotasEspeciales = reservaDto.NotasEspeciales,
                    FechaReserva = DateTime.Now
                };

                _context.Reservas.Add(reserva);
                await _context.SaveChangesAsync();

                // Retornar la reserva creada con datos completos
                var reservaCreada = await _context.Reservas
                    .Include(r => r.Usuario)
                    .Include(r => r.Habitacion)
                    .Where(r => r.Id == reserva.Id)
                    .Select(r => new
                    {
                        r.Id,
                        r.FechaInicio,
                        r.FechaFin,
                        r.NumeroHuespedes,
                        r.Total,
                        r.Estado,
                        r.FechaReserva,
                        r.NotasEspeciales,
                        Usuario = new
                        {
                            r.Usuario.Nombre,
                            r.Usuario.Apellido,
                            r.Usuario.Email
                        },
                        Habitacion = new
                        {
                            r.Habitacion.Numero,
                            r.Habitacion.Tipo,
                            r.Habitacion.PrecioPorNoche
                        }
                    })
                    .FirstOrDefaultAsync();

                return CreatedAtAction("GetReserva", new { id = reserva.Id }, reservaCreada);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear reserva");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        // PUT: api/ReservasApi/5/estado
        [HttpPut("{id}/estado")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoDto dto)
        {
            try
            {
                var reserva = await _context.Reservas.FindAsync(id);
                if (reserva == null)
                {
                    return NotFound(new { mensaje = $"Reserva con ID {id} no encontrada" });
                }

                var estadosValidos = new[] { "Pendiente", "Confirmada", "Cancelada", "Completada" };
                if (!estadosValidos.Contains(dto.NuevoEstado))
                {
                    return BadRequest(new { mensaje = "Estado no válido" });
                }

                reserva.Estado = dto.NuevoEstado;
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = $"Estado de reserva actualizado a: {dto.NuevoEstado}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado de reserva {ReservaId}", id);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        // GET: api/ReservasApi/disponibilidad
        [HttpGet("disponibilidad")]
        public async Task<ActionResult<object>> ConsultarDisponibilidad(
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFin)
        {
            try
            {
                if (fechaInicio >= fechaFin)
                {
                    return BadRequest(new { mensaje = "La fecha de fin debe ser posterior a la fecha de inicio" });
                }

                // Obtener habitaciones ocupadas en el rango de fechas
                var habitacionesOcupadas = await _context.Reservas
                    .Where(r => r.Estado != "Cancelada" &&
                               r.FechaInicio < fechaFin &&
                               r.FechaFin > fechaInicio)
                    .Select(r => r.HabitacionId)
                    .ToListAsync();

                // Obtener habitaciones disponibles
                var habitacionesDisponibles = await _context.Habitaciones
                    .Where(h => h.Disponible && !habitacionesOcupadas.Contains(h.Id))
                    .Select(h => new
                    {
                        h.Id,
                        h.Numero,
                        h.Tipo,
                        h.Descripcion,
                        h.PrecioPorNoche,
                        h.Capacidad,
                        h.Caracteristicas,
                        TotalNoches = (fechaFin - fechaInicio).Days,
                        TotalEstimado = h.PrecioPorNoche * (fechaFin - fechaInicio).Days
                    })
                    .OrderBy(h => h.PrecioPorNoche)
                    .ToListAsync();

                return Ok(new
                {
                    FechaConsulta = DateTime.Now,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    HabitacionesDisponibles = habitacionesDisponibles.Count,
                    Habitaciones = habitacionesDisponibles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar disponibilidad");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        // DELETE: api/ReservasApi/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> CancelarReserva(int id)
        {
            try
            {
                var reserva = await _context.Reservas.FindAsync(id);
                if (reserva == null)
                {
                    return NotFound(new { mensaje = $"Reserva con ID {id} no encontrada" });
                }

                // Verificar que el usuario puede cancelar esta reserva
                var usuario = await _userManager.GetUserAsync(User);
                if (usuario == null || (!User.IsInRole("Admin") && reserva.UsuarioId != usuario.Id))
                {
                    return Forbid();
                }

                // En lugar de eliminar, cancelar la reserva
                reserva.Estado = "Cancelada";
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Reserva cancelada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar reserva {ReservaId}", id);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        // GET: api/ReservasApi/usuario/{userId}
        [HttpGet("usuario/{userId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<object>>> GetReservasUsuario(string userId)
        {
            try
            {
                var usuario = await _userManager.GetUserAsync(User);
                if (usuario == null || (!User.IsInRole("Admin") && usuario.Id != userId))
                {
                    return Forbid();
                }

                var reservas = await _context.Reservas
                    .Include(r => r.Habitacion)
                    .Where(r => r.UsuarioId == userId)
                    .Select(r => new
                    {
                        r.Id,
                        r.FechaInicio,
                        r.FechaFin,
                        r.NumeroHuespedes,
                        r.Total,
                        r.Estado,
                        r.FechaReserva,
                        r.NotasEspeciales,
                        Habitacion = new
                        {
                            r.Habitacion.Numero,
                            r.Habitacion.Tipo,
                            r.Habitacion.Descripcion,
                            r.Habitacion.PrecioPorNoche
                        }
                    })
                    .OrderByDescending(r => r.FechaReserva)
                    .ToListAsync();

                return Ok(reservas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservas del usuario {UserId}", userId);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }
    }

    // DTOs para la API
    public class CreateReservaDto
    {
        public int HabitacionId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int NumeroHuespedes { get; set; } = 1;
        public string? NotasEspeciales { get; set; }
    }

    public class CambiarEstadoDto
    {
        public string NuevoEstado { get; set; } = string.Empty;
    }
}