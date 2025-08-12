using System.Web.Http;
using IsraelRailProject.app.v1.DAL;

namespace IsraelRailProject.app.v1.controllers
{
    [RoutePrefix("api/v1/risks")]
    public class RisksController : ApiController
    {
        // GET /api/v1/risks
        [HttpGet, Route("")]
        public IHttpActionResult GetAll()
        {
            var list = RiskItemDAL.GetAll();
            return Ok(list);
        }
    }
}
