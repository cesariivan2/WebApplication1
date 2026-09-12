using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class PartidaTorneo
    {
        public int Id { get; set; }

        public int TorneoVideojuegoId { get; set; }

        public int Participante1Id { get; set; }

        public int? Participante2Id { get; set; }

        public int? GanadorId { get; set; }

        public int Ronda { get; set; }

        public int? Puntaje1 { get; set; }

        public int? Puntaje2 { get; set; }

        public DateTime? FechaHora { get; set; }

        [Required]
        [StringLength(30)]
        public string Estado { get; set; } = "Pendiente";

        public TorneoVideojuego? TorneoVideojuego { get; set; }

        public ParticipanteTorneo? Participante1 { get; set; }

        public ParticipanteTorneo? Participante2 { get; set; }

        public ParticipanteTorneo? Ganador { get; set; }
    }
}