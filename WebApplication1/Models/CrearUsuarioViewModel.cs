using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class CrearUsuarioViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Rol { get; set; } = string.Empty;

        public string? Nombre { get; set; }

        public string? Matricula { get; set; }

        public string? Turno { get; set; }
    }
}