using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Modulo2B_Meseros.Models
{
    public class item
    {
        [Key]
        public int itemId { get; set; }
        public decimal precio { get; set; }
        public string? nombre { get; set; }
        public int subCategoriaId { get; set; }
    }
}
