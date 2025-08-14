using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using IsraelRailProject.app.v1.models.dtos;

namespace IsraelRailProject.app.v1.DAL
{
    public static class WorkFormHistoryDAL
    {
        public static List<WorkFormHistoryDto> GetLatest(int top = 200)
        {
            var list = new List<WorkFormHistoryDto>();
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                SELECT TOP (@Top)
                       wf.Id,
                       wf.WorkDateTime,
                       wf.Site,
                       wf.WorkType,
                       mgr.FullName AS ManagerName,
                       (SELECT COUNT(1) FROM WorkFormEmployees wfe WHERE wfe.WorkFormId = wf.Id)        AS EmployeesCount,
                       (SELECT COUNT(1) FROM Signatures s         WHERE s.WorkFormId = wf.Id)            AS SignedCount
                FROM WorkForms wf
                LEFT JOIN Users mgr ON mgr.Id = wf.ManagerId
                ORDER BY wf.WorkDateTime DESC, wf.Id DESC;", conn))
            {
                cmd.Parameters.AddWithValue("@Top", top);
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new WorkFormHistoryDto
                        {
                            Id = rd.GetInt32(0),
                            WorkDateTime = rd.GetDateTime(1),
                            Site = rd.IsDBNull(2) ? "" : rd.GetString(2),
                            WorkType = rd.IsDBNull(3) ? "" : rd.GetString(3),
                            ManagerName = rd.IsDBNull(4) ? "" : rd.GetString(4),
                            EmployeesCount = rd.IsDBNull(5) ? 0 : rd.GetInt32(5),
                            SignedCount = rd.IsDBNull(6) ? 0 : rd.GetInt32(6),
                        });
                    }
                }
            }
            return list;
        }
        public static WorkFormDetailsDto GetDetails(int formId)
        {
            WorkFormDetailsDto dto = null;

            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                SELECT 
                    wf.Id,
                    wf.WorkDateTime,
                    wf.Site,
                    wf.WorkType,
                    wf.ManagerId,
                    mgr.FullName AS ManagerName,
                    emp.Id      AS EmpId,
                    emp.FullName AS EmpName,
                    wfe.SignedAt
                FROM WorkForms wf
                LEFT JOIN Users mgr              ON mgr.Id = wf.ManagerId
                LEFT JOIN WorkFormEmployees wfe  ON wfe.WorkFormId = wf.Id
                LEFT JOIN Users emp              ON emp.Id = wfe.EmployeeId
                WHERE wf.Id = @id
                ORDER BY emp.FullName;", conn))
            {
                cmd.Parameters.AddWithValue("@id", formId);
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        if (dto == null)
                        {
                            // יצירת הראש
                            dto = new WorkFormDetailsDto
                            {
                                Id = rd.GetInt32(0),
                                WorkDateTime = rd.GetDateTime(1),
                                Site = rd.IsDBNull(2) ? "" : rd.GetString(2),
                                WorkType = rd.IsDBNull(3) ? "" : rd.GetString(3),
                                ManagerId = rd.IsDBNull(4) ? 0 : rd.GetInt32(4),
                                ManagerName = rd.IsDBNull(5) ? "" : rd.GetString(5)
                            };
                        }

                        // הוספת עובד (אם קיים)
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

            return dto; // אם לא נמצא טופס – יחזור null
        }
    }
}
    
