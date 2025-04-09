using Microsoft.AspNetCore.Mvc;
using Modulo2B_Meseros.Models;
using System.Diagnostics;
using static Modulo2B_Meseros.Servicios.AutenticationAttribute;

namespace Modulo2B_Meseros.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        [Autenticacion]
        public IActionResult Index()
        {
            return View();
        }
        [Autenticacion]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
