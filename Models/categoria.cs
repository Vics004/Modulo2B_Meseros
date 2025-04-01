using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Modulo2B_Meseros.Models
{
    public class categoria
    {
        [Key]
        public int categoriaId { get; set; }
        public string? nombre { get; set; }
    }
}
