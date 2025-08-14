using System;
using System.Data.SqlClient;
using System.Web.Mvc;
using IsraelRailProject.app.v1.DAL;
using IsraelRailProject.app.v1.models.dtos;

namespace IsraelRailProject.Controllers
{
    public class EmployeesController : Controller
    {
        // === הרשאות בסיסיות: נשלפות מה-Session ===
        private CurrentUserDto CurrentUser => Session["CurrentUser"] as CurrentUserDto;
        private bool IsManager => CurrentUser != null &&
            string.Equals(CurrentUser.Role, "Manager", StringComparison.OrdinalIgnoreCase);

        // רשימה
        public ActionResult Index()
        {
            if (CurrentUser == null) return RedirectToAction("Login", "Auth");
            if (!IsManager) return RedirectToAction("Home", "Employee");

            var list = UserDAL.GetEmployees();
            // חשוב: הנתיב ל-View בתיקייה Employee (יחיד)
            return View("~/Views/Employee/Index.cshtml", list);
        }

        // הוספה
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EmployeeDto model)
        {
            if (CurrentUser == null) return RedirectToAction("Login", "Auth");
            if (!IsManager) return RedirectToAction("Home", "Employee");

            try
            {
                if (model == null ||
                    string.IsNullOrWhiteSpace(model.FullName) ||
                    string.IsNullOrWhiteSpace(model.Email) ||
                    string.IsNullOrWhiteSpace(model.Pass))
                {
                    TempData["error"] = "שם מלא, אימייל וסיסמה — שדות חובה.";
                    return RedirectToAction("Index");
                }

                var id = UserDAL.AddEmployee(new EmployeeDto
                {
                    FullName = model.FullName.Trim(),
                    Email = model.Email.Trim(),
                    Pass = model.Pass.Trim()
                });

                TempData["success"] = $"העובד נוסף (#{id}).";
                return RedirectToAction("Index");
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                TempData["error"] = "האימייל כבר קיים במערכת.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = "שגיאה בהוספת עובד: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // עריכה
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EmployeeDto model)
        {
            if (CurrentUser == null) return RedirectToAction("Login", "Auth");
            if (!IsManager) return RedirectToAction("Home", "Employee");

            try
            {
                if (model == null || model.Id <= 0 ||
                    string.IsNullOrWhiteSpace(model.FullName) ||
                    string.IsNullOrWhiteSpace(model.Email))
                {
                    TempData["error"] = "נתוני עריכה אינם תקינים.";
                    return RedirectToAction("Index");
                }

                var ok = UserDAL.UpdateEmployee(new EmployeeDto
                {
                    Id = model.Id,
                    FullName = model.FullName.Trim(),
                    Email = model.Email.Trim(),
                    // Pass אופציונלי בעריכה
                    Pass = string.IsNullOrWhiteSpace(model.Pass) ? null : model.Pass.Trim()
                });

                TempData[ok ? "success" : "error"] = ok ? "העובד עודכן." : "לא נמצא עובד לעדכון.";
                return RedirectToAction("Index");
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                TempData["error"] = "האימייל כבר קיים במערכת.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = "שגיאה בעדכון עובד: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // מחיקה
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (CurrentUser == null) return RedirectToAction("Login", "Auth");
            if (!IsManager) return RedirectToAction("Home", "Employee");

            try
            {
                if (id <= 0)
                {
                    TempData["error"] = "מזהה למחיקה אינו תקין.";
                    return RedirectToAction("Index");
                }

                var ok = UserDAL.DeleteEmployee(id);
                TempData[ok ? "success" : "error"] = ok ? "העובד נמחק." : "לא נמצא עובד למחיקה.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = "שגיאה במחיקת עובד: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}
