using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using HotelCostaAzulFinal.Models;
using HotelCostaAzulFinal.ViewModels;

namespace HotelCostaAzulFinal.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // GET: Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                _logger.LogInformation("Usuario {Email} intentó iniciar sesión", model.Email);

                var result = await _signInManager.PasswordSignInAsync(
                    model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Usuario {Email} inició sesión exitosamente", model.Email);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        // Redirigir según el rol
                        var user = await _userManager.FindByEmailAsync(model.Email);
                        if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
                        {
                            TempData["SuccessMessage"] = $"¡Bienvenido de vuelta, {user.Nombre}!";
                            return RedirectToAction("Dashboard", "Admin");
                        }
                        else
                        {
                            TempData["SuccessMessage"] = $"¡Bienvenido de vuelta, {user?.Nombre}!";
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
                else if (result.IsLockedOut)
                {
                    _logger.LogWarning("Usuario {Email} está bloqueado", model.Email);
                    ModelState.AddModelError(string.Empty, "Tu cuenta está temporalmente bloqueada. Intenta más tarde.");
                }
                else
                {
                    _logger.LogWarning("Intento de login fallido para {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos");
                }
            }

            return View(model);
        }

        // GET: Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validar que el email no esté ya registrado
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Este email ya está registrado");
                    return View(model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    Documento = model.Documento,
                    TipoDocumento = model.TipoDocumento,
                    PhoneNumber = model.Telefono,
                    FechaRegistro = DateTime.Now,
                    EsAdmin = false
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Usuario {Email} se registró exitosamente", model.Email);

                    // Asignar rol de Cliente
                    if (!await _roleManager.RoleExistsAsync("Cliente"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Cliente"));
                    }
                    await _userManager.AddToRoleAsync(user, "Cliente");

                    // Login automático después del registro
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    TempData["SuccessMessage"] = $"¡Registro exitoso! Bienvenido a Hotel Costa Azul, {user.Nombre}!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    _logger.LogError("Error al registrar usuario {Email}: {Errors}",
                        model.Email, string.Join(", ", result.Errors.Select(e => e.Description)));

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return View(model);
        }

        // POST: Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userName = User.Identity?.Name;
            await _signInManager.SignOutAsync();

            _logger.LogInformation("Usuario {UserName} cerró sesión", userName);
            TempData["InfoMessage"] = "Has cerrado sesión exitosamente";
            return RedirectToAction("Index", "Home");
        }

        // GET: AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Método para crear admin inicial (solo desarrollo)
        [HttpGet]
        public async Task<IActionResult> CreateAdmin()
        {
#if DEBUG
            // Solo en desarrollo y si no hay admin
            if (!_userManager.Users.Any(u => u.EsAdmin))
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@hotelcostaazul.com",
                    Email = "admin@hotelcostaazul.com",
                    Nombre = "Administrador",
                    Apellido = "Hotel",
                    Documento = "000000000",
                    TipoDocumento = "Cedula",
                    EsAdmin = true,
                    EmailConfirmed = true,
                    FechaRegistro = DateTime.Now
                };

                var result = await _userManager.CreateAsync(admin, "Admin123!");

                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("Admin"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    }
                    await _userManager.AddToRoleAsync(admin, "Admin");

                    _logger.LogInformation("Usuario admin creado exitosamente");
                    return Json(new { success = true, message = "Admin creado: admin@hotelcostaazul.com / Admin123!" });
                }
                else
                {
                    _logger.LogError("Error al crear admin: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    return Json(new { success = false, message = string.Join(", ", result.Errors.Select(e => e.Description)) });
                }
            }

            return Json(new { success = false, message = "Ya existe un administrador" });
#else
            return NotFound();
#endif
        }

        // GET: Profile (para ver/editar perfil)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                Email = user.Email!,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Documento = user.Documento,
                TipoDocumento = user.TipoDocumento,
                Telefono = user.PhoneNumber,
                FechaRegistro = user.FechaRegistro
            };

            return View(model);
        }

        // POST: Profile (actualizar perfil)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                user.Nombre = model.Nombre;
                user.Apellido = model.Apellido;
                user.PhoneNumber = model.Telefono;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Perfil actualizado exitosamente";
                    return RedirectToAction("Profile");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return View(model);
        }

        // GET: ChangePassword
        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);
                    TempData["SuccessMessage"] = "Contraseña cambiada exitosamente";
                    return RedirectToAction("Profile");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return View(model);
        }
    }
}