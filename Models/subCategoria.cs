using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Modulo2B_Meseros.Models
{
    public class subCategoria
    {
        [Key]
        public int subCategoriaId { get; set; }
        public string? nombre { get; set; }
        public int categoriaId { get; set; }
    }
}
