using System.Web.Mvc;

namespace IsraelRailProject.Controllers
{
    // קונטרולר MVC – מיועד למסכי ה־UI של WorkForms
    // ה־View (AddEdit.cshtml) משתמש ב־AJAX כדי לגשת ל־API שב־WorkFormController (api/v1/workforms)
    public class WorkFormsController : Controller
    {
        // GET: /WorkForms/AddEdit   או   /WorkForms/AddEdit?id=123
        [HttpGet]
        public ActionResult AddEdit(int? id)
        {
            // לא נטען נתונים כאן – ה־JS ב־AddEdit יקרא ל־/api/v1/workforms/{id}
            // אם יש id, וימלא את השדות
            return View();
        }

        // GET: /WorkForms/Create
        [HttpGet]
        public ActionResult Create()
        {
            return View("AddEdit");
        }

        // GET: /WorkForms/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            return RedirectToAction("AddEdit", new { id });
        }

        // דף ראשי – רשימת טפסים (נכין בהמשך אם תרצה)
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
    }
}
