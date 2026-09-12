using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class VotacionesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public VotacionesController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================
        // MOSTRAR PROPUESTAS ABIERTAS PARA VOTAR
        // ==========================================
        [Authorize(Roles = "Alumno")]
        public async Task<IActionResult> Index()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            var propuestas = await _context.PropuestasComite
                .Where(p => p.Estado == "Abierta")
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync();

            var votosUsuario = await _context.VotosPropuesta
                .Where(v => v.UsuarioId == usuario.Id)
                .Select(v => v.PropuestaComiteId)
                .ToListAsync();

            ViewBag.VotosUsuario = votosUsuario;

            return View(propuestas);
        }

        // ==========================================
        // ABRIR PANTALLA PARA VOTAR
        // ==========================================
        [Authorize(Roles = "Alumno")]
        public async Task<IActionResult> Votar(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            var propuesta = await _context.PropuestasComite
                .FirstOrDefaultAsync(p => p.Id == id);

            if (propuesta == null)
            {
                return NotFound();
            }

            if (propuesta.Estado != "Abierta")
            {
                TempData["Error"] =
                    "Esta propuesta ya está cerrada.";

                return RedirectToAction(nameof(Index));
            }

            bool yaVoto = await _context.VotosPropuesta
                .AnyAsync(v =>
                    v.PropuestaComiteId == id &&
                    v.UsuarioId == usuario.Id
                );

            if (yaVoto)
            {
                TempData["Error"] =
                    "Ya emitiste tu voto en esta propuesta.";

                return RedirectToAction(nameof(Index));
            }

            return View(propuesta);
        }

        // ==========================================
        // GUARDAR VOTO
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "Alumno")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Votar(
            int propuestaId,
            string opcion)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            var propuesta = await _context.PropuestasComite
                .FirstOrDefaultAsync(
                    p => p.Id == propuestaId
                );

            if (propuesta == null)
            {
                return NotFound();
            }

            // No permitir votos en propuestas cerradas
            if (propuesta.Estado != "Abierta")
            {
                TempData["Error"] =
                    "No puedes votar porque esta propuesta ya está cerrada.";

                return RedirectToAction(nameof(Index));
            }

            // Solo aceptar opciones válidas
            bool opcionValida =
                opcion == "A favor" ||
                opcion == "En contra" ||
                opcion == "Abstencion";

            if (!opcionValida)
            {
                TempData["Error"] =
                    "La opción de voto no es válida.";

                return RedirectToAction(
                    nameof(Votar),
                    new { id = propuestaId }
                );
            }

            // Evitar voto duplicado
            bool yaVoto = await _context.VotosPropuesta
                .AnyAsync(v =>
                    v.PropuestaComiteId == propuestaId &&
                    v.UsuarioId == usuario.Id
                );

            if (yaVoto)
            {
                TempData["Error"] =
                    "Ya emitiste tu voto en esta propuesta.";

                return RedirectToAction(nameof(Index));
            }

            var voto = new VotoPropuesta
            {
                PropuestaComiteId = propuestaId,
                UsuarioId = usuario.Id,
                Opcion = opcion,
                FechaVoto = DateTime.Now
            };

            _context.VotosPropuesta.Add(voto);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // La base también tiene una restricción única.
                // Esto protege contra doble clic o solicitudes repetidas.
                TempData["Error"] =
                    "Tu voto ya había sido registrado.";

                return RedirectToAction(nameof(Index));
            }

            TempData["Exito"] =
                "Tu voto fue registrado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // VER MIS VOTOS
        // ==========================================
        [Authorize(Roles = "Alumno")]
        public async Task<IActionResult> MisVotos()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            var votos = await _context.VotosPropuesta
                .Include(v => v.PropuestaComite)
                .Where(v => v.UsuarioId == usuario.Id)
                .OrderByDescending(v => v.FechaVoto)
                .ToListAsync();

            return View(votos);
        }

        // ==========================================
        // RESULTADOS
        // ADMINISTRADOR Y COMITÉ
        // ==========================================
        [Authorize(Roles = "Administrador,Comite")]
        public async Task<IActionResult> Resultados(int id)
        {
            var propuesta = await _context.PropuestasComite
                .FirstOrDefaultAsync(p => p.Id == id);

            if (propuesta == null)
            {
                return NotFound();
            }

            int aFavor = await _context.VotosPropuesta
                .CountAsync(v =>
                    v.PropuestaComiteId == id &&
                    v.Opcion == "A favor"
                );

            int enContra = await _context.VotosPropuesta
                .CountAsync(v =>
                    v.PropuestaComiteId == id &&
                    v.Opcion == "En contra"
                );

            int abstenciones = await _context.VotosPropuesta
                .CountAsync(v =>
                    v.PropuestaComiteId == id &&
                    v.Opcion == "Abstencion"
                );

            int total = aFavor + enContra + abstenciones;

            ViewBag.AFavor = aFavor;
            ViewBag.EnContra = enContra;
            ViewBag.Abstenciones = abstenciones;
            ViewBag.Total = total;

            return View(propuesta);
        }
    }
}