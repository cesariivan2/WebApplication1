using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;


namespace WebApplication1.Controllers
{
    public class AlumnosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AlumnosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // LISTAR
        public async Task<IActionResult> Index()
        {
            var alumnos = await _context.Alumnos.ToListAsync();
            return View(alumnos);
        }

        // ABRIR FORMULARIO CREAR
        public IActionResult Create()
        {
            return View();
        }

        // GUARDAR NUEVO ALUMNO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Alumno alumno)
        {
            bool matriculaExiste = await _context.Alumnos
                .AnyAsync(a => a.Matricula == alumno.Matricula);

            if (matriculaExiste)
            {
                ModelState.AddModelError(
                    "Matricula",
                    "Ya existe un alumno con esta matrícula."
                );
            }

            if (ModelState.IsValid)
            {
                _context.Alumnos.Add(alumno);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(alumno);
        }

        // VER DETALLES
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

        // ABRIR FORMULARIO EDITAR
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alumno = await _context.Alumnos.FindAsync(id);

            if (alumno == null)
            {
                return NotFound();
            }

            return View(alumno);
        }

        // GUARDAR EDICIÓN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Alumno alumno)
        {
            if (id != alumno.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(alumno);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlumnoExists(alumno.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(alumno);
        }

        // ABRIR CONFIRMACIÓN DE BORRADO
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

        // BORRAR REALMENTE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alumno = await _context.Alumnos.FindAsync(id);

            if (alumno != null)
            {
                _context.Alumnos.Remove(alumno);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AlumnoExists(int id)
        {
            return _context.Alumnos.Any(a => a.Id == id);
        }
    }
}