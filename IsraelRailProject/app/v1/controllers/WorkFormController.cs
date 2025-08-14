// קובץ: app/v1/controllers/WorkFormController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using IsraelRailProject.app.v1.DAL;
using IsraelRailProject.app.v1.models;
using IsraelRailProject.app.v1.models.dtos;

namespace IsraelRailProject.app.v1.controllers
{
    [RoutePrefix("api/v1/workforms")]
    public class WorkFormController : ApiController
    {
        // --------------------------------------------
        // POST /api/v1/workforms  (יצירת טופס + שיוך עובדים/סיכונים)
        // --------------------------------------------
        [HttpPost, Route("")]
        public IHttpActionResult Create([FromBody] WorkFormCreateDto dto)
        {
            if (dto == null) return BadRequest("Body is required");
            if (dto.ManagerId <= 0) return BadRequest("ManagerId חובה וחייב להיות גדול מ-0");
            if (string.IsNullOrWhiteSpace(dto.Site)) return BadRequest("Site הוא שדה חובה");
            if (string.IsNullOrWhiteSpace(dto.WorkType)) return BadRequest("WorkType הוא שדה חובה");

            try
            {
                // בדיקת קיום מנהל
                if (!UserDAL.UserExists(dto.ManagerId))
                    return Content(HttpStatusCode.BadRequest,
                        new { ok = false, message = "ManagerId לא קיים בטבלת Users" });

                // נורמליזציה של זמן ל-UTC
                var workUtc = dto.WorkDateTime.Kind == DateTimeKind.Utc
                                ? dto.WorkDateTime
                                : dto.WorkDateTime.ToUniversalTime();

                // יצירת הרשומה הראשית
                var wf = new WorkForm
                {
                    ManagerId = dto.ManagerId,
                    Site = dto.Site.Trim(),
                    WorkDateTime = workUtc,
                    WorkType = dto.WorkType.Trim()
                };
                var newId = WorkFormDAL.Create(wf);

                // שיוך עובדים (רק קיימים/בתפקיד Employee)
                var validEmpIds = new List<int>();
                var invalidEmpIds = new List<int>();
                if (dto.EmployeeIds != null && dto.EmployeeIds.Count > 0)
                {
                    foreach (var empId in dto.EmployeeIds.Where(i => i > 0).Distinct())
                    {
                        if (UserDAL.IsEmployeeId(empId)) validEmpIds.Add(empId);
                        else invalidEmpIds.Add(empId);
                    }
                }

                if (invalidEmpIds.Count > 0)
                {
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        ok = false,
                        message = "יש עובדים שלא קיימים כ-Employee בטבלת Users",
                        invalidIds = invalidEmpIds
                    });
                }

                foreach (var empId in validEmpIds)
                    WorkFormEmployeeDAL.Add(newId, empId);

                // ✅ שיוך סיכונים לטופס החדש
                if (dto.RiskItemIds != null && dto.RiskItemIds.Count > 0)
                {
                    foreach (var rid in dto.RiskItemIds.Where(x => x > 0).Distinct())
                        WorkFormRiskItemDAL.Add(newId, rid);
                    // לחלופין, אם קיימת אצלך מתודה מרוכזת:
                    // WorkFormDAL.SetRiskItems(newId, dto.RiskItemIds);
                }

                // נחזיר פרטי בסיס
                var created = WorkFormDAL.GetById(newId);
                return Ok(new { id = created.Id, version = created.Version });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError,
                    new { ok = false, error = ex.ToString() });
            }
        }

        // --------------------------------------------
        // GET /api/v1/workforms?managerId=1
        // --------------------------------------------
        [HttpGet, Route("")]
        public IHttpActionResult ByManager([FromUri] int? managerId = null)
        {
            if (managerId == null || managerId <= 0)
                return BadRequest("יש לספק managerId תקין בשורת הכתובת (?managerId=)");

            try
            {
                var list = WorkFormDAL.GetByManager(managerId.Value) ?? new List<WorkForm>();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError,
                               new { ok = false, error = ex.Message });
            }
        }

        // --------------------------------------------
        // GET /api/v1/workforms/{id}  (טופס בודד)
        // --------------------------------------------
        [HttpGet, Route("{id:int}", Name = "GetWorkFormById")]
        public IHttpActionResult GetOne(int id)
        {
            var wf = WorkFormDAL.GetById(id);
            if (wf == null) return NotFound();
            return Ok(wf);
        }

        // --------------------------------------------
        // POST /api/v1/workforms/{id}/send  (פתיחה לחתימות)
        // --------------------------------------------
        [HttpPost, Route("{id:int}/send")]
        public IHttpActionResult OpenForSign(int id)
        {
            try
            {
                var wf = WorkFormDAL.GetById(id);
                if (wf == null) return NotFound();

                WorkFormDAL.OpenForSign(id);
                return Ok(new { message = "הטופס נפתח לחתימות", id });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError,
                               new { ok = false, error = ex.Message });
            }
        }

        // --------------------------------------------
        // GET /api/v1/workforms/{id}/status
        // --------------------------------------------
        [HttpGet, Route("{id:int}/status")]
        public IHttpActionResult Status(int id)
        {
            var wf = WorkFormDAL.GetById(id);
            if (wf == null) return NotFound();

            var assignedIds = WorkFormEmployeeDAL.GetAssignedIds(id);          // מי אמור לחתום
            var signedIds = SignatureDAL.GetSignedEmployeeIds(id, wf.Version); // מי חתם בפועל
            var total = assignedIds.Count > 0 ? (int?)assignedIds.Count : null;
            var pendingIds = assignedIds.Except(signedIds).ToList();

            return Ok(new
            {
                id = wf.Id,
                version = wf.Version,
                status = wf.Status,
                total,
                signedCount = signedIds.Count,
                pendingCount = total.HasValue ? pendingIds.Count : (int?)null,
                signedIds,
                pendingIds = total.HasValue ? pendingIds : null
            });
        }

        // --------------------------------------------
        // POST /api/v1/workforms/{id}/close  (סגירת טופס)
        // --------------------------------------------
        [HttpPost, Route("{id:int}/close")]
        public IHttpActionResult Close(int id)
        {
            try
            {
                var wf = WorkFormDAL.GetById(id);
                if (wf == null) return NotFound();

                if (wf.Status == "Closed")
                    return Ok(new { message = "הטופס כבר סגור", id });

                var assignedIds = WorkFormEmployeeDAL.GetAssignedIds(id);
                var signedIds = SignatureDAL.GetSignedEmployeeIds(id, wf.Version);

                if (assignedIds.Count > 0)
                {
                    var pending = assignedIds.Except(signedIds).ToList();
                    if (pending.Count > 0)
                    {
                        return Content(HttpStatusCode.BadRequest, new
                        {
                            ok = false,
                            message = "לא ניתן לסגור – יש עובדים שלא חתמו",
                            total = assignedIds.Count,
                            signed = signedIds.Count,
                            pendingIds = pending
                        });
                    }
                }

                WorkFormDAL.SetStatus(id, "Closed");
                return Ok(new { message = "הטופס נסגר", id });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError,
                               new { ok = false, error = ex.Message });
            }
        }

        // --------------------------------------------
        // PUT /api/v1/workforms/{id}  (גרסה חדשה, כלל 12 שעות)
        // --------------------------------------------
        [HttpPut, Route("{id:int}")]
        public IHttpActionResult Update(int id, [FromBody] WorkFormUpdateDto dto)
        {
            if (dto == null) return BadRequest("Body is required");

            try
            {
                var current = WorkFormDAL.GetById(id);
                if (current == null) return NotFound();

                // כלל 12 שעות
                if (DateTime.UtcNow > current.WorkDateTime.AddHours(12))
                    return Content(HttpStatusCode.BadRequest,
                                   new { ok = false, message = "חלון 12 השעות לעריכה הסתיים" });

                // צור גרסה חדשה
                var newId = WorkFormDAL.CreateNewVersion(id);
                var newForm = WorkFormDAL.GetById(newId);

                // עדכון שדות בסיסיים
                if (!string.IsNullOrWhiteSpace(dto.Site)) newForm.Site = dto.Site.Trim();
                if (!string.IsNullOrWhiteSpace(dto.WorkType)) newForm.WorkType = dto.WorkType.Trim();
                if (dto.WorkDateTime != default(DateTime))
                {
                    newForm.WorkDateTime = dto.WorkDateTime.Kind == DateTimeKind.Utc
                        ? dto.WorkDateTime
                        : dto.WorkDateTime.ToUniversalTime();
                }
                WorkFormDAL.UpdateBasic(newForm);

                // החלפת עובדים אם נשלחו
                if (dto.EmployeeIds != null)
                {
                    WorkFormEmployeeDAL.RemoveAllForForm(newId);
                    foreach (var empId in dto.EmployeeIds.Where(x => x > 0).Distinct())
                        WorkFormEmployeeDAL.Add(newId, empId);
                }

                // החלפת סיכונים אם נשלחו
                if (dto.RiskItemIds != null)
                {
                    WorkFormRiskItemDAL.RemoveAllForForm(newId);
                    foreach (var rid in dto.RiskItemIds.Where(x => x > 0).Distinct())
                        WorkFormRiskItemDAL.Add(newId, rid);
                }

                var after = WorkFormDAL.GetById(newId);
                return Ok(after);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError,
                               new { ok = false, error = ex.Message });
            }
        }

        // אופציונלי: תמיכה ב-OPTIONS ל-CORS/Preflight
        [HttpOptions, Route("{*any}")]
        public IHttpActionResult Options()
        {
            return Ok();
        }
    }
}
