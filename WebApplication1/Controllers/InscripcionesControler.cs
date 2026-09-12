using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Administrador,Alumno")]
    public class InscripcionesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InscripcionesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // MOSTRAR INSCRIPCIONES
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var consulta = _context.Inscripciones
                .Include(i => i.alumno)
                .Include(i => i.horarioTaller)
                    .ThenInclude(h => h!.Taller)
                .AsQueryable();

            // Si es Alumno, solo puede ver sus inscripciones
            if (User.IsInRole("Alumno"))
            {
                string? correo = User.Identity?.Name;

                if (string.IsNullOrEmpty(correo))
                {
                    return Forbid();
                }

                var alumno = await _context.Alumnos
                    .FirstOrDefaultAsync(a => a.Correo == correo);

                if (alumno == null)
                {
                    TempData["Error"] =
                        "No existe un alumno relacionado con esta cuenta.";

                    return View(new List<Inscripcion>());
                }

                consulta = consulta
                    .Where(i => i.alumnoId == alumno.Id);
            }

            var inscripciones = await consulta
                .OrderByDescending(i => i.FechaInscripcion)
                .ToListAsync();

            return View(inscripciones);
        }

        // ==========================================
        // ABRIR FORMULARIO
        // ==========================================
        public async Task<IActionResult> Create()
        {
            if (User.IsInRole("Administrador"))
            {
                ViewBag.Alumnos = await _context.Alumnos
                    .OrderBy(a => a.Nombre)
                    .ToListAsync();
            }
            else
            {
                string? correo = User.Identity?.Name;

                var alumno = await _context.Alumnos
                    .FirstOrDefaultAsync(a => a.Correo == correo);

                if (alumno == null)
                {
                    TempData["Error"] =
                        "Tu cuenta no está relacionada con ningún alumno.";

                    return RedirectToAction(nameof(Index));
                }

                // Solo enviamos al alumno actual
                ViewBag.Alumnos = new List<Alumno>
                {
                    alumno
                };
            }

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
            Alumno? alumno;

            // ADMIN puede seleccionar alumno
            if (User.IsInRole("Administrador"))
            {
                alumno = await _context.Alumnos
                    .FindAsync(alumnoId);
            }
            else
            {
                // ALUMNO solo puede inscribirse a sí mismo
                string? correo = User.Identity?.Name;

                if (string.IsNullOrEmpty(correo))
                {
                    return Forbid();
                }

                alumno = await _context.Alumnos
                    .FirstOrDefaultAsync(a => a.Correo == correo);

                if (alumno != null)
                {
                    alumnoId = alumno.Id;
                }
            }

            if (alumno == null)
            {
                TempData["Error"] =
                    "El alumno no existe.";

                return RedirectToAction(nameof(Create));
            }

            // Buscar horario
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
            // EVITAR INSCRIPCIÓN DUPLICADA
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
                    "Ya existe una inscripción activa en este horario.";

                return RedirectToAction(nameof(Create));
            }

            // ==========================================
            // EVITAR EMPALME DE HORARIOS
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
                    "Ya tienes otro taller que se realiza a la misma hora.";

                return RedirectToAction(nameof(Create));
            }

            // ==========================================
            // CONTAR MATUTINOS
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
            // CONTAR VESPERTINOS
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
            // VALIDAR CUPO SEGÚN TURNO
            // ==========================================
            if (alumno.Turno == "Matutino")
            {
                if (inscritosMatutinos >= horario.CupoMatutino)
                {
                    TempData["Error"] =
                        "Ya no hay lugares disponibles para el turno matutino.";

                    return RedirectToAction(nameof(Create));
                }
            }
            else if (alumno.Turno == "Vespertino")
            {
                if (inscritosVespertinos >= horario.CupoVespertino)
                {
                    TempData["Error"] =
                        "Ya no hay lugares disponibles para el turno vespertino.";

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
            // CREAR INSCRIPCIÓN
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
        // DETALLES
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

            // Alumno solamente puede ver la suya
            if (User.IsInRole("Alumno"))
            {
                string? correo = User.Identity?.Name;

                if (inscripcion.alumno == null ||
                    inscripcion.alumno.Correo != correo)
                {
                    return Forbid();
                }
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
                .Include(i => i.alumno)
                .FirstOrDefaultAsync(i => i.id == id);

            if (inscripcion == null)
            {
                return NotFound();
            }

            // Alumno solamente puede cancelar la suya
            if (User.IsInRole("Alumno"))
            {
                string? correo = User.Identity?.Name;

                if (inscripcion.alumno == null ||
                    inscripcion.alumno.Correo != correo)
                {
                    return Forbid();
                }
            }

            if (inscripcion.estado == "Cancelada")
            {
                TempData["Error"] =
                    "Esta inscripción ya está cancelada.";

                return RedirectToAction(nameof(Index));
            }

            inscripcion.estado = "Cancelada";

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "Inscripción cancelada correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}