using System.Collections.Generic;
using System.Data.SqlClient;
using IsraelRailProject.app.v1.models;

namespace IsraelRailProject.app.v1.DAL
{
    public static class WorkFormRiskItemDAL
    {
        public static void Add(int workFormId, int riskItemId)
        {
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                IF NOT EXISTS (SELECT 1 FROM WorkFormRiskItems WHERE WorkFormId=@F AND RiskItemId=@R)
                INSERT INTO WorkFormRiskItems (WorkFormId, RiskItemId) VALUES (@F, @R)", conn))
            {
                cmd.Parameters.AddWithValue("@F", workFormId);
                cmd.Parameters.AddWithValue("@R", riskItemId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void RemoveAllForForm(int workFormId)
        {
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand("DELETE FROM WorkFormRiskItems WHERE WorkFormId=@F", conn))
            {
                cmd.Parameters.AddWithValue("@F", workFormId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
