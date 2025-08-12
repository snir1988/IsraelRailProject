using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Http;
using IsraelRailProject.App_Start;

namespace IsraelRailProject
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register); // ← הוסף
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
