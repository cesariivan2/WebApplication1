using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class TorneoVideojuego
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Videojuego { get; set; } = string.Empty;

        [StringLength(100)]
        public string Plataforma { get; set; } = string.Empty;

        [StringLength(100)]
        public string Formato { get; set; } = "Eliminación directa";

        [StringLength(150)]
        public string Lugar { get; set; } = string.Empty;

        [Range(2, 256)]
        public int CupoMaximo { get; set; } = 16;

        public DateTime FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        [Required]
        [StringLength(30)]
        public string Estado { get; set; } = "Abierto";
    }
}