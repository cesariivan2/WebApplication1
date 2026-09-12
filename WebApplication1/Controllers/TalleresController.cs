using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class TalleresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TalleresController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // LISTAR TALLERES
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var talleres = await _context.Talleres
                .OrderBy(t => t.nombre)
                .ToListAsync();

            return View(talleres);
        }

        // ==========================================
        // ABRIR FORMULARIO PARA CREAR
        // ==========================================
        public IActionResult Create()
        {
            return View();
        }

        // ==========================================
        // GUARDAR NUEVO TALLER
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Taller taller)
        {
            // Validar nombre
            if (string.IsNullOrWhiteSpace(taller.nombre))
            {
                ModelState.AddModelError(
                    "nombre",
                    "El nombre del taller es obligatorio."
                );
            }

            // Validar descripción
            if (string.IsNullOrWhiteSpace(taller.descripcion))
            {
                ModelState.AddModelError(
                    "descripcion",
                    "La descripción del taller es obligatoria."
                );
            }

            // Validar instructor
            if (string.IsNullOrWhiteSpace(taller.instructor))
            {
                ModelState.AddModelError(
                    "instructor",
                    "El nombre del instructor es obligatorio."
                );
            }

            // Evitar talleres con exactamente el mismo nombre
            bool nombreExiste = await _context.Talleres
                .AnyAsync(t =>
                    t.nombre.ToLower() ==
                    taller.nombre.ToLower()
                );

            if (nombreExiste)
            {
                ModelState.AddModelError(
                    "nombre",
                    "Ya existe un taller con este nombre."
                );
            }

            if (ModelState.IsValid)
            {
                _context.Talleres.Add(taller);

                await _context.SaveChangesAsync();

                TempData["Exito"] =
                    "Taller registrado correctamente.";

                return RedirectToAction(nameof(Index));
            }

            return View(taller);
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

            var taller = await _context.Talleres
                .FirstOrDefaultAsync(t => t.id == id);

            if (taller == null)
            {
                return NotFound();
            }

            return View(taller);
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

            var taller = await _context.Talleres
                .FindAsync(id);

            if (taller == null)
            {
                return NotFound();
            }

            return View(taller);
        }

        // ==========================================
        // GUARDAR EDICIÓN
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Taller taller)
        {
            if (id != taller.id)
            {
                return NotFound();
            }

            // Validar nombre
            if (string.IsNullOrWhiteSpace(taller.nombre))
            {
                ModelState.AddModelError(
                    "nombre",
                    "El nombre del taller es obligatorio."
                );
            }

            // Validar descripción
            if (string.IsNullOrWhiteSpace(taller.descripcion))
            {
                ModelState.AddModelError(
                    "descripcion",
                    "La descripción del taller es obligatoria."
                );
            }

            // Validar instructor
            if (string.IsNullOrWhiteSpace(taller.instructor))
            {
                ModelState.AddModelError(
                    "instructor",
                    "El nombre del instructor es obligatorio."
                );
            }

            // Evitar nombre duplicado en otro taller
            bool nombreExiste = await _context.Talleres
                .AnyAsync(t =>
                    t.id != taller.id &&
                    t.nombre.ToLower() ==
                    taller.nombre.ToLower()
                );

            if (nombreExiste)
            {
                ModelState.AddModelError(
                    "nombre",
                    "Ya existe otro taller con este nombre."
                );
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Talleres.Update(taller);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    bool existe = await _context.Talleres
                        .AnyAsync(t => t.id == taller.id);

                    if (!existe)
                    {
                        return NotFound();
                    }

                    throw;
                }

                TempData["Exito"] =
                    "Taller actualizado correctamente.";

                return RedirectToAction(nameof(Index));
            }

            return View(taller);
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

            var taller = await _context.Talleres
                .FirstOrDefaultAsync(t => t.id == id);

            if (taller == null)
            {
                return NotFound();
            }

            return View(taller);
        }

        // ==========================================
        // ELIMINAR TALLER
        // ==========================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // No permitir eliminar un taller
            // que ya tenga horarios
            bool tieneHorarios = await _context.HorariosTaller
                .AnyAsync(h => h.tallerId == id);

            if (tieneHorarios)
            {
                TempData["Error"] =
                    "No se puede eliminar el taller porque tiene horarios asignados.";

                return RedirectToAction(nameof(Index));
            }

            var taller = await _context.Talleres
                .FindAsync(id);

            if (taller == null)
            {
                return NotFound();
            }

            _context.Talleres.Remove(taller);

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "Taller eliminado correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}