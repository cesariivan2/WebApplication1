using Microsoft.AspNetCore.Authorization;
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
        // VER LISTA DE TALLERES
        // CUALQUIERA PUEDE VERLOS
        // ==========================================
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var talleres = await _context.Talleres
                .OrderBy(t => t.nombre)
                .ToListAsync();

            return View(talleres);
        }

        // ==========================================
        // VER DETALLES
        // CUALQUIERA PUEDE VERLOS
        // ==========================================
        [AllowAnonymous]
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
        // ABRIR CREAR TALLER
        // ADMINISTRADOR O COMITÉ
        // ==========================================
        [Authorize(Roles = "Administrador,Comite")]
        public IActionResult Create()
        {
            return View();
        }

        // ==========================================
        // GUARDAR NUEVO TALLER
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "Administrador,Comite")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Taller taller)
        {
            if (string.IsNullOrWhiteSpace(taller.nombre))
            {
                ModelState.AddModelError(
                    "nombre",
                    "El nombre del taller es obligatorio."
                );
            }

            if (string.IsNullOrWhiteSpace(taller.descripcion))
            {
                ModelState.AddModelError(
                    "descripcion",
                    "La descripción del taller es obligatoria."
                );
            }

            if (string.IsNullOrWhiteSpace(taller.instructor))
            {
                ModelState.AddModelError(
                    "instructor",
                    "El instructor es obligatorio."
                );
            }

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
        // ABRIR EDITAR
        // ADMINISTRADOR O COMITÉ
        // ==========================================
        [Authorize(Roles = "Administrador,Comite")]
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
        [Authorize(Roles = "Administrador,Comite")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Taller taller)
        {
            if (id != taller.id)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(taller.nombre))
            {
                ModelState.AddModelError(
                    "nombre",
                    "El nombre del taller es obligatorio."
                );
            }

            if (string.IsNullOrWhiteSpace(taller.descripcion))
            {
                ModelState.AddModelError(
                    "descripcion",
                    "La descripción del taller es obligatoria."
                );
            }

            if (string.IsNullOrWhiteSpace(taller.instructor))
            {
                ModelState.AddModelError(
                    "instructor",
                    "El instructor es obligatorio."
                );
            }

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
        // ABRIR ELIMINAR
        // SOLO ADMINISTRADOR
        // ==========================================
        [Authorize(Roles = "Administrador")]
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
        // SOLO ADMINISTRADOR
        // ==========================================
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
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