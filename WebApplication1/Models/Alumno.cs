using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Alumno
    {
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; } = string.Empty;

        public string Matricula { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        public string Turno { get; set; } = string.Empty;
    }
}
