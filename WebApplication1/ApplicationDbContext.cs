using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<PropuestaComite> PropuestasComite { get; set; }

        public DbSet<VotoPropuesta> VotosPropuesta { get; set; }
        public DbSet<Alumno> Alumnos { get; set; }
        public DbSet<Taller> Talleres { get; set; }

        public DbSet<HorarioTaller> HorariosTaller { get; set; }

        public DbSet<Inscripcion> Inscripciones { get; set; }
        public DbSet<Asistencia> Asistencias { get; set; }
        public DbSet<TorneoVideojuego> TorneosVideojuegos { get; set; }

        public DbSet<ParticipanteTorneo> ParticipantesTorneo { get; set; }

        public DbSet<PartidaTorneo> PartidasTorneo { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Asistencia>()
    .HasIndex(a => a.InscripcionId)
    .IsUnique();
            builder.Entity<Alumno>()
                .HasOne(a => a.Usuario)
                .WithOne()
                .HasForeignKey<Alumno>(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);
            builder.Entity<Alumno>()
    .HasIndex(a => a.Matricula)
    .IsUnique();

            builder.Entity<Alumno>()
                .HasIndex(a => a.Correo)
                .IsUnique();
            builder.Entity<Inscripcion>()
    .HasIndex(i => new
    {
        i.alumnoId,
        i.horarioTallerId
    })
    .IsUnique();




            builder.Entity<Asistencia>()
            .HasOne(a => a.Inscripcion)
            .WithOne()
            .HasForeignKey<Asistencia>(a => a.InscripcionId)
            .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<PropuestaComite>()
    .HasOne(p => p.CreadoPorUsuario)
    .WithMany()
    .HasForeignKey(p => p.CreadoPorUsuarioId)
    .OnDelete(DeleteBehavior.SetNull);
            builder.Entity<VotoPropuesta>()
    .HasOne(v => v.PropuestaComite)
    .WithMany()
    .HasForeignKey(v => v.PropuestaComiteId)
    .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<VotoPropuesta>()
    .HasOne(v => v.Usuario)
    .WithMany()
    .HasForeignKey(v => v.UsuarioId)
    .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<VotoPropuesta>()
    .HasIndex(v => new
    {
        v.PropuestaComiteId,
        v.UsuarioId
    })
    .IsUnique();
            builder.Entity<ParticipanteTorneo>()
    .HasOne(p => p.TorneoVideojuego)
    .WithMany()
    .HasForeignKey(p => p.TorneoVideojuegoId)
    .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ParticipanteTorneo>()
                .HasOne(p => p.Alumno)
                .WithMany()
                .HasForeignKey(p => p.AlumnoId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ParticipanteTorneo>()
    .HasIndex(p => new
    {
        p.TorneoVideojuegoId,
        p.AlumnoId
    })
    .IsUnique();
            builder.Entity<PartidaTorneo>()
    .HasOne(p => p.TorneoVideojuego)
    .WithMany()
    .HasForeignKey(p => p.TorneoVideojuegoId)
    .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PartidaTorneo>()
                .HasOne(p => p.Participante1)
                .WithMany()
                .HasForeignKey(p => p.Participante1Id)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<PartidaTorneo>()
                .HasOne(p => p.Participante2)
                .WithMany()
                .HasForeignKey(p => p.Participante2Id)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<PartidaTorneo>()
                .HasOne(p => p.Ganador)
                .WithMany()
                .HasForeignKey(p => p.GanadorId)
                .OnDelete(DeleteBehavior.NoAction);
    
        }
    }
}

