using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class InscripcionesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InscripcionesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // MOSTRAR TODAS LAS INSCRIPCIONES
        public async Task<IActionResult> Index()
        {
            var inscripciones = await _context.Inscripciones
                .Include(i => i.alumno)
                .Include(i => i.horarioTaller)
                    .ThenInclude(h => h!.Taller)
                .ToListAsync();

            return View(inscripciones);
        }

        // ABRIR FORMULARIO
        public async Task<IActionResult> Create()
        {
            ViewBag.Alumnos = await _context.Alumnos.ToListAsync();

            ViewBag.Horarios = await _context.HorariosTaller
                .Include(h => h.Taller)
                .ToListAsync();

            return View();
        }

        // REALIZAR INSCRIPCIÓN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int alumnoId,
            int horarioTallerId)
        {
            // 1. Buscar alumno
            var alumno = await _context.Alumnos
                .FindAsync(alumnoId);

            if (alumno == null)
            {
                return NotFound();
            }

            // 2. Buscar horario
            var horario = await _context.HorariosTaller
                .Include(h => h.Taller)
                .FirstOrDefaultAsync(
                    h => h.id == horarioTallerId
                );

            if (horario == null)
            {
                return NotFound();
            }

            // 3. Evitar inscripción duplicada
            bool yaInscrito = await _context.Inscripciones
                .AnyAsync(i =>
                    i.alumnoId == alumnoId &&
                    i.horarioTallerId == horarioTallerId
                );

            if (yaInscrito)
            {
                TempData["Error"] =
                    "El alumno ya está inscrito en este horario.";

                return RedirectToAction(nameof(Create));
            }

            // 4. Contar alumnos del turno matutino
            int inscritosMatutinos =
                await _context.Inscripciones
                .Include(i => i.alumno)
                .CountAsync(i =>
                    i.horarioTallerId == horarioTallerId &&
                    i.alumno != null &&
                    i.alumno.Turno == "Matutino"
                );

            // 5. Contar alumnos del turno vespertino
            int inscritosVespertinos =
                await _context.Inscripciones
                .Include(i => i.alumno)
                .CountAsync(i =>
                    i.horarioTallerId == horarioTallerId &&
                    i.alumno != null &&
                    i.alumno.Turno == "Vespertino"
                );

            // 6. Revisar cupo según turno
            if (alumno.Turno == "Matutino")
            {
                if (inscritosMatutinos >= horario.CupoMatutino)
                {
                    TempData["Error"] =
                        "Ya no hay lugares disponibles para alumnos del turno matutino.";

                    return RedirectToAction(nameof(Create));
                }
            }
            else if (alumno.Turno == "Vespertino")
            {
                if (inscritosVespertinos >= horario.CupoVespertino)
                {
                    TempData["Error"] =
                        "Ya no hay lugares disponibles para alumnos del turno vespertino.";

                    return RedirectToAction(nameof(Create));
                }
            }
            else
            {
                TempData["Error"] =
                    "El alumno no tiene un turno válido.";

                return RedirectToAction(nameof(Create));
            }

            // 7. Crear inscripción
            var inscripcion = new Inscripcion
            {
                alumnoId = alumnoId,
                horarioTallerId = horarioTallerId,
                FechaInscripcion = DateTime.Now,
                estado = "Confirmada"
            };

            // 8. Guardar en SQL Server
            _context.Inscripciones.Add(inscripcion);

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "Inscripción realizada correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}