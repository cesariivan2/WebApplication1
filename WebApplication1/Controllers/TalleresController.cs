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

        // LISTA DE TALLERES
        public async Task<IActionResult> Index()
        {
            var talleres = await _context.Talleres.ToListAsync();

            return View(talleres);
        }

        // ABRIR CREAR
        public IActionResult Create()
        {
            return View();
        }

        // GUARDAR TALLER
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Taller taller)
        {
            if (ModelState.IsValid)
            {
                _context.Talleres.Add(taller);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(taller);
        }

        // DETALLES
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

        // ABRIR EDITAR
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taller = await _context.Talleres.FindAsync(id);

            if (taller == null)
            {
                return NotFound();
            }

            return View(taller);
        }

        // GUARDAR EDICIÓN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Taller taller)
        {
            if (id != taller.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Talleres.Update(taller);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(taller);
        }

        // ABRIR BORRAR
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

        // CONFIRMAR BORRADO
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taller = await _context.Talleres.FindAsync(id);

            if (taller != null)
            {
                _context.Talleres.Remove(taller);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}