using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class PropuestaComite
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Descripcion { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime? FechaCierre { get; set; }

        [Required]
        public string Estado { get; set; } = "Abierta";

        public string? CreadoPorUsuarioId { get; set; }

        public IdentityUser? CreadoPorUsuario { get; set; }
    }
}