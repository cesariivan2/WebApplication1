using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class EventoDataCode
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Nombre { get; set; } = "DATA CODE 2.0";

        [StringLength(1000)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ModalidadReto { get; set; } = "Buildathon";

        [Required]
        [StringLength(100)]
        public string CoberturaTerritorial { get; set; } = "Local";

        [Required]
        [StringLength(200)]
        public string Sede { get; set; } = string.Empty;

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFin { get; set; }

        public bool TieneCosto { get; set; }

        public decimal Costo { get; set; }

        [StringLength(10)]
        public string Moneda { get; set; } = "MXN";

        public bool RegistroAbierto { get; set; } = true;

        public DateTime? FechaLimiteRegistro { get; set; }
    }
}