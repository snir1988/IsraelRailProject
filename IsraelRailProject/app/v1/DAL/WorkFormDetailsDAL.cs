using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using IsraelRailProject.app.v1.models.dtos;

namespace IsraelRailProject.app.v1.DAL
{
    public static class WorkFormDetailsDAL
    {
        public static WorkFormDetailsDto GetDetails(int formId)
        {
            WorkFormDetailsDto dto = null;

            using (var conn = Db.GetConnection())
            {
                conn.Open();

                // --- פרטי טופס + עובדים + האם חתמו (אם יש עמודת SignedAt ב-Signatures יוצג תאריך, אחרת יישאר null) ---
                using (var cmd = new SqlCommand(@"
SELECT 
    wf.Id,
    wf.WorkDateTime,
    wf.Site,
    wf.WorkType,
    mgr.Id           AS ManagerId,
    mgr.FullName     AS ManagerName,
    emp.Id           AS EmpId,
    emp.FullName     AS EmpName,
    s.SignedAt       AS EmpSignedAt
FROM WorkForms wf
LEFT JOIN Users              AS mgr ON mgr.Id = wf.ManagerId
LEFT JOIN WorkFormEmployees  AS wfe ON wfe.WorkFormId = wf.Id
LEFT JOIN Users              AS emp ON emp.Id = wfe.EmployeeId
LEFT JOIN Signatures         AS s   ON s.WorkFormId = wf.Id AND s.EmployeeId = emp.Id
WHERE wf.Id = @id
ORDER BY emp.FullName;", conn))
                {
                    cmd.Parameters.AddWithValue("@id", formId);

                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            if (dto == null)
                            {
                                dto = new WorkFormDetailsDto
                                {
                                    Id = rd.GetInt32(0),
                                    WorkDateTime = rd.GetDateTime(1),
                                    Site = rd.IsDBNull(2) ? "" : rd.GetString(2),
                                    WorkType = rd.IsDBNull(3) ? "" : rd.GetString(3),
                                    ManagerId = rd.IsDBNull(4) ? 0 : rd.GetInt32(4),
                                    ManagerName = rd.IsDBNull(5) ? "" : rd.GetString(5),
                                    Employees = new List<WorkFormEmployeeSignDto>(),
                                    Risks = new List<RiskItemDto>()
                                };
                            }

                            // עובד (ייתכן שאין כלל עובדים משויכים)
                            if (!rd.IsDBNull(6))
                            {
                                dto.Employees.Add(new WorkFormEmployeeSignDto
                                {
                                    EmployeeId = rd.GetInt32(6),
                                    FullName = rd.IsDBNull(7) ? "" : rd.GetString(7),
                                    SignedAt = rd.IsDBNull(8) ? (DateTime?)null : rd.GetDateTime(8)
                                });
                            }
                        }
                    }
                }

                if (dto == null) return null;

                // --- סיכונים ששויכו לטופס (WorkFormRiskItems + RiskItems) ---
                using (var cmd2 = new SqlCommand(@"
SELECT ri.Id, ri.Name
FROM WorkFormRiskItems AS wfri
INNER JOIN RiskItems   AS ri ON ri.Id = wfri.RiskItemId
WHERE wfri.WorkFormId = @id
ORDER BY ri.Name;", conn))
                {
                    cmd2.Parameters.AddWithValue("@id", formId);

                    using (var rd2 = cmd2.ExecuteReader())
                    {
                        while (rd2.Read())
                        {
                            dto.Risks.Add(new RiskItemDto
                            {
                                Id = rd2.GetInt32(0),
                                Name = rd2.IsDBNull(1) ? "" : rd2.GetString(1)
                            });
                        }
                    }
                }
            }

            return dto;
        }

        public static bool IsEmployeeAssigned(int formId, int employeeId)
        {
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(
                "SELECT COUNT(1) FROM WorkFormEmployees WHERE WorkFormId=@fid AND EmployeeId=@eid;", conn))
            {
                cmd.Parameters.AddWithValue("@fid", formId);
                cmd.Parameters.AddWithValue("@eid", employeeId);
                conn.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }
    }
}
