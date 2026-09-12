using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Controllers
{
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // RESUMEN GENERAL DEL EVENTO
        // ==========================================
        public async Task<IActionResult> Index()
        {
            // ALUMNOS
            ViewBag.TotalAlumnos =
                await _context.Alumnos.CountAsync();

            ViewBag.AlumnosMatutinos =
                await _context.Alumnos
                    .CountAsync(a => a.Turno == "Matutino");

            ViewBag.AlumnosVespertinos =
                await _context.Alumnos
                    .CountAsync(a => a.Turno == "Vespertino");

            // TALLERES
            ViewBag.TotalTalleres =
                await _context.Talleres.CountAsync();

            // HORARIOS
            ViewBag.TotalHorarios =
                await _context.HorariosTaller.CountAsync();

            // INSCRIPCIONES
            ViewBag.TotalInscripciones =
                await _context.Inscripciones.CountAsync();

            ViewBag.Confirmadas =
                await _context.Inscripciones
                    .CountAsync(i => i.estado == "Confirmada");

            ViewBag.Canceladas =
                await _context.Inscripciones
                    .CountAsync(i => i.estado == "Cancelada");

            return View();
        }

        // ==========================================
        // REPORTE DE OCUPACIÓN DE HORARIOS
        // ==========================================
        public async Task<IActionResult> Ocupacion()
        {
            var horarios = await _context.HorariosTaller
                .Include(h => h.Taller)
                .OrderBy(h => h.fecha)
                .ThenBy(h => h.horaInicio)
                .ToListAsync();

            var reporte = new List<object>();

            foreach (var horario in horarios)
            {
                int matutinos = await _context.Inscripciones
                    .Include(i => i.alumno)
                    .CountAsync(i =>
                        i.horarioTallerId == horario.id &&
                        i.estado == "Confirmada" &&
                        i.alumno != null &&
                        i.alumno.Turno == "Matutino"
                    );

                int vespertinos = await _context.Inscripciones
                    .Include(i => i.alumno)
                    .CountAsync(i =>
                        i.horarioTallerId == horario.id &&
                        i.estado == "Confirmada" &&
                        i.alumno != null &&
                        i.alumno.Turno == "Vespertino"
                    );

                int disponiblesMatutinos =
                    horario.CupoMatutino - matutinos;

                int disponiblesVespertinos =
                    horario.CupoVespertino - vespertinos;

                reporte.Add(new
                {
                    HorarioId = horario.id,

                    Taller =
                        horario.Taller != null
                            ? horario.Taller.nombre
                            : "Sin taller",

                    Fecha = horario.fecha,

                    HoraInicio = horario.horaInicio,

                    HoraFin = horario.horaFin,

                    Espacio = horario.Espacio,

                    CupoMatutino = horario.CupoMatutino,

                    InscritosMatutinos = matutinos,

                    DisponiblesMatutinos =
                        disponiblesMatutinos,

                    CupoVespertino =
                        horario.CupoVespertino,

                    InscritosVespertinos =
                        vespertinos,

                    DisponiblesVespertinos =
                        disponiblesVespertinos
                });
            }

            ViewBag.Reporte = reporte;

            return View();
        }

        // ==========================================
        // REPORTE DE INSCRIPCIONES POR ALUMNO
        // ==========================================
        public async Task<IActionResult> Inscripciones()
        {
            var inscripciones =
                await _context.Inscripciones
                    .Include(i => i.alumno)
                    .Include(i => i.horarioTaller)
                        .ThenInclude(h => h!.Taller)
                    .OrderByDescending(
                        i => i.FechaInscripcion
                    )
                    .ToListAsync();

            return View(inscripciones);
        }
    }
}