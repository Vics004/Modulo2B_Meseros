using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Modulo2B_Meseros.Models
{
    public class pedido
    {
        [Key]
        public int pedidoId { get; set; }
        public int mesaId { get; set; }
        public int empleadoId { get; set; }
        public int empleadoIdFinal { get; set; }
        public DateTime? fechaHoraInicio { get; set; }
        public DateTime? fechaHoraFinal { get; set; }
        public bool estado { get; set; }
        public decimal total { get; set; }
    }
}
