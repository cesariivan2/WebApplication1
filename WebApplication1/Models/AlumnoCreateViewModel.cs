using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class AlumnoCreateViewModel
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public string Matricula { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required]
        public string Turno { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}