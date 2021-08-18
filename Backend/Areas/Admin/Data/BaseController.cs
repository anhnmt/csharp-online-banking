using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using OnlineBanking.DAL;

namespace Backend.Areas.Admin.Data
{
    public class BaseController : Controller
    {
        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            var routingValues = filterContext.RouteData.Values;
            var currentArea = filterContext.RouteData.DataTokens["area"] ?? string.Empty;
            var currentController = (string) routingValues["controller"] ?? string.Empty;
            var currentAction = (string) routingValues["action"] ?? string.Empty;

            string[] routeAnnonymous = {"Login", "Register", "CheckLogin", "CheckRegister"};
            if (filterContext.HttpContext.Session["user"] == null && !routeAnnonymous.Contains(currentAction))
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new RouteValueDictionary(new
                    {
                        action = "Login",
                        controller = "Home",
                        area = ""
                    }
                )));
            }

            var obj = (Accounts) filterContext.HttpContext.Session["user"];

            if (obj != null && routeAnnonymous.Contains(currentAction))
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new RouteValueDictionary(new
                    {
                        action = "Index",
                        controller = "Home",
                        area = ""
                    }
                )));
            }
            
            if (obj != null && obj.RoleId == 3 && (string) currentArea == "Admin")
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new RouteValueDictionary(new
                    {
                        action = "Index",
                        controller = "Home",
                        area = ""
                    }
                )));
            }
        }
    }
}