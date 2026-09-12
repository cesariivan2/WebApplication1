namespace WebApplication1.Models
{
    public class HorarioTaller
    {
        public int id { get; set; }
        public int tallerId { get; set; }
        public DateTime fecha { get; set; }
        public TimeSpan horaInicio { get; set; }
        public TimeSpan horaFin { get; set; }
        public string Espacio { get; set; } = string.Empty;

        public int CupoMatutino { get; set; }

        public int CupoVespertino { get; set; }

        public Taller? Taller { get; set; }
    }
}
