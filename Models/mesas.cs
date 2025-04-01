using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Modulo2B_Meseros.Models
{
    public class mesas
    {
        [Key]
        public int mesaId { get; set; }
        public int numeroMesa { get; set; }
        public int capacidad { get; set; }
        public string? estado { get; set; }
    }
}
