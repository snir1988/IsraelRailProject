// Controllers/UiController.cs
using System.Web.Mvc;

namespace IsraelRailProject.Controllers
{
    public class UiController : Controller
    {
        [HttpGet]
        public ActionResult WorkForm() => View("~/Views/WorkForm/Index.cshtml");

        [HttpGet]
        public ActionResult Employees() => View("~/Views/Employees/Index.cshtml"); // דף "עובדים"

        [HttpGet]
        public ActionResult Risks() => View("~/Views/Risks/Index.cshtml"); // דף "ניהול סיכונים"

        [HttpGet]
        public ActionResult History() => View("~/Views/History/Index.cshtml"); // דף "היסטוריה"
    }
}
