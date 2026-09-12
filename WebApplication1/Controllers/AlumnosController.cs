using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Data;
using WebApplication1.Models;
namespace WebApplication1.Controllers

{
    public class AlumnosController : Controller
    {


        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        public AlumnosController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================
        // LISTAR ALUMNOS
        // ==========================================

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index()
        {
            var alumnos = await _context.Alumnos
                .OrderBy(a => a.Nombre)
                .ToListAsync();

            return View(alumnos);
        }

        // ==========================================
        // ABRIR FORMULARIO PARA CREAR
        // ==========================================
        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            return View(new AlumnoCreateViewModel());
        }

        // ==========================================
        // GUARDAR NUEVO ALUMNO
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create(
     AlumnoCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Revisar matrícula repetida
            var matriculaExiste = await _context.Alumnos
                .AnyAsync(a => a.Matricula == model.Matricula);

            if (matriculaExiste)
            {
                ModelState.AddModelError(
                    "Matricula",
                    "Ya existe un alumno con esta matrícula."
                );

                return View(model);
            }

            // Revisar correo repetido en Identity
            var usuarioExistente =
                await _userManager.FindByEmailAsync(model.Correo);

            if (usuarioExistente != null)
            {
                ModelState.AddModelError(
                    "Correo",
                    "Ya existe una cuenta con este correo."
                );

                return View(model);
            }

            // Crear cuenta de acceso
            var usuario = new IdentityUser
            {
                UserName = model.Correo,
                Email = model.Correo,
                EmailConfirmed = true
            };

            var resultadoUsuario =
                await _userManager.CreateAsync(
                    usuario,
                    model.Password
                );

            if (!resultadoUsuario.Succeeded)
            {
                foreach (var error in resultadoUsuario.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description
                    );
                }

                return View(model);
            }

            // Asignarle el rol Alumno
            var resultadoRol =
                await _userManager.AddToRoleAsync(
                    usuario,
                    "Alumno"
                );

            if (!resultadoRol.Succeeded)
            {
                await _userManager.DeleteAsync(usuario);

                foreach (var error in resultadoRol.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description
                    );
                }

                return View(model);
            }

            // Crear el registro Alumno
            var alumno = new Alumno
            {
                Nombre = model.Nombre,
                Matricula = model.Matricula,
                Correo = model.Correo,
                Turno = model.Turno,

                // Une Alumno con AspNetUsers
                UsuarioId = usuario.Id
            };

            _context.Alumnos.Add(alumno);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Evita dejar una cuenta huérfana
                await _userManager.DeleteAsync(usuario);
                throw;
            }

            TempData["Exito"] =
                "Alumno y cuenta creados correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // VER DETALLES
        // ==========================================
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumno = await _context.Alumnos
                .FirstOrDefaultAsync(a => a.Id == id);

            if (alumno == null)
            {
                return NotFound();
            }

            return View(alumno);
        }

        // ==========================================
        // ABRIR FORMULARIO PARA EDITAR
        // ==========================================
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumno = await _context.Alumnos
                .FindAsync(id);

            if (alumno == null)
            {
                return NotFound();
            }

            return View(alumno);
        }

        // ==========================================
        // GUARDAR EDICIÓN
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(
            int id,
            Alumno alumno)
        {
            if (id != alumno.Id)
            {
                return NotFound();
            }

            // Evitar que otro alumno tenga
            // la misma matrícula
            bool matriculaExiste = await _context.Alumnos
                .AnyAsync(a =>
                    a.Matricula == alumno.Matricula &&
                    a.Id != alumno.Id
                );

            if (matriculaExiste)
            {
                ModelState.AddModelError(
                    "Matricula",
                    "Ya existe otro alumno con esta matrícula."
                );
            }

            // Validar turno
            if (alumno.Turno != "Matutino" &&
                alumno.Turno != "Vespertino")
            {
                ModelState.AddModelError(
                    "Turno",
                    "El turno debe ser Matutino o Vespertino."
                );
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Alumnos.Update(alumno);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    bool existe = await _context.Alumnos
                        .AnyAsync(a => a.Id == alumno.Id);

                    if (!existe)
                    {
                        return NotFound();
                    }

                    throw;
                }

                TempData["Exito"] =
                    "Alumno actualizado correctamente.";

                return RedirectToAction(nameof(Index));
            }

            return View(alumno);
        }

        // ==========================================
        // ABRIR CONFIRMACIÓN PARA ELIMINAR
        // ==========================================
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumno = await _context.Alumnos
                .FirstOrDefaultAsync(a => a.Id == id);

            if (alumno == null)
            {
                return NotFound();
            }

            return View(alumno);
        }

        // ==========================================
        // ELIMINAR ALUMNO
        // ==========================================
        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Evitar borrar alumno con inscripciones
            bool tieneInscripciones = await _context.Inscripciones
                .AnyAsync(i => i.alumnoId == id);

            if (tieneInscripciones)
            {
                TempData["Error"] =
                    "No se puede eliminar al alumno porque tiene inscripciones registradas.";

                return RedirectToAction(nameof(Index));
            }

            var alumno = await _context.Alumnos
                .FindAsync(id);

            if (alumno == null)
            {
                return NotFound();
            }

            _context.Alumnos.Remove(alumno);

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "Alumno eliminado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Alumno")]
        public async Task<IActionResult> MiGafete()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(usuarioId))
            {
                return Unauthorized();
            }

            var alumno = await _context.Alumnos
                .FirstOrDefaultAsync(a => a.UsuarioId == usuarioId);

            if (alumno == null)
            {
                TempData["Error"] =
                    "Tu usuario todavía no está relacionado con un registro de alumno.";

                return RedirectToAction("Index", "Home");
            }

            return View(alumno);
        }

    }
}