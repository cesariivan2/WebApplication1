using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Administrador,Tallerista")]
    public class AsistenciasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AsistenciasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // LISTAR REGISTROS DE ASISTENCIA
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var asistencias = await _context.Asistencias
                .Include(a => a.Inscripcion)
                    .ThenInclude(i => i!.alumno)
                .Include(a => a.Inscripcion)
                    .ThenInclude(i => i!.horarioTaller)
                        .ThenInclude(h => h!.Taller)
                .OrderByDescending(a => a.FechaRegistro)
                .ToListAsync();

            return View(asistencias);
        }

        // ==========================================
        // VER PARTICIPANTES DE UN HORARIO
        // Y SU ESTADO DE ASISTENCIA
        // ==========================================
        public async Task<IActionResult> PorHorario(int id)
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

            var idsInscripciones = inscripciones
                .Select(i => i.id)
                .ToList();

            var asistencias = await _context.Asistencias
                .Where(a =>
                    idsInscripciones.Contains(a.InscripcionId)
                )
                .ToDictionaryAsync(
                    a => a.InscripcionId,
                    a => a
                );

            ViewBag.Horario = horario;
            ViewBag.Asistencias = asistencias;

            return View(inscripciones);
        }

        // ==========================================
        // ABRIR FORMULARIO PARA REGISTRAR
        // O MODIFICAR ASISTENCIA
        // ==========================================
        public async Task<IActionResult> Registrar(
            int inscripcionId)
        {
            var inscripcion = await _context.Inscripciones
                .Include(i => i.alumno)
                .Include(i => i.horarioTaller)
                    .ThenInclude(h => h!.Taller)
                .FirstOrDefaultAsync(i =>
                    i.id == inscripcionId &&
                    i.estado == "Confirmada"
                );

            if (inscripcion == null)
            {
                return NotFound();
            }

            var asistencia = await _context.Asistencias
                .FirstOrDefaultAsync(a =>
                    a.InscripcionId == inscripcionId
                );

            ViewBag.Inscripcion = inscripcion;

            if (asistencia == null)
            {
                asistencia = new Asistencia
                {
                    InscripcionId = inscripcionId,
                    FechaRegistro = DateTime.Now,
                    Presente = false
                };
            }

            return View(asistencia);
        }

        // ==========================================
        // GUARDAR ASISTENCIA
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(
            int inscripcionId,
            bool presente,
            string? observacion)
        {
            // Comprobar que la inscripción exista
            // y siga confirmada
            var inscripcion = await _context.Inscripciones
                .FirstOrDefaultAsync(i =>
                    i.id == inscripcionId &&
                    i.estado == "Confirmada"
                );

            if (inscripcion == null)
            {
                TempData["Error"] =
                    "La inscripción no existe o ya no está confirmada.";

                return RedirectToAction(nameof(Index));
            }

            // Buscar si ya tiene registro de asistencia
            var asistencia = await _context.Asistencias
                .FirstOrDefaultAsync(a =>
                    a.InscripcionId == inscripcionId
                );

            // Si no existe, crear
            if (asistencia == null)
            {
                asistencia = new Asistencia
                {
                    InscripcionId = inscripcionId,
                    FechaRegistro = DateTime.Now,
                    Presente = presente,
                    Observacion =
                        string.IsNullOrWhiteSpace(observacion)
                            ? null
                            : observacion.Trim()
                };

                _context.Asistencias.Add(asistencia);
            }
            else
            {
                // Si ya existe, actualizar.
                // Así no creamos asistencias duplicadas.
                asistencia.Presente = presente;

                asistencia.FechaRegistro =
                    DateTime.Now;

                asistencia.Observacion =
                    string.IsNullOrWhiteSpace(observacion)
                        ? null
                        : observacion.Trim();
            }

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                presente
                    ? "Asistencia registrada como presente."
                    : "Asistencia registrada como ausente.";

            return RedirectToAction(
                nameof(PorHorario),
                new
                {
                    id = inscripcion.horarioTallerId
                }
            );
        }

        // ==========================================
        // VER DETALLE DE ASISTENCIA
        // ==========================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asistencia = await _context.Asistencias
                .Include(a => a.Inscripcion)
                    .ThenInclude(i => i!.alumno)
                .Include(a => a.Inscripcion)
                    .ThenInclude(i => i!.horarioTaller)
                        .ThenInclude(h => h!.Taller)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (asistencia == null)
            {
                return NotFound();
            }

            return View(asistencia);
        }

        // ==========================================
        // RESUMEN DE ASISTENCIA
        // ==========================================
        public async Task<IActionResult> Resumen()
        {
            ViewBag.TotalRegistros =
                await _context.Asistencias.CountAsync();

            ViewBag.Presentes =
                await _context.Asistencias
                    .CountAsync(a => a.Presente);

            ViewBag.Ausentes =
                await _context.Asistencias
                    .CountAsync(a => !a.Presente);

            return View();
        }
    }
}