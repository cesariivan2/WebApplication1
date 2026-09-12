using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Alumno> Alumnos { get; set; }
        public DbSet<Taller> Talleres { get; set; }

        public DbSet<HorarioTaller> HorariosTaller { get; set; }

        public DbSet<Inscripcion> Inscripciones { get; set; }
    }
}