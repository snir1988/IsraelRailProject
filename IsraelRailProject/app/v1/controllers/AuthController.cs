using System;
using System.Web.Mvc;
using IsraelRailProject.app.v1.DAL;
using IsraelRailProject.app.v1.models.dtos;

namespace IsraelRailProject.Controllers
{
    public class AuthController : Controller
    {
        [HttpGet]
        public ActionResult Login()
        {
            return View("~/Views/Auth/Login.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginDto model)
        {
            if (model == null || model.EmployeeId <= 0)
            {
                TempData["error"] = "הזינו מספר עובד.";
                return View("~/Views/Auth/Login.cshtml");
            }

            bool firstCandidate;
            var user = UserDAL.Authenticate(model.EmployeeId, model.Pass ?? "", out firstCandidate);

            if (user == null)
            {
                TempData["error"] = "מספר עובד או סיסמה שגויים (או שהמשתמש לא קיים).";
                return View("~/Views/Auth/Login.cshtml");
            }

            if (firstCandidate)
            {
                // כניסה ראשונה: למסך השלמת פרטים
                Session["FirstLoginUserId"] = user.Id;
                return RedirectToAction("FirstSetup");
            }

            // כניסה רגילה
            Session["CurrentUser"] = user;
            return RedirectToAction("Start");
        }

        [HttpGet]
        public ActionResult FirstSetup()
        {
            var uid = Session["FirstLoginUserId"] as int?;
            if (uid == null || uid <= 0) return RedirectToAction("Login");

            ViewBag.UserId = uid.Value;
            return View("~/Views/Auth/FirstSetup.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FirstSetup(string FullName, string Email, string Pass)
        {
            var uid = Session["FirstLoginUserId"] as int?;
            if (uid == null || uid <= 0) return RedirectToAction("Login");

            // אפשר להשאיר את הוולידציה כאן, ה-DAL גם בודק
            if (string.IsNullOrWhiteSpace(FullName) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Pass))
            {
                TempData["error"] = "יש למלא שם מלא, אימייל וסיסמה.";
                ViewBag.UserId = uid.Value;
                return View("~/Views/Auth/FirstSetup.cshtml");
            }

            try
            {
                string err;
                var ok = UserDAL.CompleteFirstLogin(uid.Value, FullName, Email, Pass, out err);
                if (!ok)
                {
                    TempData["error"] = err ?? "עדכון נכשל. נסה שוב.";
                    ViewBag.UserId = uid.Value;
                    return View("~/Views/Auth/FirstSetup.cshtml");
                }

                // אחרי השלמה — כניסה רגילה
                var justLogged = new CurrentUserDto
                {
                    Id = uid.Value,
                    FullName = (FullName ?? "").Trim(),
                    Role = "Employee"
                };
                Session.Remove("FirstLoginUserId");
                Session["CurrentUser"] = justLogged;

                return RedirectToAction("Start");
            }
            catch (Exception ex)
            {
                TempData["error"] = "שגיאה בעדכון: " + ex.Message;
                ViewBag.UserId = uid.Value;
                return View("~/Views/Auth/FirstSetup.cshtml");
            }
        }

        [HttpGet]
        public ActionResult Start()
        {
            var user = Session["CurrentUser"] as CurrentUserDto;
            if (user == null) return RedirectToAction("Login");

            if (string.Equals(user.Role, "Manager", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("WorkForm", "Ui");

            return RedirectToAction("Home", "Employee");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
