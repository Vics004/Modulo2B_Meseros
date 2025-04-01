using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Modulo2B_Meseros.Models
{
    public class detalle_pedido
    {
        [Key]
        public int dePedidoId { get; set; }
        public string? comentario { get; set; }
        public int pedidoId { get; set; }
        public int itemId { get; set; }
        public int estadoPedidoId { get; set; }
    }
}
