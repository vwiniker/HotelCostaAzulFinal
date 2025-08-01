using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using HotelCostaAzulFinal.Data;
using HotelCostaAzulFinal.Models;

namespace HotelCostaAzulFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HabitacionesController : ControllerBase
    {
        private readonly HotelContext _context;
        private readonly ILogger<HabitacionesController> _logger;

        public HabitacionesController(HotelContext context, ILogger<HabitacionesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Habitaciones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetHabitaciones()
        {
            try
            {
                var habitaciones = await _context.Habitaciones
                    .Select(h => new
                    {
                        h.Id,
                        h.Numero,
                        h.Tipo,
                        h.Descripcion,
                        h.PrecioPorNoche,
                        h.Capacidad,
                        h.Caracteristicas,
                        h.Disponible,
                        h.ImagenUrl,
                        h.FechaCreacion
                    })
                    .OrderBy(h => h.Numero)
                    .ToListAsync();

                return Ok(habitaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener habitaciones");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        // GET: api/Habitaciones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetHabitacion(int id)
        {
            try
            {
                var habitacion = await _context.Habitaciones
                    .Where(h => h.Id == id)
                    .Select(h => new
                    {
                        h.Id,
                        h.Numero,
                        h.Tipo,
                        h.Descripcion,
                        h.PrecioPorNoche,
                        h.Capacidad,
                        h.Caracteristicas,
                        h.Disponible,
                        h.ImagenUrl,
                        h.FechaCreacion,
                        ReservasActivas = h.Reservas.Count(r => r.Estado != "Cancelada" && r.Estado != "Completada")
                    })
                    .FirstOrDefaultAsync();

                if (habitacion == null)
                {
                    return NotFound(new { mensaje = $"Habitación con ID {id} no encontrada" });
                }

                return Ok(habitacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener habitación {HabitacionId}", id);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        // GET: api/Habitaciones/disponibles
        [HttpGet("disponibles")]
        public async Task<ActionResult<IEnumerable<object>>> GetHabitacionesDisponibles()
        {
            try
            {
                var habitaciones = await _context.Habitaciones
                    .Where(h => h.Disponible)
                    .Select(h => new
                    {
                        h.Id,
                        h.Numero,
                        h.Tipo,
                        h.Descripcion,
                        h.PrecioPorNoche,
                        h.Capacidad,
                        h.Caracteristicas,
                        h.ImagenUrl
                    })
                    .OrderBy(h => h.PrecioPorNoche)
                    .ToListAsync();

                return Ok(habitaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener habitaciones disponibles");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        // GET: api/Habitaciones/tipo/{tipo}
        [HttpGet("tipo/{tipo}")]
        public async Task<ActionResult<IEnumerable<object>>> GetHabitacionesPorTipo(string tipo)
        {
            try
            {
                var habitaciones = await _context.Habitaciones
                    .Where(h => h.Tipo.ToLower() == tipo.ToLower())
                    .Select(h => new
                    {
                        h.Id,
                        h.Numero,
                        h.Tipo,
                        h.Descripcion,
                        h.PrecioPorNoche,
                        h.Capacidad,
                        h.Caracteristicas,
                        h.Disponible,
                        h.ImagenUrl
                    })
                    .OrderBy(h => h.PrecioPorNoche)
                    .ToListAsync();

                return Ok(habitaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener habitaciones por tipo {Tipo}", tipo);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        // GET: api/Habitaciones/buscar
        [HttpGet("buscar")]
        public async Task<ActionResult<IEnumerable<object>>> BuscarHabitaciones(
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin,
            [FromQuery] int? capacidadMinima,
            [FromQuery] decimal? precioMaximo,
            [FromQuery] string? tipo)
        {
            try
            {
                var query = _context.Habitaciones.Where(h => h.Disponible);

                // Filtrar por disponibilidad en fechas
                if (fechaInicio.HasValue && fechaFin.HasValue)
                {
                    var habitacionesOcupadas = await _context.Reservas
                        .Where(r => r.Estado != "Cancelada" &&
                                   r.FechaInicio < fechaFin &&
                                   r.FechaFin > fechaInicio)
                        .Select(r => r.HabitacionId)
                        .ToListAsync();

                    query = query.Where(h => !habitacionesOcupadas.Contains(h.Id));
                }

                // Aplicar otros filtros
                if (capacidadMinima.HasValue)
                {
                    query = query.Where(h => h.Capacidad >= capacidadMinima.Value);
                }

                if (precioMaximo.HasValue)
                {
                    query = query.Where(h => h.PrecioPorNoche <= precioMaximo.Value);
                }

                if (!string.IsNullOrEmpty(tipo))
                {
                    query = query.Where(h => h.Tipo.ToLower().Contains(tipo.ToLower()));
                }

                var habitaciones = await query
                    .Select(h => new
                    {
                        h.Id,
                        h.Numero,
                        h.Tipo,
                        h.Descripcion,
                        h.PrecioPorNoche,
                        h.Capacidad,
                        h.Caracteristicas,
                        h.ImagenUrl,
                        TotalEstimado = fechaInicio.HasValue && fechaFin.HasValue
                            ? h.PrecioPorNoche * (fechaFin.Value - fechaInicio.Value).Days
                            : h.PrecioPorNoche
                    })
                    .OrderBy(h => h.PrecioPorNoche)
                    .ToListAsync();

                return Ok(new
                {
                    FechaConsulta = DateTime.Now,
                    Filtros = new
                    {
                        FechaInicio = fechaInicio,
                        FechaFin = fechaFin,
                        CapacidadMinima = capacidadMinima,
                        PrecioMaximo = precioMaximo,
                        Tipo = tipo
                    },
                    TotalEncontradas = habitaciones.Count,
                    Habitaciones = habitaciones
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda de habitaciones");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        // PUT: api/Habitaciones/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutHabitacion(int id, UpdateHabitacionDto habitacionDto)
        {
            try
            {
                var habitacion = await _context.Habitaciones.FindAsync(id);
                if (habitacion == null)
                {
                    return NotFound(new { mensaje = $"Habitación con ID {id} no encontrada" });
                }

                // Actualizar propiedades
                habitacion.Descripcion = habitacionDto.Descripcion ?? habitacion.Descripcion;
                habitacion.PrecioPorNoche = habitacionDto.PrecioPorNoche ?? habitacion.PrecioPorNoche;
                habitacion.Caracteristicas = habitacionDto.Caracteristicas ?? habitacion.Caracteristicas;
                habitacion.Disponible = habitacionDto.Disponible ?? habitacion.Disponible;

                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Habitación actualizada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar habitación {HabitacionId}", id);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        // POST: api/Habitaciones
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> PostHabitacion(CreateHabitacionDto habitacionDto)
        {
            try
            {
                // Verificar que no exista una habitación con el mismo número
                var existeHabitacion = await _context.Habitaciones
                    .AnyAsync(h => h.Numero == habitacionDto.Numero);

                if (existeHabitacion)
                {
                    return BadRequest(new { mensaje = $"Ya existe una habitación con el número {habitacionDto.Numero}" });
                }

                var habitacion = new Habitacion
                {
                    Numero = habitacionDto.Numero,
                    Tipo = habitacionDto.Tipo,
                    Descripcion = habitacionDto.Descripcion,
                    PrecioPorNoche = habitacionDto.PrecioPorNoche,
                    Capacidad = habitacionDto.Capacidad,
                    Caracteristicas = habitacionDto.Caracteristicas,
                    Disponible = true,
                    FechaCreacion = DateTime.Now
                };

                _context.Habitaciones.Add(habitacion);
                await _context.SaveChangesAsync();

                var habitacionCreada = new
                {
                    habitacion.Id,
                    habitacion.Numero,
                    habitacion.Tipo,
                    habitacion.Descripcion,
                    habitacion.PrecioPorNoche,
                    habitacion.Capacidad,
                    habitacion.Caracteristicas,
                    habitacion.Disponible,
                    habitacion.FechaCreacion
                };

                return CreatedAtAction("GetHabitacion", new { id = habitacion.Id }, habitacionCreada);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear habitación");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        // PATCH: api/Habitaciones/5/disponibilidad
        [HttpPatch("{id}/disponibilidad")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CambiarDisponibilidad(int id, [FromBody] bool disponible)
        {
            try
            {
                var habitacion = await _context.Habitaciones.FindAsync(id);
                if (habitacion == null)
                {
                    return NotFound(new { mensaje = $"Habitación con ID {id} no encontrada" });
                }

                habitacion.Disponible = disponible;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    mensaje = $"Habitación {habitacion.Numero} marcada como {(disponible ? "disponible" : "no disponible")}",
                    habitacion = new
                    {
                        habitacion.Id,
                        habitacion.Numero,
                        habitacion.Disponible
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar disponibilidad de habitación {HabitacionId}", id);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        // GET: api/Habitaciones/estadisticas
        [HttpGet("estadisticas")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> GetEstadisticas()
        {
            try
            {
                var estadisticas = new
                {
                    TotalHabitaciones = await _context.Habitaciones.CountAsync(),
                    HabitacionesDisponibles = await _context.Habitaciones.CountAsync(h => h.Disponible),
                    HabitacionesOcupadas = await _context.Habitaciones.CountAsync(h => !h.Disponible),
                    PrecioPromedio = await _context.Habitaciones.AverageAsync(h => h.PrecioPorNoche),
                    PrecioMinimo = await _context.Habitaciones.MinAsync(h => h.PrecioPorNoche),
                    PrecioMaximo = await _context.Habitaciones.MaxAsync(h => h.PrecioPorNoche),
                    HabitacionesPorTipo = await _context.Habitaciones
                        .GroupBy(h => h.Tipo)
                        .Select(g => new { Tipo = g.Key, Cantidad = g.Count() })
                        .ToListAsync(),
                    CapacidadTotal = await _context.Habitaciones.SumAsync(h => h.Capacidad),
                    FechaConsulta = DateTime.Now
                };

                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de habitaciones");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }
    }

    // DTOs para Habitaciones
    public class CreateHabitacionDto
    {
        public string Numero { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal PrecioPorNoche { get; set; }
        public int Capacidad { get; set; }