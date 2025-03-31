using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Modulo2B_Meseros.Models
{
    public class mesas
    {
        [Key]
        public int mesaId { get; set; }
        public string? numeroMesa { get; set; }
        public string? capacidad { get; set; }
        public string? estado { get; set; }
    }
}
