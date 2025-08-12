using System.Web.Http;
using IsraelRailProject.app.v1.DAL;

namespace IsraelRailProject.app.v1.controllers
{
    [RoutePrefix("api/v1/employees")]
    public class EmployeesController : ApiController
    {
        // GET /api/v1/employees
        [HttpGet, Route("")]
        public IHttpActionResult GetAll()
        {
            var list = UserDAL.GetEmployees();
            return Ok(list);
        }
    }
}
