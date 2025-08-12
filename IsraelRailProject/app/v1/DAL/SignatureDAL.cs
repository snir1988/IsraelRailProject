using System.Collections.Generic;
using System.Data.SqlClient;

namespace IsraelRailProject.app.v1.DAL
{
    public static class SignatureDAL
    {
        // בדיקה אם העובד כבר חתם על הגרסה הזו
        public static bool Exists(int workFormId, int employeeId, int version)
        {
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                SELECT 1
                FROM Signatures
                WHERE WorkFormId=@F AND EmployeeId=@E AND Version=@V", conn))
            {
                cmd.Parameters.AddWithValue("@F", workFormId);
                cmd.Parameters.AddWithValue("@E", employeeId);
                cmd.Parameters.AddWithValue("@V", version);
                conn.Open();
                var res = cmd.ExecuteScalar();
                return res != null;
            }
        }

        // שמירת חתימה (הטבלה מונעת כפילויות ע"י UQ על FormId+EmployeeId+Version)
        public static int Add(int workFormId, int employeeId, int version)
        {
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                INSERT INTO Signatures (WorkFormId, EmployeeId, Version)
                VALUES (@F, @E, @V)", conn))
            {
                cmd.Parameters.AddWithValue("@F", workFormId);
                cmd.Parameters.AddWithValue("@E", employeeId);
                cmd.Parameters.AddWithValue("@V", version);
                conn.Open();
                return cmd.ExecuteNonQuery(); // 1 אם הצליח
            }
        }

        // ספירת חתימות לגרסה
        public static int CountForFormVersion(int workFormId, int version)
        {
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                SELECT COUNT(*)
                FROM Signatures
                WHERE WorkFormId=@F AND Version=@V", conn))
            {
                cmd.Parameters.AddWithValue("@F", workFormId);
                cmd.Parameters.AddWithValue("@V", version);
                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }
        public static List<int> GetSignedEmployeeIds(int workFormId, int version)
        {
            var ids = new List<int>();
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
        SELECT EmployeeId
        FROM Signatures
        WHERE WorkFormId=@F AND Version=@V
        ORDER BY EmployeeId", conn))
            {
                cmd.Parameters.AddWithValue("@F", workFormId);
                cmd.Parameters.AddWithValue("@V", version);
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read()) ids.Add(rd.GetInt32(0));
            }
            return ids;
        }

    }
}
