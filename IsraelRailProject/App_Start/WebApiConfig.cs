using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace IsraelRailProject.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // בזמן דיבוג – להציג פירוט מלא של שגיאות מהשרת
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            // Attribute routing
            config.MapHttpAttributeRoutes();

            // Fallback route (לא חובה אבל לא מזיק)
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // העדפת JSON (ולא XML) + פורמט תאריכים תקני
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            var json = config.Formatters.JsonFormatter;
            json.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            json.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            json.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
        }
    }
}
