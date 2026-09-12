using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class PartidasTorneoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PartidasTorneoController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // VER PARTIDAS DE UN TORNEO
        // PÚBLICO
        // ==========================================
        [AllowAnonymous]
        public async Task<IActionResult> Index(int torneoId)
        {
            var torneo = await _context.TorneosVideojuegos
                .FindAsync(torneoId);

            if (torneo == null)
            {
                return NotFound();
            }

            var partidas = await _context.PartidasTorneo
                .Include(p => p.Participante1)
                    .ThenInclude(p => p!.Alumno)
                .Include(p => p.Participante2)
                    .ThenInclude(p => p!.Alumno)
                .Include(p => p.Ganador)
                    .ThenInclude(p => p!.Alumno)
                .Where(p =>
                    p.TorneoVideojuegoId == torneoId)
                .OrderBy(p => p.Ronda)
                .ThenBy(p => p.Id)
                .ToListAsync();

            ViewBag.Torneo = torneo;

            return View(partidas);
        }

        // ==========================================
        // VER UNA PARTIDA
        // PÚBLICO
        // ==========================================
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var partida = await _context.PartidasTorneo
                .Include(p => p.TorneoVideojuego)
                .Include(p => p.Participante1)
                    .ThenInclude(p => p!.Alumno)
                .Include(p => p.Participante2)
                    .ThenInclude(p => p!.Alumno)
                .Include(p => p.Ganador)
                    .ThenInclude(p => p!.Alumno)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (partida == null)
            {
                return NotFound();
            }

            return View(partida);
        }

        // ==========================================
        // GENERAR PRIMERA RONDA
        // SOLO ADMINISTRADOR
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerarPrimeraRonda(
            int torneoId)
        {
            var torneo = await _context.TorneosVideojuegos
                .FindAsync(torneoId);

            if (torneo == null)
            {
                return NotFound();
            }

            // Evitar generar dos veces
            bool yaTienePartidas =
                await _context.PartidasTorneo
                    .AnyAsync(p =>
                        p.TorneoVideojuegoId == torneoId);

            if (yaTienePartidas)
            {
                TempData["Error"] =
                    "Este torneo ya tiene partidas generadas.";

                return RedirectToAction(
                    nameof(Index),
                    new { torneoId }
                );
            }

            var participantes =
                await _context.ParticipantesTorneo
                    .Where(p =>
                        p.TorneoVideojuegoId == torneoId &&
                        p.Estado == "Inscrito")
                    .OrderBy(p => p.Id)
                    .ToListAsync();

            if (participantes.Count < 2)
            {
                TempData["Error"] =
                    "Se necesitan al menos 2 participantes para generar el torneo.";

                return RedirectToAction(
                    nameof(Index),
                    new { torneoId }
                );
            }

            // Cerrar nuevas inscripciones
            torneo.Estado = "En curso";

            // Emparejar de 2 en 2
            for (int i = 0; i < participantes.Count; i += 2)
            {
                var participante1 = participantes[i];

                // Si quedó uno sin rival, pasa automáticamente
                if (i + 1 >= participantes.Count)
                {
                    var partidaLibre = new PartidaTorneo
                    {
                        TorneoVideojuegoId = torneoId,
                        Participante1Id = participante1.Id,
                        Participante2Id = null,
                        GanadorId = participante1.Id,
                        Ronda = 1,
                        Puntaje1 = null,
                        Puntaje2 = null,
                        Estado = "Finalizada"
                    };

                    _context.PartidasTorneo.Add(
                        partidaLibre
                    );

                    continue;
                }

                var participante2 =
                    participantes[i + 1];

                var partida = new PartidaTorneo
                {
                    TorneoVideojuegoId = torneoId,
                    Participante1Id = participante1.Id,
                    Participante2Id = participante2.Id,
                    GanadorId = null,
                    Ronda = 1,
                    Puntaje1 = null,
                    Puntaje2 = null,
                    Estado = "Pendiente"
                };

                _context.PartidasTorneo.Add(partida);
            }

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "La primera ronda fue generada correctamente.";

            return RedirectToAction(
                nameof(Index),
                new { torneoId }
            );
        }

        // ==========================================
        // PROGRAMAR FECHA Y HORA DE UNA PARTIDA
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Programar(
            int id,
            DateTime fechaHora)
        {
            var partida = await _context.PartidasTorneo
                .FindAsync(id);

            if (partida == null)
            {
                return NotFound();
            }

            if (partida.Estado == "Finalizada")
            {
                TempData["Error"] =
                    "No puedes reprogramar una partida finalizada.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        torneoId =
                            partida.TorneoVideojuegoId
                    }
                );
            }

            partida.FechaHora = fechaHora;
            partida.Estado = "Programada";

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "Partida programada correctamente.";

            return RedirectToAction(
                nameof(Index),
                new
                {
                    torneoId =
                        partida.TorneoVideojuegoId
                }
            );
        }

        // ==========================================
        // REGISTRAR RESULTADO
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarResultado(
            int id,
            int puntaje1,
            int puntaje2)
        {
            var partida = await _context.PartidasTorneo
                .FindAsync(id);

            if (partida == null)
            {
                return NotFound();
            }

            if (partida.Participante2Id == null)
            {
                TempData["Error"] =
                    "Esta partida corresponde a un pase automático.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        torneoId =
                            partida.TorneoVideojuegoId
                    }
                );
            }

            if (partida.Estado == "Finalizada")
            {
                TempData["Error"] =
                    "Esta partida ya fue finalizada.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        torneoId =
                            partida.TorneoVideojuegoId
                    }
                );
            }

            if (puntaje1 < 0 || puntaje2 < 0)
            {
                TempData["Error"] =
                    "Los puntajes no pueden ser negativos.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        torneoId =
                            partida.TorneoVideojuegoId
                    }
                );
            }

            // En eliminación directa necesitamos ganador
            if (puntaje1 == puntaje2)
            {
                TempData["Error"] =
                    "La partida no puede finalizar empatada.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        torneoId =
                            partida.TorneoVideojuegoId
                    }
                );
            }

            partida.Puntaje1 = puntaje1;
            partida.Puntaje2 = puntaje2;

            if (puntaje1 > puntaje2)
            {
                partida.GanadorId =
                    partida.Participante1Id;
            }
            else
            {
                partida.GanadorId =
                    partida.Participante2Id;
            }

            partida.Estado = "Finalizada";

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "Resultado registrado correctamente.";

            return RedirectToAction(
                nameof(Index),
                new
                {
                    torneoId =
                        partida.TorneoVideojuegoId
                }
            );
        }

        // ==========================================
        // GENERAR SIGUIENTE RONDA
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerarSiguienteRonda(
            int torneoId)
        {
            var torneo = await _context.TorneosVideojuegos
                .FindAsync(torneoId);

            if (torneo == null)
            {
                return NotFound();
            }

            var partidas = await _context.PartidasTorneo
                .Where(p =>
                    p.TorneoVideojuegoId == torneoId)
                .ToListAsync();

            if (!partidas.Any())
            {
                TempData["Error"] =
                    "Primero debes generar la primera ronda.";

                return RedirectToAction(
                    nameof(Index),
                    new { torneoId }
                );
            }

            int rondaActual =
                partidas.Max(p => p.Ronda);

            var partidasRondaActual = partidas
                .Where(p => p.Ronda == rondaActual)
                .ToList();

            // Todas deben estar terminadas
            bool pendientes = partidasRondaActual
                .Any(p =>
                    p.Estado != "Finalizada" ||
                    p.GanadorId == null
                );

            if (pendientes)
            {
                TempData["Error"] =
                    "Todas las partidas de la ronda actual deben finalizar antes de generar la siguiente.";

                return RedirectToAction(
                    nameof(Index),
                    new { torneoId }
                );
            }

            // Verificar que no exista ya la siguiente ronda
            bool siguienteYaExiste = partidas
                .Any(p =>
                    p.Ronda == rondaActual + 1);

            if (siguienteYaExiste)
            {
                TempData["Error"] =
                    "La siguiente ronda ya fue generada.";

                return RedirectToAction(
                    nameof(Index),
                    new { torneoId }
                );
            }

            var ganadores = partidasRondaActual
                .Where(p => p.GanadorId.HasValue)
                .Select(p => p.GanadorId!.Value)
                .ToList();

            // Si queda un único ganador, es campeón
            if (ganadores.Count == 1)
            {
                torneo.Estado = "Finalizado";
                torneo.FechaFin = DateTime.Now;

                await _context.SaveChangesAsync();

                TempData["Exito"] =
                    "El torneo ha finalizado. Ya existe un campeón.";

                return RedirectToAction(
                    nameof(Index),
                    new { torneoId }
                );
            }

            int siguienteRonda =
                rondaActual + 1;

            for (int i = 0; i < ganadores.Count; i += 2)
            {
                int participante1Id =
                    ganadores[i];

                // Número impar de ganadores:
                // pase automático
                if (i + 1 >= ganadores.Count)
                {
                    var paseAutomatico =
                        new PartidaTorneo
                        {
                            TorneoVideojuegoId =
                                torneoId,

                            Participante1Id =
                                participante1Id,

                            Participante2Id =
                                null,

                            GanadorId =
                                participante1Id,

                            Ronda =
                                siguienteRonda,

                            Estado =
                                "Finalizada"
                        };

                    _context.PartidasTorneo.Add(
                        paseAutomatico
                    );

                    continue;
                }

                int participante2Id =
                    ganadores[i + 1];

                var nuevaPartida =
                    new PartidaTorneo
                    {
                        TorneoVideojuegoId =
                            torneoId,

                        Participante1Id =
                            participante1Id,

                        Participante2Id =
                            participante2Id,

                        GanadorId =
                            null,

                        Ronda =
                            siguienteRonda,

                        Estado =
                            "Pendiente"
                    };

                _context.PartidasTorneo.Add(
                    nuevaPartida
                );
            }

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                $"Ronda {siguienteRonda} generada correctamente.";

            return RedirectToAction(
                nameof(Index),
                new { torneoId }
            );
        }

        // ==========================================
        // OBTENER CAMPEÓN DEL TORNEO
        // ==========================================
        [AllowAnonymous]
        public async Task<IActionResult> Campeon(
            int torneoId)
        {
            var torneo = await _context.TorneosVideojuegos
                .FindAsync(torneoId);

            if (torneo == null)
            {
                return NotFound();
            }

            if (torneo.Estado != "Finalizado")
            {
                TempData["Error"] =
                    "El torneo todavía no ha finalizado.";

                return RedirectToAction(
                    nameof(Index),
                    new { torneoId }
                );
            }

            var ultimaPartida =
                await _context.PartidasTorneo
                    .Include(p => p.Ganador)
                        .ThenInclude(p => p!.Alumno)
                    .Where(p =>
                        p.TorneoVideojuegoId == torneoId &&
                        p.GanadorId != null)
                    .OrderByDescending(p => p.Ronda)
                    .ThenByDescending(p => p.Id)
                    .FirstOrDefaultAsync();

            if (ultimaPartida?.Ganador == null)
            {
                return NotFound();
            }

            ViewBag.Torneo = torneo;

            return View(ultimaPartida.Ganador);
        }
    }
}