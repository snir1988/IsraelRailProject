// קובץ: app/v1/DAL/WorkFormDAL.cs
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using IsraelRailProject.app.v1.models;

namespace IsraelRailProject.app.v1.DAL
{
    /// <summary>
    /// שכבת גישה לנתונים עבור טופסי עבודה (WorkForms)
    /// יצירה, שליפות, פתיחה לחתימות, עדכון סטטוס, וגרסאות.
    /// </summary>
    public static class WorkFormDAL
    {
        // יצירת טופס חדש (Draft), מחזיר את המזהה שנוצר
        public static int Create(WorkForm wf)
        {
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                INSERT INTO WorkForms (ManagerId, Site, WorkDateTime, WorkType, Status, Version, OriginalFormId, CreatedAt)
                OUTPUT INSERTED.Id
                VALUES (@ManagerId, @Site, @WorkDateTime, @WorkType, N'Draft', 1, NULL, SYSUTCDATETIME())", conn))
            {
                cmd.Parameters.AddWithValue("@ManagerId", wf.ManagerId);
                cmd.Parameters.AddWithValue("@Site", wf.Site);
                cmd.Parameters.AddWithValue("@WorkDateTime", wf.WorkDateTime); // מומלץ להעביר UTC מהשכבה הקוראת
                cmd.Parameters.AddWithValue("@WorkType", wf.WorkType);

                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        // שליפה לפי מזהה (לטופס בודד)
        public static WorkForm GetById(int id)
        {
            WorkForm wf = null;

            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                SELECT Id, ManagerId, Site, WorkDateTime, WorkType, Status, Version, OriginalFormId, CreatedAt
                FROM WorkForms
                WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();

                using (var rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                    {
                        wf = new WorkForm
                        {
                            Id = rd.GetInt32(0),
                            ManagerId = rd.GetInt32(1),
                            Site = rd.GetString(2),
                            WorkDateTime = rd.GetDateTime(3),
                            WorkType = rd.GetString(4),
                            Status = rd.GetString(5),
                            Version = rd.GetInt32(6),
                            OriginalFormId = rd.IsDBNull(7) ? (int?)null : rd.GetInt32(7),
                            CreatedAt = rd.GetDateTime(8)
                        };
                    }
                }
            }

            if (wf != null)
            {
                // טעינת עובדים משויכים (לנוחות שכבות עליונות)
                wf.Employees = WorkFormEmployeeDAL.GetByFormId(id);
            }

            return wf;
        }

        // שליפת כל הטפסים של מנהל מסוים (לרשימה במסך)
        public static List<WorkForm> GetByManager(int managerId)
        {
            var list = new List<WorkForm>();

            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                SELECT Id, ManagerId, Site, WorkDateTime, WorkType, Status, Version, OriginalFormId, CreatedAt
                FROM WorkForms
                WHERE ManagerId = @ManagerId
                ORDER BY CreatedAt DESC", conn))
            {
                cmd.Parameters.AddWithValue("@ManagerId", managerId);
                conn.Open();

                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new WorkForm
                        {
                            Id = rd.GetInt32(0),
                            ManagerId = rd.GetInt32(1),
                            Site = rd.GetString(2),
                            WorkDateTime = rd.GetDateTime(3),
                            WorkType = rd.GetString(4),
                            Status = rd.GetString(5),
                            Version = rd.GetInt32(6),
                            OriginalFormId = rd.IsDBNull(7) ? (int?)null : rd.GetInt32(7),
                            CreatedAt = rd.GetDateTime(8)
                        });
                    }
                }
            }

            return list;
        }

        // פתיחה לחתימות (סטטוס Open)
        public static int OpenForSign(int id)
        {
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(
                "UPDATE WorkForms SET Status = N'Open' WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                return cmd.ExecuteNonQuery(); // 1 אם עודכן, 0 אם לא נמצא
            }
        }

        // עדכון סטטוס של טופס (Draft/Open/Closed/...)
        public static int SetStatus(int id, string status)
        {
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(
                "UPDATE WorkForms SET Status = @S WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@S", status);
                conn.Open();
                return cmd.ExecuteNonQuery(); // 1 אם עודכן
            }
        }

        // יצירת גרסה חדשה על בסיס טופס קיים: Version++ , Status='Draft'
        // מעתיק גם שיוכי עובדים וסיכונים ב-SQL (בתוך אותו scope)
        public static int CreateNewVersion(int sourceId)
        {
            var src = GetById(sourceId);
            if (src == null) throw new ArgumentException("הטופס לא נמצא");

            var rootId = src.OriginalFormId.HasValue ? src.OriginalFormId.Value : src.Id;

            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                -- יצירת גרסה חדשה
                INSERT INTO WorkForms (ManagerId, Site, WorkDateTime, WorkType, Status, Version, OriginalFormId, CreatedAt)
                OUTPUT INSERTED.Id
                VALUES (@M, @Site, @DT, @Type, N'Draft', @Ver, @Root, SYSUTCDATETIME());

                -- העתקת עובדים מהגרסה הישנה לגרסה החדשה
                INSERT INTO WorkFormEmployees (WorkFormId, EmployeeId)
                SELECT SCOPE_IDENTITY(), EmployeeId
                FROM WorkFormEmployees WHERE WorkFormId=@Old;

                -- העתקת סיכונים מהגרסה הישנה לגרסה החדשה (אם קיימת הטבלה)
                INSERT INTO WorkFormRiskItems (WorkFormId, RiskItemId)
                SELECT SCOPE_IDENTITY(), RiskItemId
                FROM WorkFormRiskItems WHERE WorkFormId=@Old;
            ", conn))
            {
                cmd.Parameters.AddWithValue("@M", src.ManagerId);
                cmd.Parameters.AddWithValue("@Site", src.Site);
                cmd.Parameters.AddWithValue("@DT", src.WorkDateTime);
                cmd.Parameters.AddWithValue("@Type", src.WorkType);
                cmd.Parameters.AddWithValue("@Ver", src.Version + 1);
                cmd.Parameters.AddWithValue("@Root", rootId);
                cmd.Parameters.AddWithValue("@Old", sourceId);

                conn.Open();
                return (int)cmd.ExecuteScalar(); // ה־Id החדש (מה-OUTPUT הראשון)
            }
        }

        // עדכון שדות בסיסיים (ללא סטטוס/גרסה)
        public static int UpdateBasic(WorkForm wf)
        {
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                UPDATE WorkForms
                SET Site=@Site, WorkDateTime=@DT, WorkType=@Type
                WHERE Id=@Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", wf.Id);
                cmd.Parameters.AddWithValue("@Site", wf.Site);
                cmd.Parameters.AddWithValue("@DT", wf.WorkDateTime);
                cmd.Parameters.AddWithValue("@Type", wf.WorkType);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }
    }
}
