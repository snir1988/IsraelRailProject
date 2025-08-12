using System.Collections.Generic;
using System.Web.Http;
using IsraelRailProject.app.v1.models.dtos;

namespace IsraelRailProject.app.v1.controllers
{
    [RoutePrefix("api/v1/risks")]
    public class RisksController : ApiController
    {
        [HttpGet, Route("")]
        public IHttpActionResult GetAll()
        {
            var list = new List<RiskItemDto>
            {
                new RiskItemDto { Id = 1, Name = "עבודה בסמוך למסילה פעילה" },
                new RiskItemDto { Id = 2, Name = "עבודה בגובה" },
                new RiskItemDto { Id = 3, Name = "ציוד כבד באתר" },
                new RiskItemDto { Id = 4, Name = "עבודה בלילה" }
            };
            return Ok(list);
        }
    }
}
