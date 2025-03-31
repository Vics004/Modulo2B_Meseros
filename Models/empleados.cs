using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Modulo2B_Meseros.Models
{
    public class empleados
    {
        [Key]
        public int empleadoId { get; set; }
        public string? nombre { get; set; }
        public string? contrasenia { get; set; }
        public string? rol { get; set; }
        public DateOnly fechaContratacion { get; set; }
    }
}
