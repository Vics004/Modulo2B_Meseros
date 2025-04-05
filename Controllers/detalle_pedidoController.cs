using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Modulo2B_Meseros.Models;

namespace Modulo2B_Meseros.Controllers
{
    public class detalle_pedidoController : Controller
    {
        private readonly DulceSaborDbContext _DulceSaborDbContexto;
        public detalle_pedidoController(DulceSaborDbContext DulceSaborDbContexto)
        {
            _DulceSaborDbContexto = DulceSaborDbContexto;
        }

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

            // Mantener la selección previa al usar TempData
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
            /*var categorias = _DulceSaborDbContexto.categoria.Distinct().ToList();
            return View(categorias);*/
        }

		[HttpPost]
		public IActionResult Siguiente(int pedidoId, List<int> seleccionados)
		{
			if (seleccionados == null || seleccionados.Count == 0)
			{
				TempData["Error"] = "Debe seleccionar al menos un producto.";
				return RedirectToAction("Menu");
			}

			TempData["seleccionados"] = seleccionados;

			foreach (var itemId in seleccionados)
			{
				var detallePedido = new detalle_pedido
				{
					pedidoId = pedidoId,
					itemId = itemId,
					estadoPedidoId = 1 // Estado inicial
				};
				_DulceSaborDbContexto.detalle_pedido.Add(detallePedido);
				_DulceSaborDbContexto.SaveChanges();
			}
			return RedirectToAction("Pedido", "Mesas", new { pedidoId = pedidoId });
		}


		// Mostrar Subcategorías de una Categoría
		public IActionResult SubCategorias(int categoriaId)
		{
			var subCategorias = _DulceSaborDbContexto.subCategoria
				.Where(s => s.categoriaId == categoriaId)
				.ToList();

			return PartialView("_SubCategorias", subCategorias); //actualizar la sección dinámica
		}

		// Mostrar Ítems de una Subcategoría
		public IActionResult Items(int subCategoriaId)
		{
			var items = _DulceSaborDbContexto.item
				.Where(i => i.subCategoriaId == subCategoriaId)
				.ToList();

			return PartialView("_Items", items);
		}

		// GET: detalle_pedidoController
		public IActionResult Index()
        {
            var pedido = (from p in _DulceSaborDbContexto.pedido
                          select new
                          {
                              MesaID = p.mesaId,
                              Mesa = "Mesa " + p.mesaId,
                              PedidoId = p.pedidoId

                          }).ToList();



            ViewData["listapedido"] = pedido;
            return View();
        }

        public IActionResult Indexdetalle(int id)
        {

            var detalleP = (from dp in _DulceSaborDbContexto.detalle_pedido
                            join I in _DulceSaborDbContexto.item on dp.itemId equals I.itemId
                            where dp.pedidoId == id
                            select new
                            {
                                PedidoId = dp.pedidoId,
                                DetalleId = dp.dePedidoId,
                                Item = dp.itemId,
                                Nombre = I.nombre,
                                subCategoria = I.subCategoriaId

                            }).ToList();

           

            ViewData["listadetalle"] = detalleP;

            return View();
        }


        //Parte de Fer
        public IActionResult Edit(int id, int subid)
        {
            var detalleP = (from dp in _DulceSaborDbContexto.detalle_pedido
                            join I in _DulceSaborDbContexto.item on dp.itemId equals I.itemId
                            join E in _DulceSaborDbContexto.estado_pedido on dp.estadoPedidoId equals E.estadopedidoId
                            join SC in _DulceSaborDbContexto.subCategoria on I.subCategoriaId equals SC.subCategoriaId
                            where dp.pedidoId == id && I.subCategoriaId == subid
                            select new
                            {
                                DetalleId = dp.dePedidoId,
                                Item = dp.itemId,
                                Nombre = I.nombre,
                                Precio = I.precio,
                                Comentario = dp.comentario,
                                Estado = E.nombre,
                                SubId = I.subCategoriaId,
                                SubCategoriaNombre = SC.nombre
                            }).ToList();

            // Obtener todas las subcategorías y detalles para el menú lateral
            var todosDetalles = (from dp in _DulceSaborDbContexto.detalle_pedido
                                 join I in _DulceSaborDbContexto.item on dp.itemId equals I.itemId
                                 join SC in _DulceSaborDbContexto.subCategoria on I.subCategoriaId equals SC.subCategoriaId
                                 where dp.pedidoId == id
                                 select new
                                 {
                                     DetalleId = dp.dePedidoId,
                                     Item = dp.itemId,
                                     Nombre = I.nombre,
                                     SubCategoriaId = I.subCategoriaId,
                                     SubCategoriaNombre = SC.nombre
                                 }).ToList();

            // Agrupar por subcategoría para el menú lateral
            var detallesAgrupados = todosDetalles
                .GroupBy(d => d.SubCategoriaNombre)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(d => (object)new { d.DetalleId, d.Item, d.Nombre, d.SubCategoriaId, d.SubCategoriaNombre }).ToList()
                );

            ViewData["detallesAgrupados"] = detallesAgrupados;
            ViewData["listadetalle2"] = detalleP;
            ViewData["pedidoId"] = id;


            return View();
        }
        [HttpPost]
		public IActionResult Delete(int id)
		{
			var detalleP = (from dp in _DulceSaborDbContexto.detalle_pedido
							where dp.dePedidoId == id
							select dp).FirstOrDefault();

			if (detalleP != null)
			{
				var pedidoId = detalleP.pedidoId;

				_DulceSaborDbContexto.detalle_pedido.Remove(detalleP);
				_DulceSaborDbContexto.SaveChanges();
				return RedirectToAction("Pedido", "Mesas", new { pedidoId = pedidoId });
			}
			return NotFound();
		}

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
