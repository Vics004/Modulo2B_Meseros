using Microsoft.AspNetCore.Mvc;
using Modulo2B_Meseros.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static Modulo2B_Meseros.Servicios.AutenticationAttribute;

namespace Modulo2B_Meseros.Controllers
{
    public class loginController : Controller
    {
        private readonly ILogger<loginController> _logger;
        private readonly DulceSaborDbContext _DulceSaborDbContexto;
        public loginController(ILogger<loginController> logger, DulceSaborDbContext DulceSaborDbContexto)
        {
            _DulceSaborDbContexto = DulceSaborDbContexto;
            _logger = logger;
        }

        [Autenticacion]
        public IActionResult Index()
        {
            var empleadoId = HttpContext.Session.GetInt32("empleadoId");
            var tipoUsuario = HttpContext.Session.GetString("TipoUsuario");
            var nombreUsuario = HttpContext.Session.GetString("Nombre");
            if (empleadoId == null)
            {
                return RedirectToAction("Autenticar", "login");
            }
            ViewBag.nombre = nombreUsuario;
            ViewData["tipoUsuario"] = tipoUsuario;
            return View();
        }

        public IActionResult Autenticar()
        {
            ViewData["ErrorMessage"] = "";
            return View();

        }

        [HttpPost]
        public async Task<IActionResult> Autenticar(string txtUsuario, string txtClave)
        {
            var usuario = (from u in _DulceSaborDbContexto.empleados
                           where u.nombre == txtUsuario
                           && u.contrasenia == txtClave
                           && u.rol.Equals("Mesero") 
                           select u).FirstOrDefault();

            if (usuario != null)
            {
                HttpContext.Session.SetInt32("empleadoId", usuario.empleadoId);
                HttpContext.Session.SetString("TipoUsuario", usuario.rol);
                HttpContext.Session.SetString("Nombre", usuario.nombre);

                return RedirectToAction("Index", "login");
            }
            ViewData["ErrorMessage"] = "Error, usuario inválido";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
