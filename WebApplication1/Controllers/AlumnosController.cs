using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication1.Controllers
{
    public class AlumnosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AlumnosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // LISTAR ALUMNOS
        // ==========================================
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
        public IActionResult Create()
        {
            return View();
        }

        // ==========================================
        // GUARDAR NUEVO ALUMNO
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Alumno alumno)
        {
            // Evitar matrículas repetidas
            bool matriculaExiste = await _context.Alumnos
                .AnyAsync(a => a.Matricula == alumno.Matricula);

            if (matriculaExiste)
            {
                ModelState.AddModelError(
                    "Matricula",
                    "Ya existe un alumno con esta matrícula."
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
                _context.Alumnos.Add(alumno);

                await _context.SaveChangesAsync();

                TempData["Exito"] =
                    "Alumno registrado correctamente.";

                return RedirectToAction(nameof(Index));
            }

            return View(alumno);
        }

        // ==========================================
        // VER DETALLES
        // ==========================================
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
    }
}