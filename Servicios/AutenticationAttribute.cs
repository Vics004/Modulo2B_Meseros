using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Modulo2B_Meseros.Servicios
{
    public class AutenticationAttribute
    {
        public class AutenticacionAttribute : ActionFilterAttribute
        {
            public override void OnActionExecuting(ActionExecutingContext context)
            {
                var usuarioId = context.HttpContext.Session.GetInt32("empleadoId");

                if (usuarioId == null)
                {
                    context.Result = new RedirectToActionResult("Autenticar", "login", null);
                }
                base.OnActionExecuting(context);
            }
        }
    }
}
