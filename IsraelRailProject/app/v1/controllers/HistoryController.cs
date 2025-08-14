using System;
using System.Web.Mvc;
using IsraelRailProject.app.v1.DAL;
using IsraelRailProject.app.v1.models.dtos;

namespace IsraelRailProject.Controllers
{
    public class HistoryController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            // מניח שיש לך את המתודה שמחזירה רשימת היסטוריה
            var list = WorkFormHistoryDAL.GetLatest();
            return View("~/Views/History/Index.cshtml", list);
        }

        [HttpGet]
        public ActionResult Details(int id)
        {
            var cu = Session["CurrentUser"] as CurrentUserDto;
            if (cu == null) return RedirectToAction("Login", "Auth");

            bool isManager = string.Equals(cu.Role, "Manager", StringComparison.OrdinalIgnoreCase);
            bool canView = isManager || WorkFormDetailsDAL.IsEmployeeAssigned(id, cu.Id);

            if (!canView) return RedirectToAction("Home", "Employee");

            var dto = WorkFormDetailsDAL.GetDetails(id);
            if (dto == null) return HttpNotFound();

            return View("~/Views/History/Details.cshtml", dto);
        }
    }
}
