using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Administrador,Alumno")]
    public class InscripcionesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InscripcionesController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // ==========================================
        // LISTA DE INSCRIPCIONES
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var consulta =
                _context.Inscripciones
                    .Include(i => i.alumno)
                    .Include(i => i.horarioTaller)
                        .ThenInclude(h => h.Taller)
                    .AsQueryable();


            // Alumno solamente ve sus propias inscripciones
            if (User.IsInRole("Alumno"))
            {
                var alumno =
                    await ObtenerAlumnoActual();

                if (alumno == null)
                {
                    TempData["Error"] =
                        "No se encontró el perfil de alumno asociado a tu cuenta.";

                    return View(
                        new List<Inscripcion>()
                    );
                }

                consulta =
                    consulta.Where(i =>
                        i.alumnoId == alumno.Id
                    );
            }


            var inscripciones =
                await consulta
                    .OrderByDescending(
                        i => i.FechaInscripcion
                    )
                    .ToListAsync();


            return View(inscripciones);
        }


        // ==========================================
        // DETALLE
        // ==========================================
        public async Task<IActionResult> Details(
            int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var inscripcion =
                await _context.Inscripciones
                    .Include(i => i.alumno)
                    .Include(i => i.horarioTaller)
                        .ThenInclude(h => h.Taller)
                    .FirstOrDefaultAsync(i =>
                        i.id == id
                    );


            if (inscripcion == null)
            {
                return NotFound();
            }


            // Alumno solamente puede ver sus registros
            if (User.IsInRole("Alumno"))
            {
                var alumno =
                    await ObtenerAlumnoActual();

                if (alumno == null ||
                    inscripcion.alumnoId != alumno.Id)
                {
                    return Forbid();
                }
            }


            return View(inscripcion);
        }


        // ==========================================
        // ABRIR NUEVA INSCRIPCIÓN
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // ======================================
            // ADMINISTRADOR
            // ======================================
            if (User.IsInRole("Administrador"))
            {
                ViewBag.Alumnos =
                    await _context.Alumnos
                        .OrderBy(a => a.Nombre)
                        .ToListAsync();
            }
            else
            {
                // ==================================
                // ALUMNO
                // ==================================
                var alumno =
                    await ObtenerAlumnoActual();

                if (alumno == null)
                {
                    TempData["Error"] =
                        "Tu cuenta no tiene un perfil de alumno asociado.";

                    return RedirectToAction(
                        nameof(Index)
                    );
                }


                ViewBag.Alumnos =
                    new List<Alumno>
                    {
                        alumno
                    };
            }


            ViewBag.Horarios =
                await _context.HorariosTaller
                    .Include(h => h.Taller)
                    .OrderBy(h => h.fecha)
                    .ThenBy(h => h.horaInicio)
                    .ToListAsync();


            return View();
        }


        // ==========================================
        // CREAR INSCRIPCIÓN
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int alumnoId,
            int horarioTallerId)
        {
            Alumno? alumno;


            // ======================================
            // OBTENER ALUMNO
            // ======================================
            if (User.IsInRole("Administrador"))
            {
                alumno =
                    await _context.Alumnos
                        .FirstOrDefaultAsync(
                            a => a.Id == alumnoId
                        );
            }
            else
            {
                alumno =
                    await ObtenerAlumnoActual();

                if (alumno != null)
                {
                    // Ignoramos cualquier alumnoId
                    // enviado manualmente por el navegador
                    alumnoId = alumno.Id;
                }
            }


            if (alumno == null)
            {
                TempData["Error"] =
                    "El alumno no existe.";

                return RedirectToAction(
                    nameof(Create)
                );
            }


            // ======================================
            // BUSCAR HORARIO
            // ======================================
            var horario =
                await _context.HorariosTaller
                    .Include(h => h.Taller)
                    .FirstOrDefaultAsync(
                        h => h.id == horarioTallerId
                    );


            if (horario == null)
            {
                TempData["Error"] =
                    "El horario seleccionado no existe.";

                return RedirectToAction(
                    nameof(Create)
                );
            }


            // ======================================
            // BUSCAR INSCRIPCIÓN EXISTENTE
            // ======================================
            var inscripcionExistente =
                await _context.Inscripciones
                    .FirstOrDefaultAsync(i =>
                        i.alumnoId == alumnoId &&
                        i.horarioTallerId ==
                            horarioTallerId
                    );


            // Ya está inscrito
            if (inscripcionExistente != null &&
                inscripcionExistente.estado ==
                    "Confirmada")
            {
                TempData["Error"] =
                    "Ya tienes una inscripción activa en este horario.";

                return RedirectToAction(
                    nameof(Index)
                );
            }


            // ======================================
            // EVITAR HORARIOS EMPALMADOS
            // ======================================
            bool horarioEmpalmado =
                await _context.Inscripciones
                    .AnyAsync(i =>
                        i.alumnoId == alumnoId &&
                        i.estado == "Confirmada" &&

                        i.horarioTaller.fecha ==
                            horario.fecha &&

                        horario.horaInicio <
                            i.horarioTaller.horaFin &&

                        horario.horaFin >
                            i.horarioTaller.horaInicio
                    );


            if (horarioEmpalmado)
            {
                TempData["Error"] =
                    "Ya tienes otro taller programado a la misma hora.";

                return RedirectToAction(
                    nameof(Create)
                );
            }


            // ======================================
            // CONTAR MATUTINOS
            // ======================================
            int inscritosMatutinos =
                await _context.Inscripciones
                    .Include(i => i.alumno)
                    .CountAsync(i =>
                        i.horarioTallerId ==
                            horarioTallerId &&

                        i.estado ==
                            "Confirmada" &&

                        i.alumno != null &&

                        i.alumno.Turno ==
                            "Matutino"
                    );


            // ======================================
            // CONTAR VESPERTINOS
            // ======================================
            int inscritosVespertinos =
                await _context.Inscripciones
                    .Include(i => i.alumno)
                    .CountAsync(i =>
                        i.horarioTallerId ==
                            horarioTallerId &&

                        i.estado ==
                            "Confirmada" &&

                        i.alumno != null &&

                        i.alumno.Turno ==
                            "Vespertino"
                    );


            // ======================================
            // VALIDAR CUPO SEGÚN TURNO
            // ======================================
            if (alumno.Turno == "Matutino")
            {
                if (inscritosMatutinos >=
                    horario.CupoMatutino)
                {
                    TempData["Error"] =
                        "Ya no hay lugares disponibles para el turno matutino.";

                    return RedirectToAction(
                        nameof(Create)
                    );
                }
            }
            else if (alumno.Turno == "Vespertino")
            {
                if (inscritosVespertinos >=
                    horario.CupoVespertino)
                {
                    TempData["Error"] =
                        "Ya no hay lugares disponibles para el turno vespertino.";

                    return RedirectToAction(
                        nameof(Create)
                    );
                }
            }
            else
            {
                TempData["Error"] =
                    "El alumno no tiene un turno válido.";

                return RedirectToAction(
                    nameof(Create)
                );
            }


            // ======================================
            // REACTIVAR INSCRIPCIÓN CANCELADA
            // ======================================
            if (inscripcionExistente != null &&
                inscripcionExistente.estado ==
                    "Cancelada")
            {
                inscripcionExistente.estado =
                    "Confirmada";

                inscripcionExistente.FechaInscripcion =
                    DateTime.Now;


                await _context.SaveChangesAsync();


                TempData["Exito"] =
                    "Inscripción reactivada correctamente.";


                return RedirectToAction(
                    nameof(Index)
                );
            }


            // ======================================
            // CREAR INSCRIPCIÓN NUEVA
            // ======================================
            var inscripcion =
                new Inscripcion
                {
                    alumnoId =
                        alumnoId,

                    horarioTallerId =
                        horarioTallerId,

                    FechaInscripcion =
                        DateTime.Now,

                    estado =
                        "Confirmada"
                };


            _context.Inscripciones.Add(
                inscripcion
            );


            await _context.SaveChangesAsync();


            TempData["Exito"] =
                "Inscripción realizada correctamente.";


            return RedirectToAction(
                nameof(Index)
            );
        }


        // ==========================================
        // CANCELAR INSCRIPCIÓN
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(
            int id)
        {
            var inscripcion =
                await _context.Inscripciones
                    .Include(i => i.alumno)
                    .FirstOrDefaultAsync(
                        i => i.id == id
                    );


            if (inscripcion == null)
            {
                return NotFound();
            }


            // ======================================
            // ALUMNO SOLO CANCELA LAS SUYAS
            // ======================================
            if (User.IsInRole("Alumno"))
            {
                var alumno =
                    await ObtenerAlumnoActual();


                if (alumno == null ||
                    inscripcion.alumnoId != alumno.Id)
                {
                    return Forbid();
                }
            }


            if (inscripcion.estado ==
                "Cancelada")
            {
                TempData["Error"] =
                    "La inscripción ya estaba cancelada.";

                return RedirectToAction(
                    nameof(Index)
                );
            }


            inscripcion.estado =
                "Cancelada";


            await _context.SaveChangesAsync();


            TempData["Exito"] =
                "Inscripción cancelada correctamente.";


            return RedirectToAction(
                nameof(Index)
            );
        }


        // ==========================================
        // OBTENER ALUMNO DEL USUARIO LOGUEADO
        // ==========================================
        private async Task<Alumno?>
            ObtenerAlumnoActual()
        {
            string? usuarioId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );


            if (string.IsNullOrEmpty(
                usuarioId))
            {
                return null;
            }


            return await _context.Alumnos
                .FirstOrDefaultAsync(
                    a => a.UsuarioId ==
                        usuarioId
                );
        }
    }
}