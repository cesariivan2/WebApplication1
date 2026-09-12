using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace WebApplication1.Models
{
    public class Alumno
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La matrícula es obligatoria")]
        [StringLength(20)]
        public string Matricula { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        [StringLength(150)]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El turno es obligatorio")]
        [RegularExpression(
            "^(Matutino|Vespertino)$",
            ErrorMessage = "El turno debe ser Matutino o Vespertino"
        )]
        public string Turno { get; set; } = string.Empty;

        public string? UsuarioId { get; set; }

        public IdentityUser? Usuario { get; set; }
    }
}
