using System.Collections.Generic;
using System.Data.SqlClient;
using IsraelRailProject.app.v1.models.dtos;

namespace IsraelRailProject.app.v1.DAL
{
    public static class UserDAL
    {
        public static List<EmployeeDto> GetEmployees()
        {
            var list = new List<EmployeeDto>();
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                SELECT Id, FullName
                FROM Users
                WHERE Role = N'Employee'
                ORDER BY FullName", conn))
            {
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new EmployeeDto
                        {
                            Id = rd.GetInt32(0),
                            FullName = rd.GetString(1)
                        });
                    }
                }
            }
            return list;
        }
    }
}
