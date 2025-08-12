// קובץ חדש: app/v1/controllers/SignatureController.cs
using System;
using IsraelRailProject.app.v1.DAL;
using IsraelRailProject.app.v1.models.dtos;

namespace IsraelRailProject.app.v1.controllers
{
    [System.Web.Http.RoutePrefix("api/v1/signatures")]
    public class SignatureController : System.Web.Http.ApiController
    {
        // POST /api/v1/signatures
        [System.Web.Http.HttpPost, System.Web.Http.Route("")]
        public System.Web.Http.IHttpActionResult Post([System.Web.Http.FromBody] SignRequestDto dto)
        {
            if (dto == null) return BadRequest("Body is required");

            try
            {
                var wf = WorkFormDAL.GetById(dto.WorkFormId);
                if (wf == null) return NotFound();
                if (wf.Status != "Open") return BadRequest("הטופס אינו פתוח לחתימות");

                var assigned = WorkFormEmployeeDAL.GetByFormId(dto.WorkFormId);
                if (assigned.Count > 0 && !WorkFormEmployeeDAL.IsAssigned(dto.WorkFormId, dto.EmployeeId))
                    return BadRequest("העובד אינו משויך לטופס הזה");

                if (SignatureDAL.Exists(dto.WorkFormId, dto.EmployeeId, wf.Version))
                    return Ok(new { message = "העובד כבר חתם על הגרסה הזו", duplicate = true, version = wf.Version });

                SignatureDAL.Add(dto.WorkFormId, dto.EmployeeId, wf.Version);

                var signed = SignatureDAL.CountForFormVersion(dto.WorkFormId, wf.Version);
                int? total = assigned.Count > 0 ? (int?)assigned.Count : null;

                return Ok(new { message = "החתימה נשמרה", version = wf.Version, signed, total });
            }
            catch (Exception ex)
            {
                return Content(System.Net.HttpStatusCode.InternalServerError,
                               new { ok = false, error = ex.Message });
            }
        }
    }
}
