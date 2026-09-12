using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class PropuestasComiteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PropuestasComiteController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================
        // LISTAR PROPUESTAS
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var propuestas = await _context.PropuestasComite
                .Include(p => p.CreadoPorUsuario)
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync();

            return View(propuestas);
        }

        // ==========================================
        // VER DETALLES Y RESULTADOS
        // ==========================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propuesta = await _context.PropuestasComite
                .Include(p => p.CreadoPorUsuario)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (propuesta == null)
            {
                return NotFound();
            }

            ViewBag.VotosAFavor = await _context.VotosPropuesta
                .CountAsync(v =>
                    v.PropuestaComiteId == propuesta.Id &&
                    v.Opcion == "A favor"
                );

            ViewBag.VotosEnContra = await _context.VotosPropuesta
                .CountAsync(v =>
                    v.PropuestaComiteId == propuesta.Id &&
                    v.Opcion == "En contra"
                );

            ViewBag.Abstenciones = await _context.VotosPropuesta
                .CountAsync(v =>
                    v.PropuestaComiteId == propuesta.Id &&
                    v.Opcion == "Abstencion"
                );

            return View(propuesta);
        }

        // ==========================================
        // ABRIR CREAR PROPUESTA
        // SOLO COMITÉ O ADMIN
        // ==========================================
        [Authorize(Roles = "Administrador,Comite")]
        public IActionResult Create()
        {
            return View();
        }

        // ==========================================
        // GUARDAR PROPUESTA
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "Administrador,Comite")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PropuestaComite propuesta)
        {
            if (!ModelState.IsValid)
            {
                return View(propuesta);
            }

            var usuario = await _userManager
                .GetUserAsync(User);

            propuesta.FechaCreacion = DateTime.Now;
            propuesta.Estado = "Abierta";
            propuesta.FechaCierre = null;
            propuesta.CreadoPorUsuarioId = usuario?.Id;

            _context.PropuestasComite.Add(propuesta);

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "Propuesta creada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // ABRIR EDITAR
        // ==========================================
        [Authorize(Roles = "Administrador,Comite")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propuesta = await _context.PropuestasComite
                .FindAsync(id);

            if (propuesta == null)
            {
                return NotFound();
            }

            if (propuesta.Estado == "Cerrada")
            {
                TempData["Error"] =
                    "No se puede editar una propuesta cerrada.";

                return RedirectToAction(nameof(Index));
            }

            return View(propuesta);
        }

        // ==========================================
        // GUARDAR EDICIÓN
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "Administrador,Comite")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            PropuestaComite propuesta)
        {
            if (id != propuesta.Id)
            {
                return NotFound();
            }

            var propuestaActual = await _context.PropuestasComite
                .FindAsync(id);

            if (propuestaActual == null)
            {
                return NotFound();
            }

            if (propuestaActual.Estado == "Cerrada")
            {
                TempData["Error"] =
                    "No se puede editar una propuesta cerrada.";

                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(propuesta);
            }

            propuestaActual.Titulo = propuesta.Titulo;
            propuestaActual.Descripcion = propuesta.Descripcion;

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "Propuesta actualizada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // CERRAR PROPUESTA
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "Administrador,Comite")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cerrar(int id)
        {
            var propuesta = await _context.PropuestasComite
                .FindAsync(id);

            if (propuesta == null)
            {
                return NotFound();
            }

            if (propuesta.Estado == "Cerrada")
            {
                TempData["Error"] =
                    "La propuesta ya está cerrada.";

                return RedirectToAction(nameof(Index));
            }

            propuesta.Estado = "Cerrada";
            propuesta.FechaCierre = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "Propuesta cerrada correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}