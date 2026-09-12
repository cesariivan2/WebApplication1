using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class TorneosVideojuegosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TorneosVideojuegosController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================
        // LISTAR TORNEOS
        // PÚBLICO
        // ==========================================
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var torneos = await _context.TorneosVideojuegos
                .OrderBy(t => t.FechaInicio)
                .ToListAsync();

            return View(torneos);
        }

        // ==========================================
        // VER DETALLES DEL TORNEO
        // PÚBLICO
        // ==========================================
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var torneo = await _context.TorneosVideojuegos
                .FirstOrDefaultAsync(t => t.Id == id);

            if (torneo == null)
            {
                return NotFound();
            }

            var participantes = await _context.ParticipantesTorneo
                .Include(p => p.Alumno)
                .Where(p =>
                    p.TorneoVideojuegoId == torneo.Id &&
                    p.Estado == "Inscrito"
                )
                .OrderBy(p => p.Alumno!.Nombre)
                .ToListAsync();

            var partidas = await _context.PartidasTorneo
                .Where(p => p.TorneoVideojuegoId == torneo.Id)
                .OrderBy(p => p.Ronda)
                .ThenBy(p => p.Id)
                .ToListAsync();

            ViewBag.Participantes = participantes;
            ViewBag.Partidas = partidas;

            return View(torneo);
        }

        // ==========================================
        // CREAR TORNEO
        // SOLO ADMINISTRADOR
        // ==========================================
        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            return View();
        }

        // ==========================================
        // GUARDAR TORNEO
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            TorneoVideojuego torneo)
        {
            if (string.IsNullOrWhiteSpace(torneo.Nombre))
            {
                ModelState.AddModelError(
                    "Nombre",
                    "El nombre del torneo es obligatorio."
                );
            }

            if (string.IsNullOrWhiteSpace(torneo.Videojuego))
            {
                ModelState.AddModelError(
                    "Videojuego",
                    "El videojuego es obligatorio."
                );
            }

            if (torneo.CupoMaximo < 2)
            {
                ModelState.AddModelError(
                    "CupoMaximo",
                    "El torneo debe permitir al menos 2 participantes."
                );
            }

            if (torneo.FechaInicio == default)
            {
                ModelState.AddModelError(
                    "FechaInicio",
                    "Debes indicar la fecha de inicio."
                );
            }

            if (torneo.FechaFin != null &&
                torneo.FechaFin < torneo.FechaInicio)
            {
                ModelState.AddModelError(
                    "FechaFin",
                    "La fecha final no puede ser anterior a la fecha inicial."
                );
            }

            if (ModelState.IsValid)
            {
                torneo.Estado = "Abierto";

                _context.TorneosVideojuegos.Add(torneo);

                await _context.SaveChangesAsync();

                TempData["Exito"] =
                    "Torneo creado correctamente.";

                return RedirectToAction(nameof(Index));
            }

            return View(torneo);
        }

        // ==========================================
        // ABRIR EDICIÓN
        // ==========================================
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var torneo = await _context.TorneosVideojuegos
                .FindAsync(id);

            if (torneo == null)
            {
                return NotFound();
            }

            return View(torneo);
        }

        // ==========================================
        // GUARDAR EDICIÓN
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            TorneoVideojuego torneo)
        {
            if (id != torneo.Id)
            {
                return NotFound();
            }

            var torneoActual = await _context.TorneosVideojuegos
                .FindAsync(id);

            if (torneoActual == null)
            {
                return NotFound();
            }

            if (torneo.CupoMaximo < 2)
            {
                ModelState.AddModelError(
                    "CupoMaximo",
                    "El torneo debe permitir al menos 2 participantes."
                );
            }

            if (torneo.FechaFin != null &&
                torneo.FechaFin < torneo.FechaInicio)
            {
                ModelState.AddModelError(
                    "FechaFin",
                    "La fecha final no puede ser anterior a la fecha inicial."
                );
            }

            int inscritos = await _context.ParticipantesTorneo
                .CountAsync(p =>
                    p.TorneoVideojuegoId == id &&
                    p.Estado == "Inscrito"
                );

            if (torneo.CupoMaximo < inscritos)
            {
                ModelState.AddModelError(
                    "CupoMaximo",
                    "El cupo máximo no puede ser menor que la cantidad de participantes inscritos."
                );
            }

            if (!ModelState.IsValid)
            {
                return View(torneo);
            }

            torneoActual.Nombre = torneo.Nombre;
            torneoActual.Videojuego = torneo.Videojuego;
            torneoActual.Plataforma = torneo.Plataforma;
            torneoActual.Formato = torneo.Formato;
            torneoActual.Lugar = torneo.Lugar;
            torneoActual.CupoMaximo = torneo.CupoMaximo;
            torneoActual.FechaInicio = torneo.FechaInicio;
            torneoActual.FechaFin = torneo.FechaFin;

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "Torneo actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // INSCRIBIR ALUMNO AL TORNEO
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "Alumno")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscribirse(int id)
        {
            var torneo = await _context.TorneosVideojuegos
                .FindAsync(id);

            if (torneo == null)
            {
                return NotFound();
            }

            if (torneo.Estado != "Abierto")
            {
                TempData["Error"] =
                    "Las inscripciones de este torneo están cerradas.";

                return RedirectToAction(
                    nameof(Details),
                    new { id }
                );
            }

            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            var alumno = await _context.Alumnos
                .FirstOrDefaultAsync(a =>
                    a.UsuarioId == usuario.Id
                );

            if (alumno == null)
            {
                TempData["Error"] =
                    "Tu cuenta no está relacionada con un alumno.";

                return RedirectToAction(
                    nameof(Details),
                    new { id }
                );
            }

            var participanteExistente =
                await _context.ParticipantesTorneo
                    .FirstOrDefaultAsync(p =>
                        p.TorneoVideojuegoId == id &&
                        p.AlumnoId == alumno.Id
                    );

            if (participanteExistente != null)
            {
                if (participanteExistente.Estado == "Cancelado")
                {
                    int inscritosActuales =
                        await _context.ParticipantesTorneo
                            .CountAsync(p =>
                                p.TorneoVideojuegoId == id &&
                                p.Estado == "Inscrito"
                            );

                    if (inscritosActuales >= torneo.CupoMaximo)
                    {
                        TempData["Error"] =
                            "El torneo ya alcanzó su cupo máximo.";

                        return RedirectToAction(
                            nameof(Details),
                            new { id }
                        );
                    }

                    participanteExistente.Estado = "Inscrito";
                    participanteExistente.FechaInscripcion =
                        DateTime.Now;

                    await _context.SaveChangesAsync();

                    TempData["Exito"] =
                        "Te has vuelto a inscribir al torneo.";

                    return RedirectToAction(
                        nameof(Details),
                        new { id }
                    );
                }

                TempData["Error"] =
                    "Ya estás inscrito en este torneo.";

                return RedirectToAction(
                    nameof(Details),
                    new { id }
                );
            }

            int participantes = await _context.ParticipantesTorneo
                .CountAsync(p =>
                    p.TorneoVideojuegoId == id &&
                    p.Estado == "Inscrito"
                );

            if (participantes >= torneo.CupoMaximo)
            {
                TempData["Error"] =
                    "El torneo ya alcanzó su cupo máximo.";

                return RedirectToAction(
                    nameof(Details),
                    new { id }
                );
            }

            var participante = new ParticipanteTorneo
            {
                TorneoVideojuegoId = id,
                AlumnoId = alumno.Id,
                FechaInscripcion = DateTime.Now,
                Estado = "Inscrito"
            };

            _context.ParticipantesTorneo.Add(participante);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Ya existe una inscripción para este alumno.";

                return RedirectToAction(
                    nameof(Details),
                    new { id }
                );
            }

            TempData["Exito"] =
                "Inscripción al torneo realizada correctamente.";

            return RedirectToAction(
                nameof(Details),
                new { id }
            );
        }

        // ==========================================
        // CANCELAR MI INSCRIPCIÓN
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "Alumno")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarInscripcion(int id)
        {
            var torneo = await _context.TorneosVideojuegos
                .FindAsync(id);

            if (torneo == null)
            {
                return NotFound();
            }

            if (torneo.Estado != "Abierto")
            {
                TempData["Error"] =
                    "Ya no puedes cancelar tu inscripción porque el torneo comenzó o cerró inscripciones.";

                return RedirectToAction(
                    nameof(Details),
                    new { id }
                );
            }

            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            var alumno = await _context.Alumnos
                .FirstOrDefaultAsync(a =>
                    a.UsuarioId == usuario.Id
                );

            if (alumno == null)
            {
                return Forbid();
            }

            var participante = await _context.ParticipantesTorneo
                .FirstOrDefaultAsync(p =>
                    p.TorneoVideojuegoId == id &&
                    p.AlumnoId == alumno.Id &&
                    p.Estado == "Inscrito"
                );

            if (participante == null)
            {
                TempData["Error"] =
                    "No tienes una inscripción activa en este torneo.";

                return RedirectToAction(
                    nameof(Details),
                    new { id }
                );
            }

            participante.Estado = "Cancelado";

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "Tu inscripción al torneo fue cancelada.";

            return RedirectToAction(
                nameof(Details),
                new { id }
            );
        }

        // ==========================================
        // CERRAR INSCRIPCIONES
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CerrarInscripciones(int id)
        {
            var torneo = await _context.TorneosVideojuegos
                .FindAsync(id);

            if (torneo == null)
            {
                return NotFound();
            }

            if (torneo.Estado != "Abierto")
            {
                TempData["Error"] =
                    "El torneo ya no está abierto.";

                return RedirectToAction(
                    nameof(Details),
                    new { id }
                );
            }

            torneo.Estado = "Cerrado";

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "Las inscripciones del torneo fueron cerradas.";

            return RedirectToAction(
                nameof(Details),
                new { id }
            );
        }
    }
}