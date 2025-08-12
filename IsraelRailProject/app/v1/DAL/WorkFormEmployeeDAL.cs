using System.Collections.Generic;
using System.Data.SqlClient;
using IsraelRailProject.app.v1.models;

namespace IsraelRailProject.app.v1.DAL
{
    public static class WorkFormEmployeeDAL
    {
        public static void Add(int workFormId, int employeeId)
        {
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                IF NOT EXISTS (SELECT 1 FROM WorkFormEmployees WHERE WorkFormId=@F AND EmployeeId=@E)
                INSERT INTO WorkFormEmployees (WorkFormId, EmployeeId) VALUES (@F, @E)", conn))
            {
                cmd.Parameters.AddWithValue("@F", workFormId);
                cmd.Parameters.AddWithValue("@E", employeeId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void RemoveAllForForm(int workFormId)
        {
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand("DELETE FROM WorkFormEmployees WHERE WorkFormId=@F", conn))
            {
                cmd.Parameters.AddWithValue("@F", workFormId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static List<WorkFormEmployee> GetByFormId(int workFormId)
        {
            var list = new List<WorkFormEmployee>();
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                SELECT Id, WorkFormId, EmployeeId
                FROM WorkFormEmployees WHERE WorkFormId=@F", conn))
            {
                cmd.Parameters.AddWithValue("@F", workFormId);
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new WorkFormEmployee
                        {
                            Id = rd.GetInt32(0),
                            WorkFormId = rd.GetInt32(1),
                            EmployeeId = rd.GetInt32(2)
                        });
                    }
                }
            }
            return list;
        }

        public static bool IsAssigned(int workFormId, int employeeId)
        {
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                SELECT 1 FROM WorkFormEmployees WHERE WorkFormId=@F AND EmployeeId=@E", conn))
            {
                cmd.Parameters.AddWithValue("@F", workFormId);
                cmd.Parameters.AddWithValue("@E", employeeId);
                conn.Open();
                var res = cmd.ExecuteScalar();
                return res != null;
            }
        }
        public static List<int> GetAssignedIds(int workFormId)
        {
            var ids = new List<int>();
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(
                "SELECT EmployeeId FROM WorkFormEmployees WHERE WorkFormId=@F ORDER BY EmployeeId", conn))
            {
                cmd.Parameters.AddWithValue("@F", workFormId);
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read()) ids.Add(rd.GetInt32(0));
            }
            return ids;
        }

    }
}
