using System.Web.Mvc;
using System.Web.Routing;

namespace IsraelRailProject
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // alias: /WorkForm/Index -> UiController.WorkForm  (לטובת סימניות ישנות)
            routes.MapRoute(
                name: "WorkFormIndexAlias",
                url: "WorkForm/Index",
                defaults: new { controller = "Ui", action = "WorkForm" }
            );

            // קיצור נוח: /workform
            routes.MapRoute(
                name: "WorkForm",
                url: "workform",
                defaults: new { controller = "Ui", action = "WorkForm" }
            );

            // === ניהול סיכונים ===
            // alias: /Risks/Index -> UiController.Risks
            routes.MapRoute(
                name: "RisksIndexAlias",
                url: "Risks/Index",
                defaults: new { controller = "Ui", action = "Risks" }
            );

            // קיצור נוח: /risks
            routes.MapRoute(
                name: "RisksShort",
                url: "risks",
                defaults: new { controller = "Ui", action = "Risks" }
            );

            // ברירת מחדל: "/" וכל כתובת כללית
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Ui", action = "WorkForm", id = UrlParameter.Optional }
            );
        }
    }
}
