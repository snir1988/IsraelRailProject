using System.Collections.Generic;
using System.Data.SqlClient;
using IsraelRailProject.app.v1.models.dtos;

namespace IsraelRailProject.app.v1.DAL
{
    public static class UserDAL
    {
        // מחזיר את כל העובדים לבחירה ב-UI
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

        // האם משתמש כלשהו קיים (לבדוק ManagerId)
        public static bool UserExists(int userId)
        {
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM Users WHERE Id = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", userId);
                conn.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        // האם המזהה הוא עובד (Role = Employee)
        public static bool IsEmployeeId(int userId)
        {
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM Users WHERE Id = @id AND Role = N'Employee'", conn))
            {
                cmd.Parameters.AddWithValue("@id", userId);
                conn.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }
    }
}
