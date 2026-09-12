using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Asistencia
    {
        public int Id { get; set; }

        public int InscripcionId { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public bool Presente { get; set; }

        [StringLength(250)]
        public string? Observacion { get; set; }

        public Inscripcion? Inscripcion { get; set; }
    }
}