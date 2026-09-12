using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // DASHBOARD PRINCIPAL
        // ==========================================
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalAlumnos =
                await _context.Alumnos.CountAsync();

            ViewBag.TotalTalleres =
                await _context.Talleres.CountAsync();

            ViewBag.TotalHorarios =
                await _context.HorariosTaller.CountAsync();

            ViewBag.InscripcionesConfirmadas =
                await _context.Inscripciones
                    .CountAsync(i => i.estado == "Confirmada");

            ViewBag.InscripcionesCanceladas =
                await _context.Inscripciones
                    .CountAsync(i => i.estado == "Cancelada");

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId =
                        Activity.Current?.Id ??
                        HttpContext.TraceIdentifier
                }
            );
        }
    }
}