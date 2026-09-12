using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser>
            _signInManager;

        public AccountController(
            SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
        }


        // ==========================================
        // ABRIR LOGIN
        // ==========================================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(
            string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(
                    "Index",
                    "Home"
                );
            }

            ViewBag.ReturnUrl = returnUrl;

            return View(
                new LoginViewModel()
            );
        }


        // ==========================================
        // INICIAR SESIÓN
        // ==========================================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model,
            string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;

                return View(model);
            }

            var result =
                await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.Recordarme,
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

            return View(model);
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
                "Login",
                "Account"
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