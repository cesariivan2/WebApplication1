using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UsuariosController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // ==========================================
        // LISTA DE USUARIOS
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var usuarios =
                await _userManager.Users
                    .OrderBy(u => u.Email)
                    .ToListAsync();

            var resultado =
                new List<UsuarioListaViewModel>();

            foreach (var usuario in usuarios)
            {
                var roles =
                    await _userManager.GetRolesAsync(usuario);

                resultado.Add(
                    new UsuarioListaViewModel
                    {
                        Id = usuario.Id,

                        Email =
                            usuario.Email
                            ?? usuario.UserName
                            ?? "Sin correo",

                        Roles =
                            roles.Any()
                                ? string.Join(", ", roles)
                                : "Sin rol"
                    }
                );
            }

            return View(resultado);
        }


        // ==========================================
        // ABRIR CREAR USUARIO
        // ==========================================
        [HttpGet]
        public IActionResult Create()
        {
            return View(
                new CrearUsuarioViewModel()
            );
        }


        // ==========================================
        // CREAR USUARIO
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CrearUsuarioViewModel model)
        {
            // Validar rol
            string[] rolesPermitidos =
            {
                "Administrador",
                "Alumno",
                "Tallerista",
                "Comite"
            };

            if (!rolesPermitidos.Contains(model.Rol))
            {
                ModelState.AddModelError(
                    "Rol",
                    "El rol seleccionado no es válido."
                );
            }


            // ======================================
            // VALIDACIONES ESPECIALES DE ALUMNO
            // ======================================
            if (model.Rol == "Alumno")
            {
                if (string.IsNullOrWhiteSpace(model.Nombre))
                {
                    ModelState.AddModelError(
                        "Nombre",
                        "El nombre es obligatorio para un alumno."
                    );
                }

                if (string.IsNullOrWhiteSpace(model.Matricula))
                {
                    ModelState.AddModelError(
                        "Matricula",
                        "La matrícula es obligatoria."
                    );
                }

                if (model.Turno != "Matutino" &&
                    model.Turno != "Vespertino")
                {
                    ModelState.AddModelError(
                        "Turno",
                        "Selecciona Matutino o Vespertino."
                    );
                }

                if (!string.IsNullOrWhiteSpace(model.Matricula))
                {
                    bool matriculaExiste =
                        await _context.Alumnos
                            .AnyAsync(a =>
                                a.Matricula ==
                                model.Matricula
                            );

                    if (matriculaExiste)
                    {
                        ModelState.AddModelError(
                            "Matricula",
                            "Ya existe un alumno con esa matrícula."
                        );
                    }
                }

                bool correoAlumnoExiste =
                    await _context.Alumnos
                        .AnyAsync(a =>
                            a.Correo == model.Email
                        );

                if (correoAlumnoExiste)
                {
                    ModelState.AddModelError(
                        "Email",
                        "Ya existe un alumno con ese correo."
                    );
                }
            }


            // ======================================
            // VERIFICAR IDENTITY
            // ======================================
            var usuarioExistente =
                await _userManager
                    .FindByEmailAsync(model.Email);

            if (usuarioExistente != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "Ya existe una cuenta con ese correo."
                );
            }


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // ======================================
            // CREAR CUENTA IDENTITY
            // ======================================
            var usuario =
                new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true
                };


            var resultado =
                await _userManager.CreateAsync(
                    usuario,
                    model.Password
                );


            if (!resultado.Succeeded)
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description
                    );
                }

                return View(model);
            }


            // ======================================
            // ASIGNAR ROL
            // ======================================
            var resultadoRol =
                await _userManager.AddToRoleAsync(
                    usuario,
                    model.Rol
                );


            if (!resultadoRol.Succeeded)
            {
                await _userManager.DeleteAsync(usuario);

                ModelState.AddModelError(
                    "",
                    "No fue posible asignar el rol."
                );

                return View(model);
            }


            // ======================================
            // SI ES ALUMNO, CREAR PERFIL ALUMNO
            // ======================================
            if (model.Rol == "Alumno")
            {
                try
                {
                    var alumno =
                        new Alumno
                        {
                            Nombre =
                                model.Nombre!.Trim(),

                            Matricula =
                                model.Matricula!.Trim(),

                            Correo =
                                model.Email.Trim(),

                            Turno =
                                model.Turno!,

                            UsuarioId =
                                usuario.Id
                        };


                    _context.Alumnos.Add(alumno);

                    await _context.SaveChangesAsync();
                }
                catch
                {
                    // Si falla Alumno,
                    // eliminamos también la cuenta Identity
                    await _userManager.DeleteAsync(usuario);

                    ModelState.AddModelError(
                        "",
                        "No se pudo crear el perfil del alumno."
                    );

                    return View(model);
                }
            }


            TempData["Exito"] =
                "Usuario creado correctamente.";

            return RedirectToAction(
                nameof(Index)
            );
        }
    }
}