using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Modulo2B_Meseros.Models
{
    public class promociones
    {
        [Key]
        public int promocionId { get; set; }
        public DateTime? fechaInicio { get; set; }
        public DateTime? fechaFin { get; set; }
        public string? restricciones { get; set; }
        public int itemId { get; set; }
    }
}
