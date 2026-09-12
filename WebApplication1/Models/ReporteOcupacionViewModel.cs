namespace WebApplication1.Models
{
    public class ReporteOcupacionViewModel
    {
        public int HorarioId { get; set; }

        public string Taller { get; set; } =
            string.Empty;

        public DateTime Fecha { get; set; }

        public TimeSpan HoraInicio { get; set; }

        public TimeSpan HoraFin { get; set; }

        public string Espacio { get; set; } =
            string.Empty;

        public int CupoMatutino { get; set; }

        public int InscritosMatutinos { get; set; }

        public int DisponiblesMatutinos { get; set; }

        public int CupoVespertino { get; set; }

        public int InscritosVespertinos { get; set; }

        public int DisponiblesVespertinos { get; set; }
    }
}