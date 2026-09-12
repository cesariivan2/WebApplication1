using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class VotoPropuesta
    {
        public int Id { get; set; }

        public int PropuestaComiteId { get; set; }

        public string UsuarioId { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Opcion { get; set; } = string.Empty;

        public DateTime FechaVoto { get; set; } = DateTime.Now;

        public PropuestaComite? PropuestaComite { get; set; }

        public IdentityUser? Usuario { get; set; }
    }
}