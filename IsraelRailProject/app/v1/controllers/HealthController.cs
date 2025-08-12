using IsraelRailProject.app.v1.DAL;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Http;                   

namespace IsraelRailProject.app.v1.controllers
{
    [RoutePrefix("api/v1/health")]
    public class HealthController : ApiController
    {
        [HttpGet, Route("db")]
        public IHttpActionResult CheckDb()
        {
            try
            {
                using (var conn = Db.GetConnection())
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Users", conn))
                {
                    conn.Open();
                    var count = (int)cmd.ExecuteScalar();
                    return Ok(new { ok = true, users = count });
                }
            }
            catch (System.Exception ex)
            {
                return Ok(new { ok = false, error = ex.Message });
            }
        }
    }
}