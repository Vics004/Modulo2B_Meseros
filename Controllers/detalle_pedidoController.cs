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
            //Guardar los items de la lista seleccionados en la tabla detalle_pedido
            //usar un foreach para iterar la lista seleccionados
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

            return RedirectToAction("Indexdetalle", new {id = pedidoId});
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
                            where dp.pedidoId == id && I.subCategoriaId == subid
                            select new
                            {
                                DetalleId = dp.dePedidoId,
                                Item = dp.itemId,
                                Nombre = I.nombre,
                                Precio = I.precio,
                                Estado = E.nombre

                            }).ToList();
            ViewData["listadetalle2"] = detalleP;


            return View();
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var detalleP = (from dp in _DulceSaborDbContexto.detalle_pedido
                            where dp.dePedidoId == id
                            select dp).FirstOrDefault();
            _DulceSaborDbContexto.detalle_pedido.Remove(detalleP);
            _DulceSaborDbContexto.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
/*
private readonly equiposContext _equiposContexto;
public equiposController(equiposContext equiposContexto)
{
    _equiposContexto = equiposContexto;
}
/// <summary>
///  Endpoint que retorna
/// </summary>
/// <returns></returns>
[HttpGet]
[Route("GetAll")]
public IActionResult Get()
{
    List<equipos> listadoEquipo = (from e in _equiposContexto.equipos
                                    select e).ToList();
    if (listadoEquipo.Count == 0)
    {
        return NotFound();
    }
    return Ok(listadoEquipo);
}
[HttpGet]
[Route("GetById/{id}")]
public IActionResult Get(int id)
{
    equipos? equipo = (from e in _equiposContexto.equipos
                       where e.id_equipos == id
                       select e).FirstOrDefault();
    if (equipo == null)
    {
        return NotFound();
    }
    return Ok(equipo);
}
[HttpGet]
[Route("Find/{filtro}")]
public IActionResult FindByDescription(string filtro)
{
    equipos? equipo = (from e in _equiposContexto.equipos
                       where e.descripcion.Contains(filtro)
                       select e).FirstOrDefault();
    if (equipo == null)
    {
        return NotFound();
    }
    return Ok(equipo);
}
[HttpPost]
[Route("Add")]
public IActionResult GuardarEquipo([FromBody] equipos equipo)
{
    try
    {
        _equiposContexto.equipos.Add(equipo);
        _equiposContexto.SaveChanges();
        return Ok(equipo);
    }//Esto es para entender mejor el error(solo da mas info)
    catch (DbUpdateException dbEx)
    {
        return BadRequest(dbEx.InnerException?.Message ?? dbEx.Message);
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}
[HttpPut]
[Route("actualizar/{id}")]
public IActionResult ActualizarEquipo(int id,  [FromBody] equipos equipoModificar)
{
    equipos? equipoActual = (from e in _equiposContexto.equipos
                             where e.id_equipos == id 
                             select e).FirstOrDefault();
    if ( equipoActual == null)
    {
        return NotFound();
    }
    equipoActual.nombre = equipoModificar.nombre;
    equipoActual.descripcion = equipoModificar.descripcion;
    equipoActual.marca_id = equipoModificar.marca_id;
    equipoActual.tipo_equipo_id = equipoModificar.tipo_equipo_id;
    equipoActual.anio_compra = equipoModificar.anio_compra;
    equipoActual.costo = equipoModificar.costo;

    _equiposContexto.Entry(equipoActual).State = EntityState.Modified;
    _equiposContexto.SaveChanges();
    return Ok(equipoModificar);
}
[HttpDelete]
[Route("eliminar/{id}")]
public IActionResult EliminarEquipo(int id)
{
    equipos? equipo = (from e in _equiposContexto.equipos
                        where e.id_equipos == id
                        select e).FirstOrDefault();
    if(equipo == null)
    {
        return NotFound();

    }
    _equiposContexto.equipos.Attach(equipo);
    _equiposContexto.equipos.Remove(equipo);
    _equiposContexto.SaveChanges();

    return Ok(equipo);
}
 
 */