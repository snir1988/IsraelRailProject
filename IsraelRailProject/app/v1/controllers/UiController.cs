// Controllers/UiController.cs
using System;
using System.Data.SqlClient;
using System.Web.Mvc;
using IsraelRailProject.app.v1.models.dtos;
using IsraelRailProject.app.v1.DAL;

namespace IsraelRailProject.Controllers
{
    public class UiController : Controller
    {
        private CurrentUserDto CurrentUser => Session["CurrentUser"] as CurrentUserDto;
        private bool IsManager => CurrentUser != null &&
            string.Equals(CurrentUser.Role, "Manager", StringComparison.OrdinalIgnoreCase);

        [HttpGet]
        public ActionResult WorkForm()
        {
            if (CurrentUser == null) return RedirectToAction("Login", "Auth");
            if (!IsManager) return RedirectToAction("Home", "Employee");

            ViewBag.ManagerId = CurrentUser.Id; // כדי למלא אוטומטית בטופס
            return View("~/Views/WorkForm/Index.cshtml");
        }

        [HttpGet]
        public ActionResult Employees()
        {
            if (CurrentUser == null) return RedirectToAction("Login", "Auth");
            if (!IsManager) return RedirectToAction("Home", "Employee");

            return RedirectToAction("Index", "Employees"); // הקונטרולר שמחזיר מודל
        }

        [HttpGet]
        public ActionResult Risks()
        {
            if (CurrentUser == null) return RedirectToAction("Login", "Auth");
            if (!IsManager) return RedirectToAction("Home", "Employee");

            return View("~/Views/Risks/Index.cshtml");
        }

        [HttpGet]
        public ActionResult History()
        {
            if (CurrentUser == null) return RedirectToAction("Login", "Auth");
            if (!IsManager) return RedirectToAction("Home", "Employee");

            return RedirectToAction("Index", "History");
        }

        // =========================
        // RISK CRUD – Fallback לנתיבים ישנים (/Ui/RiskCreate וכו')
        // =========================

        // הוספת סיכון: טבלת RiskItems כוללת רק (Id, Name)
        [HttpPost]
        public ActionResult RiskCreate(string Name, int Status = 1, string Description = null)
        {
            if (CurrentUser == null) return RedirectToAction("Login", "Auth");
            if (!IsManager) return RedirectToAction("Home", "Employee");

            if (string.IsNullOrWhiteSpace(Name))
            {
                TempData["error"] = "יש להזין שם סיכון.";
                return RedirectToAction("Risks");
            }

            try
            {
                using (var conn = Db.GetConnection())
                using (var cmd = new SqlCommand(@"INSERT INTO RiskItems (Name) VALUES (@n);", conn))
                {
                    cmd.Parameters.AddWithValue("@n", Name.Trim());
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                TempData["success"] = "הסיכון נוסף בהצלחה.";
            }
            catch (SqlException ex)
            {
                TempData["error"] = "שגיאת מסד נתונים בהוספת סיכון: " + ex.Message;
            }
            catch (Exception ex)
            {
                TempData["error"] = "שגיאה בהוספת סיכון: " + ex.Message;
            }

            return RedirectToAction("Risks");
        }

        // עריכת סיכון: עדכון שם בלבד
        [HttpPost]
        public ActionResult RiskEdit(int id, string Name, int Status = 1, string Description = null)
        {
            if (CurrentUser == null) return RedirectToAction("Login", "Auth");
            if (!IsManager) return RedirectToAction("Home", "Employee");

            if (id <= 0 || string.IsNullOrWhiteSpace(Name))
            {
                TempData["error"] = "נתוני עריכה אינם תקינים.";
                return RedirectToAction("Risks");
            }

            try
            {
                using (var conn = Db.GetConnection())
                using (var cmd = new SqlCommand(@"
                    UPDATE RiskItems
                       SET Name = @n
                     WHERE Id = @id;", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@n", Name.Trim());
                    conn.Open();
                    var rows = cmd.ExecuteNonQuery();
                    TempData[rows > 0 ? "success" : "error"] = rows > 0 ? "הסיכון עודכן." : "סיכון לא נמצא.";
                }
            }
            catch (SqlException ex)
            {
                TempData["error"] = "שגיאת מסד נתונים בעדכון: " + ex.Message;
            }
            catch (Exception ex)
            {
                TempData["error"] = "שגיאה בעדכון סיכון: " + ex.Message;
            }

            return RedirectToAction("Risks");
        }

        // מחיקת סיכון
        [HttpPost]
        public ActionResult RiskDelete(int id)
        {
            if (CurrentUser == null) return RedirectToAction("Login", "Auth");
            if (!IsManager) return RedirectToAction("Home", "Employee");

            if (id <= 0)
            {
                TempData["error"] = "מזהה מחיקה אינו תקין.";
                return RedirectToAction("Risks");
            }

            try
            {
                using (var conn = Db.GetConnection())
                using (var cmd = new SqlCommand("DELETE FROM RiskItems WHERE Id = @id;", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    conn.Open();
                    var rows = cmd.ExecuteNonQuery();
                    TempData[rows > 0 ? "success" : "error"] = rows > 0 ? "הסיכון נמחק." : "סיכון לא נמצא.";
                }
            }
            catch (SqlException ex)
            {
                TempData["error"] = "שגיאת מסד נתונים במחיקה: " + ex.Message;
            }
            catch (Exception ex)
            {
                TempData["error"] = "שגיאה במחיקת סיכון: " + ex.Message;
            }

            return RedirectToAction("Risks");
        }
    }
}
