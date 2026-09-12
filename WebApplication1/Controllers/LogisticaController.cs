using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Controllers
{
    public class LogisticaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LogisticaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // AGENDA GENERAL DEL EVENTO
        // ==========================================
        public async Task<IActionResult> Agenda()
        {
            var horarios = await _context.HorariosTaller
                .Include(h => h.Taller)
                .OrderBy(h => h.fecha)
                .ThenBy(h => h.horaInicio)
                .ToListAsync();

            return View(horarios);
        }

        // ==========================================
        // VER PARTICIPANTES DE UN HORARIO
        // ==========================================
        public async Task<IActionResult> Participantes(int id)
        {
            var horario = await _context.HorariosTaller
                .Include(h => h.Taller)
                .FirstOrDefaultAsync(h => h.id == id);

            if (horario == null)
            {
                return NotFound();
            }

            var inscripciones = await _context.Inscripciones
                .Include(i => i.alumno)
                .Where(i =>
                    i.horarioTallerId == id &&
                    i.estado == "Confirmada"
                )
                .OrderBy(i => i.alumno!.Nombre)
                .ToListAsync();

            ViewBag.Horario = horario;

            return View(inscripciones);
        }

        // ==========================================
        // CONSULTAR DISPONIBILIDAD DE UN HORARIO
        // ==========================================
        public async Task<IActionResult> Disponibilidad(int id)
        {
            var horario = await _context.HorariosTaller
                .Include(h => h.Taller)
                .FirstOrDefaultAsync(h => h.id == id);

            if (horario == null)
            {
                return NotFound();
            }

            int inscritosMatutinos =
                await _context.Inscripciones
                    .Include(i => i.alumno)
                    .CountAsync(i =>
                        i.horarioTallerId == id &&
                        i.estado == "Confirmada" &&
                        i.alumno != null &&
                        i.alumno.Turno == "Matutino"
                    );

            int inscritosVespertinos =
                await _context.Inscripciones
                    .Include(i => i.alumno)
                    .CountAsync(i =>
                        i.horarioTallerId == id &&
                        i.estado == "Confirmada" &&
                        i.alumno != null &&
                        i.alumno.Turno == "Vespertino"
                    );

            int disponiblesMatutinos =
                horario.CupoMatutino - inscritosMatutinos;

            int disponiblesVespertinos =
                horario.CupoVespertino - inscritosVespertinos;

            if (disponiblesMatutinos < 0)
            {
                disponiblesMatutinos = 0;
            }

            if (disponiblesVespertinos < 0)
            {
                disponiblesVespertinos = 0;
            }

            return Json(new
            {
                horarioId = horario.id,

                taller = horario.Taller != null
                    ? horario.Taller.nombre
                    : "Sin taller",

                fecha = horario.fecha,

                horaInicio = horario.horaInicio.ToString(),

                horaFin = horario.horaFin.ToString(),

                espacio = horario.Espacio,

                matutino = new
                {
                    cupo = horario.CupoMatutino,
                    inscritos = inscritosMatutinos,
                    disponibles = disponiblesMatutinos
                },

                vespertino = new
                {
                    cupo = horario.CupoVespertino,
                    inscritos = inscritosVespertinos,
                    disponibles = disponiblesVespertinos
                }
            });
        }

        // ==========================================
        // VER TODOS LOS ESPACIOS UTILIZADOS
        // ==========================================
        public async Task<IActionResult> Espacios()
        {
            var horarios = await _context.HorariosTaller
                .Include(h => h.Taller)
                .OrderBy(h => h.Espacio)
                .ThenBy(h => h.fecha)
                .ThenBy(h => h.horaInicio)
                .ToListAsync();

            return View(horarios);
        }

        // ==========================================
        // RESUMEN OPERATIVO
        // ==========================================
        public async Task<IActionResult> Resumen()
        {
            ViewBag.TotalEspacios =
                await _context.HorariosTaller
                    .Select(h => h.Espacio)
                    .Distinct()
                    .CountAsync();

            ViewBag.TotalHorarios =
                await _context.HorariosTaller
                    .CountAsync();

            ViewBag.TotalInscripcionesConfirmadas =
                await _context.Inscripciones
                    .CountAsync(i =>
                        i.estado == "Confirmada"
                    );

            ViewBag.TotalInscripcionesCanceladas =
                await _context.Inscripciones
                    .CountAsync(i =>
                        i.estado == "Cancelada"
                    );

            ViewBag.TotalTalleres =
                await _context.Talleres
                    .CountAsync();

            return View();
        }
    }
}