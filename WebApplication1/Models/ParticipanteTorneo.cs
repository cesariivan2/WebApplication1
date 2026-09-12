using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class ParticipanteTorneo
    {
        public int Id { get; set; }

        public int TorneoVideojuegoId { get; set; }

        public int AlumnoId { get; set; }

        public DateTime FechaInscripcion { get; set; } = DateTime.Now;

        [Required]
        [StringLength(30)]
        public string Estado { get; set; } = "Inscrito";

        public TorneoVideojuego? TorneoVideojuego { get; set; }

        public Alumno? Alumno { get; set; }
    }
}