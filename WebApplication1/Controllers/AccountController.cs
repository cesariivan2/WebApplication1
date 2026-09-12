using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(
            SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
        }

        // ==========================================
        // ABRIR LOGIN
        // ==========================================
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(
                    "Index",
                    "Home"
                );
            }

            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        // ==========================================
        // INICIAR SESIÓN
        // ==========================================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string email,
            string password,
            bool rememberMe = false,
            string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(
                    "email",
                    "El correo es obligatorio."
                );
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "password",
                    "La contraseña es obligatoria."
                );
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;

                return View();
            }

            var result = await _signInManager
                .PasswordSignInAsync(
                    email,
                    password,
                    rememberMe,
                    lockoutOnFailure: false
                );

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) &&
                    Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction(
                    "Index",
                    "Home"
                );
            }

            ModelState.AddModelError(
                "",
                "Correo o contraseña incorrectos."
            );

            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        // ==========================================
        // CERRAR SESIÓN
        // ==========================================
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(
                "Index",
                "Home"
            );
        }

        // ==========================================
        // ACCESO DENEGADO
        // ==========================================
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}