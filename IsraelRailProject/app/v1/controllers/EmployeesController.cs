// app/v1/controllers/EmployeesController.cs
using System.Web.Http;
using IsraelRailProject.app.v1.DAL;

namespace IsraelRailProject.app.v1.controllers
{
    [RoutePrefix("api/v1/employees")]
    public class EmployeesController : ApiController
    {
        // מחזיר את רשימת העובדים מתוך ה-DB (Users.Role='Employee')
        [HttpGet, Route("")]
        public IHttpActionResult Get()
        {
            var list = UserDAL.GetEmployees(); // <-- זה מה-DB שלך
            return Ok(list);
        }
    }
}
