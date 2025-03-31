using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Modulo2B_Meseros.Models
{
    public class estado_pedido
    {
        [Key]
        public int estadopedidoId { get; set; }
        public string? nombre { get; set; }
    }
}
