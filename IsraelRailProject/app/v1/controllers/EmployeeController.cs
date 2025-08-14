using System.Web.Mvc;
using IsraelRailProject.app.v1.models.dtos;
using IsraelRailProject.app.v1.DAL;

namespace IsraelRailProject.Controllers
{
    public class EmployeeController : Controller
    {
        [HttpGet]
        public ActionResult Home()
        {
            var user = Session["CurrentUser"] as CurrentUserDto;
            if (user == null) return RedirectToAction("Login", "Auth");

            ViewBag.UserName = user.FullName;
            ViewBag.UserId = user.Id;

            // שולף טפסים שממתינים לחתימה של העובד הנוכחי
            var list = WorkFormSignDAL.GetPendingForEmployee(user.Id);
            return View("~/Views/Employee/Home.cshtml", list);
        }

        // אופציונלי: פעולה לסימון חתימה (נשתמש בה בהמשך אם תרצה).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Sign(int id)
        {
            var user = Session["CurrentUser"] as CurrentUserDto;
            if (user == null) return RedirectToAction("Login", "Auth");

            var ok = WorkFormSignDAL.Sign(id, user.Id);
            TempData[ok ? "success" : "error"] = ok ? "נחתם בהצלחה." : "טופס לא נמצא או כבר חתום.";
            return RedirectToAction("Home");
        }
    }
}

