using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;                   


namespace IsraelRailProject.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            // נתיב דיפולטי (לא חובה כשעובדים רק עם Attribute Routing, אבל לא מזיק)
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}