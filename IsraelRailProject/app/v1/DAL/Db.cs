using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace IsraelRailProject.app.v1.DAL
{
	public class Db
    {
        public static SqlConnection GetConnection()
        {
            var cs = ConfigurationManager.ConnectionStrings["IsraelRailDB"].ConnectionString;
            return new SqlConnection(cs);
        }
    }
}