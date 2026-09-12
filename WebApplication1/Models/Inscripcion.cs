namespace WebApplication1.Models
{
    public class Inscripcion
    {
        public int id { get; set; }
        public int alumnoId { get; set; }
        public int horarioTallerId { get; set; }
        public DateTime FechaInscripcion { get; set; }
        public string estado { get; set; } = string.Empty;
        public Alumno? alumno { get; set; }
        public HorarioTaller? horarioTaller { get; set; }
    }
}
