using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using IsraelRailProject.app.v1.models.dtos;

namespace IsraelRailProject.app.v1.DAL
{
    public static class WorkFormSignDAL
    {
        // טפסים שממתינים לחתימה של עובד: אין רשומה מתאימה ב-Signatures
        public static List<WorkFormForSignDto> GetPendingForEmployee(int employeeId)
        {
            var list = new List<WorkFormForSignDto>();

            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
SELECT  wf.Id,
        wf.WorkDateTime,
        wf.Site,
        wf.WorkType,
        mgr.FullName AS ManagerName
FROM WorkForms              AS wf
JOIN WorkFormEmployees      AS wfe
  ON wfe.WorkFormId = wf.Id
 AND wfe.EmployeeId = @EmpId
LEFT JOIN Users             AS mgr
  ON mgr.Id = wf.ManagerId
WHERE NOT EXISTS (
    SELECT 1
    FROM Signatures s
    WHERE s.WorkFormId = wf.Id
      AND s.EmployeeId  = @EmpId
)
ORDER BY wf.WorkDateTime DESC, wf.Id DESC;", conn))
            {
                cmd.Parameters.AddWithValue("@EmpId", employeeId);
                conn.Open();

                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new WorkFormForSignDto
                        {
                            Id = rd.GetInt32(0),
                            WorkDateTime = rd.GetDateTime(1),
                            Site = rd.IsDBNull(2) ? "" : rd.GetString(2),
                            WorkType = rd.IsDBNull(3) ? "" : rd.GetString(3),
                            ManagerName = rd.IsDBNull(4) ? "" : rd.GetString(4),
                            SignedAt = null
                        });
                    }
                }
            }

            return list;
        }

        // חתימה: אם כבר קיים—נחשב הצלחה; אם לא—נכניס עם Version תקף
        public static bool Sign(int workFormId, int employeeId)
        {
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
IF EXISTS (SELECT 1 FROM Signatures WHERE WorkFormId=@Fid AND EmployeeId=@EmpId)
BEGIN
    SELECT 1; -- כבר חתום
END
ELSE
BEGIN
    DECLARE @ver INT = 1;
    IF COL_LENGTH('WorkForms','Version') IS NOT NULL
        SELECT TOP 1 @ver = ISNULL(Version,1) FROM WorkForms WHERE Id=@Fid;

    INSERT INTO Signatures(WorkFormId, EmployeeId, Version)
    VALUES(@Fid, @EmpId, @ver);

    SELECT 1;
END
", conn))
            {
                cmd.Parameters.AddWithValue("@Fid", workFormId);
                cmd.Parameters.AddWithValue("@EmpId", employeeId);
                conn.Open();

                var result = cmd.ExecuteScalar();
                return Convert.ToInt32(result) == 1;
            }
        }
    }
}
