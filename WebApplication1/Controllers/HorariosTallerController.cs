using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HorariosTallerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HorariosTallerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // LISTAR TODOS LOS HORARIOS
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var horarios = await _context.HorariosTaller
                .Include(h => h.Taller)
                .OrderBy(h => h.fecha)
                .ThenBy(h => h.horaInicio)
                .ToListAsync();

            return View(horarios);
        }

        // ==========================================
        // ABRIR FORMULARIO PARA CREAR
        // ==========================================
        public async Task<IActionResult> Create()
        {
            ViewBag.Talleres = await _context.Talleres
                .OrderBy(t => t.nombre)
                .ToListAsync();

            return View();
        }

        // ==========================================
        // GUARDAR NUEVO HORARIO
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HorarioTaller horario)
        {
            // Comprobar que el taller exista
            bool tallerExiste = await _context.Talleres
                .AnyAsync(t => t.id == horario.tallerId);

            if (!tallerExiste)
            {
                ModelState.AddModelError(
                    "tallerId",
                    "El taller seleccionado no existe."
                );
            }

            // La hora final debe ser posterior a la inicial
            if (horario.horaFin <= horario.horaInicio)
            {
                ModelState.AddModelError(
                    "horaFin",
                    "La hora de finalización debe ser posterior a la hora de inicio."
                );
            }

            // Cupo matutino válido
            if (horario.CupoMatutino < 0)
            {
                ModelState.AddModelError(
                    "CupoMatutino",
                    "El cupo matutino no puede ser negativo."
                );
            }

            // Cupo vespertino válido
            if (horario.CupoVespertino < 0)
            {
                ModelState.AddModelError(
                    "CupoVespertino",
                    "El cupo vespertino no puede ser negativo."
                );
            }

            // Evitar que dos talleres utilicen
            // el mismo espacio al mismo tiempo
            bool espacioOcupado = await _context.HorariosTaller
                .AnyAsync(h =>
                    h.fecha == horario.fecha &&
                    h.Espacio == horario.Espacio &&
                    horario.horaInicio < h.horaFin &&
                    horario.horaFin > h.horaInicio
                );

            if (espacioOcupado)
            {
                ModelState.AddModelError(
                    "Espacio",
                    "Este espacio ya está ocupado por otro taller en ese horario."
                );
            }

            // Si todo está correcto, guardar
            if (ModelState.IsValid)
            {
                _context.HorariosTaller.Add(horario);

                await _context.SaveChangesAsync();

                TempData["Exito"] =
                    "Horario creado correctamente.";

                return RedirectToAction(nameof(Index));
            }

            // Si hubo errores, volver a cargar talleres
            ViewBag.Talleres = await _context.Talleres
                .OrderBy(t => t.nombre)
                .ToListAsync();

            return View(horario);
        }

        // ==========================================
        // VER DETALLES
        // ==========================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var horario = await _context.HorariosTaller
                .Include(h => h.Taller)
                .FirstOrDefaultAsync(h => h.id == id);

            if (horario == null)
            {
                return NotFound();
            }

            return View(horario);
        }

        // ==========================================
        // ABRIR FORMULARIO PARA EDITAR
        // ==========================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var horario = await _context.HorariosTaller
                .FindAsync(id);

            if (horario == null)
            {
                return NotFound();
            }

            ViewBag.Talleres = await _context.Talleres
                .OrderBy(t => t.nombre)
                .ToListAsync();

            return View(horario);
        }

        // ==========================================
        // GUARDAR EDICIÓN
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            HorarioTaller horario)
        {
            if (id != horario.id)
            {
                return NotFound();
            }

            // Comprobar que el taller exista
            bool tallerExiste = await _context.Talleres
                .AnyAsync(t => t.id == horario.tallerId);

            if (!tallerExiste)
            {
                ModelState.AddModelError(
                    "tallerId",
                    "El taller seleccionado no existe."
                );
            }

            // Validar horas
            if (horario.horaFin <= horario.horaInicio)
            {
                ModelState.AddModelError(
                    "horaFin",
                    "La hora de finalización debe ser posterior a la hora de inicio."
                );
            }

            // Validar cupo matutino
            if (horario.CupoMatutino < 0)
            {
                ModelState.AddModelError(
                    "CupoMatutino",
                    "El cupo matutino no puede ser negativo."
                );
            }

            // Validar cupo vespertino
            if (horario.CupoVespertino < 0)
            {
                ModelState.AddModelError(
                    "CupoVespertino",
                    "El cupo vespertino no puede ser negativo."
                );
            }

            // Evitar choques de espacio.
            // h.id != horario.id sirve para no compararlo consigo mismo.
            bool espacioOcupado = await _context.HorariosTaller
                .AnyAsync(h =>
                    h.id != horario.id &&
                    h.fecha == horario.fecha &&
                    h.Espacio == horario.Espacio &&
                    horario.horaInicio < h.horaFin &&
                    horario.horaFin > h.horaInicio
                );

            if (espacioOcupado)
            {
                ModelState.AddModelError(
                    "Espacio",
                    "Este espacio ya está ocupado por otro taller en ese horario."
                );
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.HorariosTaller.Update(horario);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    bool existe = await _context.HorariosTaller
                        .AnyAsync(h => h.id == horario.id);

                    if (!existe)
                    {
                        return NotFound();
                    }

                    throw;
                }

                TempData["Exito"] =
                    "Horario actualizado correctamente.";

                return RedirectToAction(nameof(Index));
            }

            // Si hay errores, volver a cargar talleres
            ViewBag.Talleres = await _context.Talleres
                .OrderBy(t => t.nombre)
                .ToListAsync();

            return View(horario);
        }

        // ==========================================
        // ABRIR CONFIRMACIÓN PARA ELIMINAR
        // ==========================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var horario = await _context.HorariosTaller
                .Include(h => h.Taller)
                .FirstOrDefaultAsync(h => h.id == id);

            if (horario == null)
            {
                return NotFound();
            }

            return View(horario);
        }

        // ==========================================
        // ELIMINAR HORARIO
        // ==========================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Verificar si existen inscripciones
            bool tieneInscripciones = await _context.Inscripciones
                .AnyAsync(i => i.horarioTallerId == id);

            if (tieneInscripciones)
            {
                TempData["Error"] =
                    "No se puede eliminar el horario porque ya tiene inscripciones.";

                return RedirectToAction(nameof(Index));
            }

            var horario = await _context.HorariosTaller
                .FindAsync(id);

            if (horario == null)
            {
                return NotFound();
            }

            _context.HorariosTaller.Remove(horario);

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "Horario eliminado correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}