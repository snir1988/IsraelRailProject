// קובץ: app/v1/controllers/WorkFormController.cs
using System;
using System.Linq;
using System.Net;
using IsraelRailProject.app.v1.DAL;
using IsraelRailProject.app.v1.models;
using IsraelRailProject.app.v1.models.dtos;

namespace IsraelRailProject.app.v1.controllers
{
    [System.Web.Http.RoutePrefix("api/v1/workforms")]
    public class WorkFormController : System.Web.Http.ApiController
    {
        // POST /api/v1/workforms  (יצירת טופס + שיוך עובדים אופציונלי)
        [System.Web.Http.HttpPost, System.Web.Http.Route("")]
        public System.Web.Http.IHttpActionResult Create([System.Web.Http.FromBody] WorkFormCreateDto dto)
        {
            if (dto == null) return BadRequest("Body is required");
            if (string.IsNullOrWhiteSpace(dto.Site) || string.IsNullOrWhiteSpace(dto.WorkType))
                return BadRequest("Site ו-WorkType הם שדות חובה");

            try
            {
                var workUtc = dto.WorkDateTime.Kind == DateTimeKind.Utc
                              ? dto.WorkDateTime
                              : dto.WorkDateTime.ToUniversalTime();

                var wf = new WorkForm
                {
                    ManagerId = dto.ManagerId,
                    Site = dto.Site,
                    WorkDateTime = workUtc,
                    WorkType = dto.WorkType
                };

                var id = WorkFormDAL.Create(wf);

                // שיוך עובדים אם נשלחו
                if (dto.EmployeeIds != null)
                {
                    foreach (var empId in dto.EmployeeIds)
                        WorkFormEmployeeDAL.Add(id, empId);
                }

                var created = WorkFormDAL.GetById(id);
                return Ok(created);
            }
            catch (Exception ex)
            {
                return Content(System.Net.HttpStatusCode.InternalServerError,
                               new { ok = false, error = ex.Message });
            }
        }

        // GET /api/v1/workforms?managerId=1
        [System.Web.Http.HttpGet, System.Web.Http.Route("")]
        public System.Web.Http.IHttpActionResult ByManager([System.Web.Http.FromUri] int managerId)
        {
            var list = WorkFormDAL.GetByManager(managerId);
            return Ok(list);
        }

        // GET /api/v1/workforms/{id}  (טופס בודד)
        [System.Web.Http.HttpGet, System.Web.Http.Route("{id:int}")]
        public System.Web.Http.IHttpActionResult GetOne(int id)
        {
            var wf = WorkFormDAL.GetById(id);
            if (wf == null) return NotFound();
            return Ok(wf);
        }

        // POST /api/v1/workforms/{id}/send  (פתיחה לחתימות)
        [System.Web.Http.HttpPost, System.Web.Http.Route("{id:int}/send")]
        public System.Web.Http.IHttpActionResult OpenForSign(int id)
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
                return Content(System.Net.HttpStatusCode.InternalServerError,
                               new { ok = false, error = ex.Message });
            }
        }

        // GET /api/v1/workforms/{id}/status
        [System.Web.Http.HttpGet, System.Web.Http.Route("{id:int}/status")]
        public System.Web.Http.IHttpActionResult Status(int id)
        {
            var wf = WorkFormDAL.GetById(id);
            if (wf == null) return NotFound();

            var assignedIds = WorkFormEmployeeDAL.GetAssignedIds(id);            // מי אמור לחתום
            var signedIds = SignatureDAL.GetSignedEmployeeIds(id, wf.Version); // מי חתם בפועל
            var total = assignedIds.Count > 0 ? (int?)assignedIds.Count : null;

            var pendingIds = assignedIds.Except(signedIds).ToList();             // מי חסר

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

        // POST /api/v1/workforms/{id}/close  (סגירת טופס כשכל העובדים חתמו)
        [System.Web.Http.HttpPost, System.Web.Http.Route("{id:int}/close")]
        public System.Web.Http.IHttpActionResult Close(int id)
        {
            try
            {
                var wf = WorkFormDAL.GetById(id);
                if (wf == null) return NotFound();

                if (wf.Status == "Closed")
                    return Ok(new { message = "הטופס כבר סגור", id });

                // מי אמור לחתום ומי חתם בפועל
                var assignedIds = WorkFormEmployeeDAL.GetAssignedIds(id);
                var signedIds = SignatureDAL.GetSignedEmployeeIds(id, wf.Version);

                if (assignedIds.Count > 0)
                {
                    var pending = assignedIds.Except(signedIds).ToList();
                    if (pending.Count > 0)
                    {
                        return Content(System.Net.HttpStatusCode.BadRequest, new
                        {
                            ok = false,
                            message = "לא ניתן לסגור – יש עובדים שלא חתמו",
                            total = assignedIds.Count,
                            signed = signedIds.Count,
                            pendingIds = pending
                        });
                    }
                }

                // אם אין שיוכים בכלל, נאפשר סגירה (לדמו)
                WorkFormDAL.SetStatus(id, "Closed");
                return Ok(new { message = "הטופס נסגר", id });
            }
            catch (Exception ex)
            {
                return Content(System.Net.HttpStatusCode.InternalServerError,
                               new { ok = false, error = ex.Message });
            }
        }

        // PUT /api/v1/workforms/{id}  (יוצר גרסה חדשה עם שדות מעודכנים) — כלל 12 שעות
        [System.Web.Http.HttpPut, System.Web.Http.Route("{id:int}")]
        public System.Web.Http.IHttpActionResult Update(int id, [System.Web.Http.FromBody] WorkFormUpdateDto dto)
        {
            if (dto == null) return BadRequest("Body is required");

            try
            {
                var current = WorkFormDAL.GetById(id);
                if (current == null) return NotFound();

                // כלל 12 שעות
                if (DateTime.UtcNow > current.WorkDateTime.AddHours(12))
                    return Content(System.Net.HttpStatusCode.BadRequest,
                                   new { ok = false, message = "חלון 12 השעות לעריכה הסתיים" });

                // צור גרסה חדשה
                var newId = WorkFormDAL.CreateNewVersion(id);
                var newForm = WorkFormDAL.GetById(newId);

                // עדכון שדות בסיסיים מה־DTO
                newForm.Site = dto.Site ?? newForm.Site;
                newForm.WorkType = dto.WorkType ?? newForm.WorkType;
                newForm.WorkDateTime = dto.WorkDateTime == default(DateTime)
                    ? newForm.WorkDateTime
                    : (dto.WorkDateTime.Kind == DateTimeKind.Utc ? dto.WorkDateTime : dto.WorkDateTime.ToUniversalTime());

                WorkFormDAL.UpdateBasic(newForm);

                // אם שלחו רשימת עובדים – נחליף (אחרת נשאר מההעתקה)
                if (dto.EmployeeIds != null)
                {
                    WorkFormEmployeeDAL.RemoveAllForForm(newId);
                    foreach (var empId in dto.EmployeeIds)
                        WorkFormEmployeeDAL.Add(newId, empId);
                }

                // אם שלחו סיכונים – נחליף (אחרת נשאר מההעתקה)
                if (dto.RiskItemIds != null)
                {
                    WorkFormRiskItemDAL.RemoveAllForForm(newId);
                    foreach (var rid in dto.RiskItemIds)
                        WorkFormRiskItemDAL.Add(newId, rid);
                }

                var after = WorkFormDAL.GetById(newId);
                return Ok(after);
            }
            catch (Exception ex)
            {
                return Content(System.Net.HttpStatusCode.InternalServerError,
                               new { ok = false, error = ex.Message });
            }
        }
    }
}
