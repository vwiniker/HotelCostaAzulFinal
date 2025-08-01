using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelCostaAzulFinal.Data;
using HotelCostaAzulFinal.Models;

namespace HotelCostaAzulFinal.Controllers
{
    public class AyudaController : Controller
    {
        private readonly HotelContext _context;
        private readonly ILogger<AyudaController> _logger;
        private readonly IWebHostEnvironment _environment;

        public AyudaController(HotelContext context, ILogger<AyudaController> logger, IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        // GET: Ayuda
        public IActionResult Index()
        {
            var seccionesAyuda = new List<SeccionAyuda>
            {
                new SeccionAyuda
                {
                    Titulo = "🏨 Bienvenido al Sistema de Reservas",
                    Contenido = "Aprende a usar nuestro sistema de reservas paso a paso",
                    Icono = "fas fa-home",
                    Orden = 1
                },
                new SeccionAyuda
                {
                    Titulo = "📋 Cómo Hacer una Reserva",
                    Contenido = "Guía completa para realizar tu reserva en línea",
                    Icono = "fas fa-calendar-plus",
                    Orden = 2
                },
                new SeccionAyuda
                {
                    Titulo = "👤 Gestión de Cuenta",
                    Contenido = "Administra tu perfil y configuraciones",
                    Icono = "fas fa-user-cog",
                    Orden = 3
                },
                new SeccionAyuda
                {
                    Titulo = "💳 Pagos y Facturación",
                    Contenido = "Información sobre métodos de pago y facturación",
                    Icono = "fas fa-credit-card",
                    Orden = 4
                },
                new SeccionAyuda
                {
                    Titulo = "🛎️ Servicios del Hotel",
                    Contenido = "Conoce todos los servicios disponibles",
                    Icono = "fas fa-concierge-bell",
                    Orden = 5
                },
                new SeccionAyuda
                {
                    Titulo = "❓ Preguntas Frecuentes",
                    Contenido = "Respuestas a las dudas más comunes",
                    Icono = "fas fa-question-circle",
                    Orden = 6
                }
            };

            return View(seccionesAyuda);
        }

        // GET: Ayuda/ComoReservar
        public IActionResult ComoReservar()
        {
            return View();
        }

        // GET: Ayuda/GestionCuenta
        public IActionResult GestionCuenta()
        {
            return View();
        }

        // GET: Ayuda/PagosFacturacion
        public IActionResult PagosFacturacion()
        {
            return View();
        }

        // GET: Ayuda/ServiciosHotel
        public IActionResult ServiciosHotel()
        {
            return View();
        }

        // GET: Ayuda/PreguntasFrecuentes
        public IActionResult PreguntasFrecuentes()
        {
            var faqs = new List<FAQ>
            {
                new FAQ
                {
                    Pregunta = "¿Cómo puedo hacer una reserva?",
                    Respuesta = "Puedes hacer una reserva accediendo a la sección de habitaciones, seleccionando las fechas deseadas y siguiendo el proceso de reserva paso a paso.",
                    Categoria = "Reservas"
                },
                new FAQ
                {
                    Pregunta = "¿Puedo cancelar mi reserva?",
                    Respuesta = "Sí, puedes cancelar tu reserva desde tu panel de 'Mis Reservas'. Las políticas de cancelación varían según el tipo de tarifa seleccionada.",
                    Categoria = "Reservas"
                },
                new FAQ
                {
                    Pregunta = "¿Qué métodos de pago aceptan?",
                    Respuesta = "Aceptamos tarjetas de crédito Visa, MasterCard, American Express y transferencias bancarias locales.",
                    Categoria = "Pagos"
                },
                new FAQ
                {
                    Pregunta = "¿El hotel incluye desayuno?",
                    Respuesta = "Sí, todas nuestras habitaciones incluyen desayuno buffet continental. También ofrecemos opciones vegetarianas y veganas.",
                    Categoria = "Servicios"
                },
                new FAQ
                {
                    Pregunta = "¿Tienen servicio de transporte al aeropuerto?",
                    Respuesta = "Sí, ofrecemos servicio de transporte al aeropuerto. Debe solicitarse con al menos 24 horas de anticipación.",
                    Categoria = "Servicios"
                },
                new FAQ
                {
                    Pregunta = "¿Qué sucede si llego tarde al hotel?",
                    Respuesta = "Nuestro servicio de recepción está disponible 24/7. Si llegará después de las 11:00 PM, por favor notifíquenos para coordinar su llegada.",
                    Categoria = "Check-in"
                },
                new FAQ
                {
                    Pregunta = "¿Puedo modificar mi reserva?",
                    Respuesta = "Sí, puedes modificar tu reserva sujeto a disponibilidad. Los cambios pueden conllevar cargos adicionales según las tarifas aplicables.",
                    Categoria = "Reservas"
                },
                new FAQ
                {
                    Pregunta = "¿Las habitaciones tienen aire acondicionado?",
                    Respuesta = "Sí, todas nuestras habitaciones cuentan con aire acondicionado, WiFi gratuito y televisión por cable.",
                    Categoria = "Habitaciones"
                },
                new FAQ
                {
                    Pregunta = "¿Aceptan mascotas?",
                    Respuesta = "Sí, somos pet-friendly. Aceptamos mascotas pequeñas y medianas con un cargo adicional de ₡15,000 por noche.",
                    Categoria = "Servicios"
                },
                new FAQ
                {
                    Pregunta = "¿Cómo puedo obtener una factura?",
                    Respuesta = "Las facturas se generan automáticamente y se envían por email. También puedes descargarlas desde tu panel de usuario.",
                    Categoria = "Facturación"
                }
            };

            return View(faqs);
        }

        // GET: Ayuda/Contacto
        public IActionResult Contacto()
        {
            return View(new ConsultaAyuda());
        }

        // POST: Ayuda/EnviarConsulta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarConsulta(ConsultaAyuda consulta, IFormFile? imagen)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Procesar imagen si se subió
                    if (imagen != null && imagen.Length > 0)
                    {
                        // Validar tipo de archivo
                        var tiposPermitidos = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();

                        if (!tiposPermitidos.Contains(extension))
                        {
                            ModelState.AddModelError("imagen", "Solo se permiten archivos de imagen (JPG, PNG, GIF)");
                            return View("Contacto", consulta);
                        }

                        // Validar tamaño (máximo 5MB)
                        if (imagen.Length > 5 * 1024 * 1024)
                        {
                            ModelState.AddModelError("imagen", "El archivo no puede ser mayor a 5MB");
                            return View("Contacto", consulta);
                        }

                        // Crear directorio si no existe
                        var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "consultas");
                        Directory.CreateDirectory(uploadsPath);

                        // Guardar imagen
                        var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                        var rutaCompleta = Path.Combine(uploadsPath, nombreArchivo);

                        using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                        {
                            await imagen.CopyToAsync(stream);
                        }

                        consulta.RutaImagen = $"/uploads/consultas/{nombreArchivo}";
                    }

                    consulta.FechaEnvio = DateTime.Now;
                    consulta.Estado = "Pendiente";

                    // Aquí normalmente guardarías en base de datos
                    // _context.ConsultasAyuda.Add(consulta);
                    // await _context.SaveChangesAsync();

                    // Por ahora, simular el guardado
                    _logger.LogInformation("Nueva consulta recibida de {Email}: {Asunto}", consulta.Email, consulta.Asunto);

                    TempData["SuccessMessage"] = "Tu consulta ha sido enviada exitosamente. Te responderemos pronto.";
                    return RedirectToAction("Contacto");
                }

                return View("Contacto", consulta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar consulta de ayuda");
                TempData["ErrorMessage"] = "Error al enviar la consulta. Por favor intenta de nuevo.";
                return View("Contacto", consulta);
            }
        }

        // GET: Ayuda/TutorialVideo
        public IActionResult TutorialVideo()
        {
            return View();
        }

        // GET: Ayuda/GuiaRapida
        public IActionResult GuiaRapida()
        {
            return View();
        }
    }

    // Modelos para la ayuda
    public class SeccionAyuda
    {
        public string Titulo { get; set; } = string.Empty;
        public string Contenido { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;
        public int Orden { get; set; }
    }

    public class FAQ
    {
        public string Pregunta { get; set; } = string.Empty;
        public string Respuesta { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
    }
}