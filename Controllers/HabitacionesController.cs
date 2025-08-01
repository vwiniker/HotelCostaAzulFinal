using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelCostaAzulFinal.Data;
using HotelCostaAzulFinal.Models;

namespace HotelCostaAzulFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HabitacionesController : ControllerBase
    {
        private readonly HotelContext _context;

        public HabitacionesController(HotelContext context)
        {
            _context = context;
        }

        // GET: api/Habitaciones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Habitacion>>> GetHabitaciones()
        {
            return await _context.Habitaciones.ToListAsync();
        }

        // GET: api/Habitaciones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Habitacion>> GetHabitacion(int id)
        {
            var habitacion = await _context.Habitaciones.FindAsync(id);

            if (habitacion == null)
            {
                return NotFound();
            }

            return habitacion;
        }

        // GET: api/Habitaciones/disponibles
        [HttpGet("disponibles")]
        public async Task<ActionResult<IEnumerable<Habitacion>>> GetHabitacionesDisponibles()
        {
            return await _context.Habitaciones
                .Where(h => h.Disponible)
                .ToListAsync();
        }

        // GET: api/Habitaciones/tipo/{tipo}
        [HttpGet("tipo/{tipo}")]
        public async Task<ActionResult<IEnumerable<Habitacion>>> GetHabitacionesPorTipo(string tipo)
        {
            return await _context.Habitaciones
                .Where(h => h.Tipo.ToLower() == tipo.ToLower())
                .ToListAsync();
        }

        // PUT: api/Habitaciones/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHabitacion(int id, Habitacion habitacion)
        {
            if (id != habitacion.Id)
            {
                return BadRequest();
            }

            _context.Entry(habitacion).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HabitacionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Habitaciones
        [HttpPost]
        public async Task<ActionResult<Habitacion>> PostHabitacion(Habitacion habitacion)
        {
            _context.Habitaciones.Add(habitacion);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetHabitacion", new { id = habitacion.Id }, habitacion);
        }

        // DELETE: api/Habitaciones/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHabitacion(int id)
        {
            var habitacion = await _context.Habitaciones.FindAsync(id);
            if (habitacion == null)
            {
                return NotFound();
            }

            _context.Habitaciones.Remove(habitacion);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/Habitaciones/5/disponibilidad
        [HttpPatch("{id}/disponibilidad")]
        public async Task<IActionResult> CambiarDisponibilidad(int id, [FromBody] bool disponible)
        {
            var habitacion = await _context.Habitaciones.FindAsync(id);
            if (habitacion == null)
            {
                return NotFound();
            }

            habitacion.Disponible = disponible;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = $"Habitación {habitacion.Numero} marcada como {(disponible ? "disponible" : "no disponible")}" });
        }

        private bool HabitacionExists(int id)
        {
            return _context.Habitaciones.Any(e => e.Id == id);
        }
    }
}