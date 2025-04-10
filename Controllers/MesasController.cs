using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modulo2B_Meseros.Models;
using static Modulo2B_Meseros.Servicios.AutenticationAttribute;

namespace Modulo2B_Meseros.Controllers
{
    public class MesasController : Controller
    {
        private readonly DulceSaborDbContext _db;

        public MesasController(DulceSaborDbContext db)
        {
            _db = db;
        }

        [Autenticacion]
        public IActionResult Index()
        {
            var mesasOcupadas = (from m in _db.mesas
                                join p in _db.pedido on m.mesaId equals p.mesaId into pedidosMesa
                                from p in pedidosMesa
                                    .Where(p => !p.estado) 
                                    .OrderByDescending(p => p.fechaHoraInicio) 
                                    .Take(1) 
                                    .DefaultIfEmpty()
                                where m.estado == "Ocupado" && p != null
                                select new
                                {
                                    mesaId = m.mesaId,
                                    numeroMesa = m.numeroMesa,
                                    capacidad = m.capacidad,
                                    pedidoId = p.pedidoId,
                                    fechaPedido = p.fechaHoraInicio 
                                }
                            ).OrderByDescending(x => x.fechaPedido) 
                            .ToList();

            ViewData["listadoMesasOcupadas"] = mesasOcupadas;

            return View();
        }

        [Autenticacion]
        public IActionResult Index1()
        {
            var mesasDisponibles = _db.mesas
                .Where(m => m.estado == "Disponible")
                .ToList();

            ViewData["listadoMesasDisponibles"] = mesasDisponibles;

            return View();
        }

        [Autenticacion]
        public IActionResult AbrirPedido(int mesaId)
        {
            var mesa = _db.mesas.Find(mesaId);
            ViewData["MesaSeleccionada"] = $"Mesa {mesa.numeroMesa} - Capacidad: {mesa.capacidad}";
            ViewData["MesaId"] = mesaId;
            return View();
        }

        [HttpPost]
        [Autenticacion]
        public IActionResult ConfirmarPedido(int mesaId)
        {
            var nuevoPedido = new pedido
            {
                mesaId = mesaId,
                empleadoId = HttpContext.Session.GetInt32("empleadoId") ?? 0,
                fechaHoraInicio = DateTime.Now,
                estado = false,
                tipoPedido = "Local",
                total = 0.00m
            };

            _db.pedido.Add(nuevoPedido);
            var mesa = _db.mesas.Find(mesaId);
            mesa.estado = "Ocupado";
            _db.SaveChanges();

            var pedidoDetalle = new
            {
                Pedido = nuevoPedido,
                NumeroMesa = mesa.numeroMesa,
                NombreEmpleado = _db.empleados.Find(nuevoPedido.empleadoId)?.nombre
            };

            ViewData["pedidoDetalle"] = pedidoDetalle;
            return RedirectToAction("Pedido", new { pedidoId = nuevoPedido.pedidoId });
        }

        [Autenticacion]
        public IActionResult Pedido(int pedidoId)
        {
            var pedido = (from p in _db.pedido
                          join m in _db.mesas on p.mesaId equals m.mesaId
                          join e in _db.empleados on p.empleadoId equals e.empleadoId
                          where p.pedidoId == pedidoId
                          select new
                          {
                              Pedido = p,
                              NumeroMesa = m.numeroMesa,
                              NombreEmpleado = e.nombre
                          }).FirstOrDefault();

            var detalleP = (from dp in _db.detalle_pedido
                            join i in _db.item on dp.itemId equals i.itemId
                            join ep in _db.estado_pedido on dp.estadoPedidoId equals ep.estadopedidoId
                            where dp.pedidoId == pedidoId
                            select new
                            {
                                PedidoId = dp.pedidoId,
                                DetalleId = dp.dePedidoId,
                                Item = dp.itemId,
                                Nombre = i.nombre,
                                Precio = i.precio,
                                Url_img = i.url_img,
                                subCategoria = i.subCategoriaId,
                                EstadoPedidoId = ep.estadopedidoId,
                                EstadoNombre = ep.nombre
                            }).ToList();

            ViewData["listadetalle"] = detalleP;
            ViewData["pedidoDetalle"] = pedido;

            return View();
        }

        [HttpPost]
        [Autenticacion]
        public IActionResult MarcarComoEntregado(int detallePedidoId)
        {

            var detalle = (from dp in _db.detalle_pedido
                           where dp.dePedidoId == detallePedidoId
                           select dp).FirstOrDefault();

            if (detalle != null)
            {
                detalle.estadoPedidoId = 4;
                _db.SaveChanges();
            }

            return RedirectToAction("Pedido", new { pedidoId = detalle.pedidoId });
        }

        [HttpPost]
        [Autenticacion]
        public IActionResult CerrarPedido(int pedidoId)
        {
            var pedido = (from p in _db.pedido
                          where p.pedidoId == pedidoId
                          select p).FirstOrDefault();

            if (pedido == null)
            {
                return NotFound();
            }

            var detalles = (from dp in _db.detalle_pedido
                            join ep in _db.estado_pedido on dp.estadoPedidoId equals ep.estadopedidoId
                            where dp.pedidoId == pedidoId
                            select new
                            {
                                EstadoId = ep.estadopedidoId
                            }).ToList();

            bool puedeCerrar = detalles.All(d => d.EstadoId == 4 || d.EstadoId == 5);

            if (!puedeCerrar)
            {
                TempData["Error"] = "No se puede cerrar el pedido. Hay ítems pendientes.";
                return RedirectToAction("Pedido", new { pedidoId });
            }

            pedido.fechaHoraFinal = DateTime.Now;
            pedido.empleadoIdFinal = HttpContext.Session.GetInt32("empleadoId") ?? 0;
            pedido.estado = true;

            var mesa = (from m in _db.mesas
                        where m.mesaId == pedido.mesaId
                        select m).FirstOrDefault();
            mesa.estado = "Disponible";

            _db.SaveChanges();

            return RedirectToAction("Index1");
        }

    }
}
