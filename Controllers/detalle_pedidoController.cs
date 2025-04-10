using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Modulo2B_Meseros.Models;
using static Modulo2B_Meseros.Servicios.AutenticationAttribute;

namespace Modulo2B_Meseros.Controllers
{
    public class detalle_pedidoController : Controller
    {
        private readonly DulceSaborDbContext _DulceSaborDbContexto;
        public detalle_pedidoController(DulceSaborDbContext DulceSaborDbContexto)
        {
            _DulceSaborDbContexto = DulceSaborDbContexto;
        }

        [Autenticacion]
        public IActionResult Menu(int id, int? categoriaId, int? subCategoriaId)
        {
            ViewBag.pedido_id = id;
            var categorias = _DulceSaborDbContexto.categoria.ToList();
            var subCategorias = categoriaId.HasValue
                ? _DulceSaborDbContexto.subCategoria.Where(s => s.categoriaId == categoriaId).Distinct().ToList()
                : new List<subCategoria>();

            var items = subCategoriaId.HasValue
                ? _DulceSaborDbContexto.item.Where(i => i.subCategoriaId == subCategoriaId).Distinct().ToList()
                : new List<item>();

            
            var itemIds = items.Select(i => i.itemId).ToList();
            var promociones = _DulceSaborDbContexto.promociones
                .Where(p => itemIds.Contains(p.itemId) &&
                      p.fechaInicio <= DateTime.Now &&
                      p.fechaFin >= DateTime.Now)
                .ToList();

            
            ViewBag.Promociones = promociones;

           
            if (TempData["seleccionados"] != null)
            {
                var seleccionadosPrevios = TempData["seleccionados"] as List<int>;
                if (seleccionadosPrevios != null)
                {
                    items = items.Where(i => seleccionadosPrevios.Contains(i.itemId)).ToList();
                }
            }

            ViewBag.Categorias = categorias.Distinct().ToList();
            ViewBag.SubCategorias = subCategorias.Distinct().ToList();
            ViewBag.Items = items;

            return View();
        }

        [Autenticacion]
        [HttpPost]
		public IActionResult Siguiente(int pedidoId, List<int> seleccionados)
		{
			if (seleccionados == null || seleccionados.Count == 0)
			{
				TempData["Error"] = "Debe seleccionar al menos un producto.";
				return RedirectToAction("Menu");
			}

			TempData["seleccionados"] = seleccionados;

            decimal total = 0;

			foreach (var itemId in seleccionados)
			{
                var item = _DulceSaborDbContexto.item.Find(itemId);
                if (item != null)
                {
                    total += item.precio;
                    var detallePedido = new detalle_pedido
                    {
                        pedidoId = pedidoId,
                        itemId = itemId,
                        estadoPedidoId = 1 // Estado inicial
                    };
                    _DulceSaborDbContexto.detalle_pedido.Add(detallePedido);
                }
				
				_DulceSaborDbContexto.SaveChanges();
			}

            var pedido = _DulceSaborDbContexto.pedido.Find(pedidoId);
            if (pedido != null)
            {
                pedido.total += total;
            }

            _DulceSaborDbContexto.SaveChanges();

			return RedirectToAction("Pedido", "Mesas", new { pedidoId = pedidoId });
		}



		//Parte de Fer
		[Autenticacion]
		public IActionResult Edit(int id, int subid)
		{
			
			var detalleP = (from dp in _DulceSaborDbContexto.detalle_pedido
							join I in _DulceSaborDbContexto.item on dp.itemId equals I.itemId
							join E in _DulceSaborDbContexto.estado_pedido on dp.estadoPedidoId equals E.estadopedidoId
							join SC in _DulceSaborDbContexto.subCategoria on I.subCategoriaId equals SC.subCategoriaId
							where dp.pedidoId == id && I.subCategoriaId == subid && E.nombre != "Cancelado" 
							select new
							{
								DetalleId = dp.dePedidoId,
								Item = dp.itemId,
								Nombre = I.nombre,
								Precio = I.precio,
								Comentario = dp.comentario,
								Estado = E.nombre,
								SubId = I.subCategoriaId,
								SubCategoriaNombre = SC.nombre,
								Itemurl = I.url_img
							}).ToList();

			// Obtener todas las subcategorias y detalles
			var todosDetalles = (from dp in _DulceSaborDbContexto.detalle_pedido
								 join I in _DulceSaborDbContexto.item on dp.itemId equals I.itemId
								 join SC in _DulceSaborDbContexto.subCategoria on I.subCategoriaId equals SC.subCategoriaId
								 join E in _DulceSaborDbContexto.estado_pedido on dp.estadoPedidoId equals E.estadopedidoId
								 where dp.pedidoId == id && E.nombre != "Cancelado" 
								 select new
								 {
									 DetalleId = dp.dePedidoId,
									 SubCategoriaId = I.subCategoriaId,
									 SubCategoriaNombre = SC.nombre
								 }).ToList();

            // Agrupacion por subcategoría para menu lateral
            var detallesAgrupados = todosDetalles
				.GroupBy(d => d.SubCategoriaNombre)
				.ToDictionary(
					g => g.Key,
					g => g.Select(d => (object)new { d.DetalleId, d.SubCategoriaId, d.SubCategoriaNombre }).ToList()
				);



            ViewData["detallesAgrupados"] = detallesAgrupados;
			ViewData["listadetalle2"] = detalleP;
			ViewData["pedidoId"] = id;

			return View();
		}

		[Autenticacion]
        [HttpPost]
		public IActionResult Delete(int id)
		{
			var detalleP = (from dp in _DulceSaborDbContexto.detalle_pedido
							where dp.dePedidoId == id
							select dp).FirstOrDefault();

			if (detalleP != null)
			{
				var pedidoId = detalleP.pedidoId;

                var pedido  = (from p in _DulceSaborDbContexto.pedido
                               where p.pedidoId == pedidoId
                               select p).FirstOrDefault();

                //var pedido2 = _DulceSaborDbContexto.pedido.FirstOrDefault(p => p.pedidoId == pedidoId);

                //Buscamos el item para actualizar el total del pedido

                if (pedido != null)
                {
                    var item = (from i in _DulceSaborDbContexto.item
                                where i.itemId == detalleP.itemId
                                select i).FirstOrDefault();

                    //var item2 = _DulceSaborDbContexto.item.FirstOrDefault(i => i.itemId == detalleP.itemId);
                    if (item != null)
                    {
                        decimal subtotal = item.precio;
                        pedido.total -= subtotal;
                    }
                }

                //Buscamos cual Id tiene como estado "cancelado"

                detalleP.estadoPedidoId = (from ep in _DulceSaborDbContexto.estado_pedido
                                            where ep.nombre == "Cancelado"
                                            select ep.estadopedidoId
                                            ).FirstOrDefault();

                /*detalleP.estadoPedidoId = _DulceSaborDbContexto.estado_pedido
				.Where(ep => ep.nombre == "Cancelado")
				.Select(ep => ep.estadopedidoId)
				.FirstOrDefault();*/

                _DulceSaborDbContexto.SaveChanges();
				return RedirectToAction("Pedido", "Mesas", new { pedidoId = pedidoId });
			}
			return NotFound();
		}

        [Autenticacion]
        [HttpPost]
        public IActionResult Actualizar(int detalleId, int subID, [Bind("dePedidoId, comentario")] detalle_pedido detPedido)
        {
            var pedidoExistente = (from dp in _DulceSaborDbContexto.detalle_pedido
                                   where dp.dePedidoId == detalleId
                                   select dp).FirstOrDefault();
            if (pedidoExistente == null)
            {
                return NotFound();
            }
            int id = pedidoExistente.pedidoId, subid = subID;

            pedidoExistente.comentario = detPedido.comentario;

            _DulceSaborDbContexto.Entry(pedidoExistente).State = EntityState.Modified;
            _DulceSaborDbContexto.SaveChanges();
            return RedirectToAction("Edit", new { id = id, subid = subid });
        }
        //Metodo para retornar a la vista de Edit
        [Autenticacion]
        [HttpPost]
		public IActionResult Return(int iddet)
		{
			var pedidoExistente = (from dp in _DulceSaborDbContexto.detalle_pedido
								   where dp.dePedidoId == iddet
								   select dp).FirstOrDefault();
			if (pedidoExistente == null)
			{
				return NotFound();
			}
			int id = pedidoExistente.pedidoId;
			return RedirectToAction("Pedido", "Mesas", new { pedidoId = id });
		}
	}
}
