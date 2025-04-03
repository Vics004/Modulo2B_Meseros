using Microsoft.EntityFrameworkCore;

namespace Modulo2B_Meseros.Models
{
    public class DulceSaborDbContext : DbContext
    {
        public DulceSaborDbContext(DbContextOptions<DulceSaborDbContext> options) : base(options)
        {

        }

        public DbSet<detalle_pedido> detalle_pedido { get; set; }
        public DbSet<empleados> empleados { get; set; }
        public DbSet<estado_pedido> estado_pedido { get; set; }
        public DbSet<mesas> mesas { get; set; }
        public DbSet<pedido> pedido { get; set; }
        public DbSet<categoria> categoria { get; set; }
        public DbSet<subCategoria> subCategoria { get; set; }
        public DbSet<promociones> promociones { get; set; }
        public DbSet<item> item { get; set; }
    }
}
