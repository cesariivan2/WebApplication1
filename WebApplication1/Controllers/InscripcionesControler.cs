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

        // ==========================================
        // MOSTRAR TODAS LAS INSCRIPCIONES
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var inscripciones = await _context.Inscripciones
                .Include(i => i.alumno)
                .Include(i => i.horarioTaller)
                    .ThenInclude(h => h!.Taller)
                .OrderByDescending(i => i.FechaInscripcion)
                .ToListAsync();

            return View(inscripciones);
        }

        // ==========================================
        // ABRIR FORMULARIO DE INSCRIPCIÓN
        // ==========================================
        public async Task<IActionResult> Create()
        {
            ViewBag.Alumnos = await _context.Alumnos
                .OrderBy(a => a.Nombre)
                .ToListAsync();

            ViewBag.Horarios = await _context.HorariosTaller
                .Include(h => h.Taller)
                .OrderBy(h => h.fecha)
                .ThenBy(h => h.horaInicio)
                .ToListAsync();

            return View();
        }

        // ==========================================
        // REALIZAR INSCRIPCIÓN
        // ==========================================
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
                TempData["Error"] =
                    "El alumno seleccionado no existe.";

                return RedirectToAction(nameof(Create));
            }

            // 2. Buscar horario y taller
            var horario = await _context.HorariosTaller
                .Include(h => h.Taller)
                .FirstOrDefaultAsync(
                    h => h.id == horarioTallerId
                );

            if (horario == null)
            {
                TempData["Error"] =
                    "El horario seleccionado no existe.";

                return RedirectToAction(nameof(Create));
            }

            // ==========================================
            // 3. EVITAR INSCRIPCIÓN DUPLICADA ACTIVA
            // ==========================================
            bool yaInscrito = await _context.Inscripciones
                .AnyAsync(i =>
                    i.alumnoId == alumnoId &&
                    i.horarioTallerId == horarioTallerId &&
                    i.estado == "Confirmada"
                );

            if (yaInscrito)
            {
                TempData["Error"] =
                    "El alumno ya está inscrito en este horario.";

                return RedirectToAction(nameof(Create));
            }

            // ==========================================
            // 4. EVITAR CHOQUE DE HORARIOS DEL ALUMNO
            // ==========================================
            bool horarioEmpalmado = await _context.Inscripciones
                .AnyAsync(i =>
                    i.alumnoId == alumnoId &&
                    i.estado == "Confirmada" &&
                    i.horarioTaller != null &&
                    i.horarioTaller.fecha == horario.fecha &&
                    horario.horaInicio < i.horarioTaller.horaFin &&
                    horario.horaFin > i.horarioTaller.horaInicio
                );

            if (horarioEmpalmado)
            {
                TempData["Error"] =
                    "El alumno ya está inscrito en otro taller que se realiza a la misma hora.";

                return RedirectToAction(nameof(Create));
            }

            // ==========================================
            // 5. CONTAR INSCRITOS MATUTINOS
            // ==========================================
            int inscritosMatutinos =
                await _context.Inscripciones
                .Include(i => i.alumno)
                .CountAsync(i =>
                    i.horarioTallerId == horarioTallerId &&
                    i.estado == "Confirmada" &&
                    i.alumno != null &&
                    i.alumno.Turno == "Matutino"
                );

            // ==========================================
            // 6. CONTAR INSCRITOS VESPERTINOS
            // ==========================================
            int inscritosVespertinos =
                await _context.Inscripciones
                .Include(i => i.alumno)
                .CountAsync(i =>
                    i.horarioTallerId == horarioTallerId &&
                    i.estado == "Confirmada" &&
                    i.alumno != null &&
                    i.alumno.Turno == "Vespertino"
                );

            // ==========================================
            // 7. REVISAR CUPO SEGÚN TURNO
            // ==========================================
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

            // ==========================================
            // 8. CREAR INSCRIPCIÓN
            // ==========================================
            var inscripcion = new Inscripcion
            {
                alumnoId = alumnoId,
                horarioTallerId = horarioTallerId,
                FechaInscripcion = DateTime.Now,
                estado = "Confirmada"
            };

            _context.Inscripciones.Add(inscripcion);

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "Inscripción realizada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // VER DETALLES DE UNA INSCRIPCIÓN
        // ==========================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inscripcion = await _context.Inscripciones
                .Include(i => i.alumno)
                .Include(i => i.horarioTaller)
                    .ThenInclude(h => h!.Taller)
                .FirstOrDefaultAsync(i => i.id == id);

            if (inscripcion == null)
            {
                return NotFound();
            }

            return View(inscripcion);
        }

        // ==========================================
        // CANCELAR INSCRIPCIÓN
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var inscripcion = await _context.Inscripciones
                .FindAsync(id);

            if (inscripcion == null)
            {
                return NotFound();
            }

            // Si ya estaba cancelada, no hacer nada
            if (inscripcion.estado == "Cancelada")
            {
                TempData["Error"] =
                    "Esta inscripción ya se encuentra cancelada.";

                return RedirectToAction(nameof(Index));
            }

            inscripcion.estado = "Cancelada";

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "La inscripción fue cancelada correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}