// קובץ: app/v1/DAL/WorkFormDAL.cs
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using IsraelRailProject.app.v1.models;

namespace IsraelRailProject.app.v1.DAL
{
    /// <summary>
    /// שכבת גישה לנתונים עבור טופסי עבודה (WorkForms):
    /// יצירה, שליפות, פתיחה לחתימות, עדכון סטטוס, גרסאות,
    /// ושיוכים (עובדים/סיכונים).
    /// </summary>
    public static class WorkFormDAL
    {
        // ===== יצירה בסיסית (ללא שיוכים) =====
        // מחזיר את מזהה הטופס שנוצר
        public static int Create(WorkForm wf)
        {
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                INSERT INTO WorkForms (ManagerId, Site, WorkDateTime, WorkType, Status, Version, OriginalFormId, CreatedAt)
                OUTPUT INSERTED.Id
                VALUES (@ManagerId, @Site, @WorkDateTime, @WorkType, N'Draft', 1, NULL, SYSUTCDATETIME())
            ", conn))
            {
                cmd.Parameters.AddWithValue("@ManagerId", wf.ManagerId);
                cmd.Parameters.AddWithValue("@Site", (object)wf.Site ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@WorkDateTime", wf.WorkDateTime); // מומלץ להעביר UTC מהשכבה הקוראת
                cmd.Parameters.AddWithValue("@WorkType", (object)wf.WorkType ?? DBNull.Value);

                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        // ===== יצירה + שיוך עובדים/סיכונים (עוטף) =====
        // מתודת עזר נוחה: יוצרת טופס ואז משייכת עובדים/סיכונים (בקריאות נפרדות)
        public static int CreateAndAssign(WorkForm wf, IEnumerable<int> employeeIds, IEnumerable<int> riskItemIds)
        {
            var id = Create(wf);
            if (employeeIds != null) SetEmployees(id, employeeIds);
            if (riskItemIds != null) SetRiskItems(id, riskItemIds);
            return id;
        }

        // ===== שליפה בודדת =====
        public static WorkForm GetById(int id)
        {
            WorkForm wf = null;

            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                SELECT Id, ManagerId, Site, WorkDateTime, WorkType, Status, Version, OriginalFormId, CreatedAt
                FROM WorkForms
                WHERE Id = @Id
            ", conn))
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
                            Site = rd.IsDBNull(2) ? "" : rd.GetString(2),
                            WorkDateTime = rd.GetDateTime(3),
                            WorkType = rd.IsDBNull(4) ? "" : rd.GetString(4),
                            Status = rd.IsDBNull(5) ? "" : rd.GetString(5),
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
                // אם תרצה גם סיכונים כתווים/מזהים למודל WorkForm – אפשר להוסיף שדה במודל ולקרוא כאן DAL נוסף.
            }

            return wf;
        }

        // ===== שליפה לפי מנהל =====
        public static List<WorkForm> GetByManager(int managerId)
        {
            var list = new List<WorkForm>();

            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                SELECT Id, ManagerId, Site, WorkDateTime, WorkType, Status, Version, OriginalFormId, CreatedAt
                FROM WorkForms
                WHERE ManagerId = @ManagerId
                ORDER BY CreatedAt DESC
            ", conn))
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
                            Site = rd.IsDBNull(2) ? "" : rd.GetString(2),
                            WorkDateTime = rd.GetDateTime(3),
                            WorkType = rd.IsDBNull(4) ? "" : rd.GetString(4),
                            Status = rd.IsDBNull(5) ? "" : rd.GetString(5),
                            Version = rd.GetInt32(6),
                            OriginalFormId = rd.IsDBNull(7) ? (int?)null : rd.GetInt32(7),
                            CreatedAt = rd.GetDateTime(8)
                        });
                    }
                }
            }

            return list;
        }

        // ===== פתיחה לחתימות =====
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

        // ===== עדכון סטטוס =====
        public static int SetStatus(int id, string status)
        {
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(
                "UPDATE WorkForms SET Status = @S WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@S", (object)status ?? DBNull.Value);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        // ===== יצירת גרסה חדשה =====
        // מעתיק גם שיוכי עובדים וסיכונים (אם טבלת הסיכונים קיימת)
        public static int CreateNewVersion(int sourceId)
        {
            var src = GetById(sourceId);
            if (src == null) throw new ArgumentException("הטופס לא נמצא");

            var rootId = src.OriginalFormId ?? src.Id;

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
                IF OBJECT_ID('dbo.WorkFormRiskItems','U') IS NOT NULL
                BEGIN
                    INSERT INTO WorkFormRiskItems (WorkFormId, RiskItemId)
                    SELECT SCOPE_IDENTITY(), RiskItemId
                    FROM WorkFormRiskItems WHERE WorkFormId=@Old;
                END
            ", conn))
            {
                cmd.Parameters.AddWithValue("@M", src.ManagerId);
                cmd.Parameters.AddWithValue("@Site", (object)src.Site ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DT", src.WorkDateTime);
                cmd.Parameters.AddWithValue("@Type", (object)src.WorkType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Ver", src.Version + 1);
                cmd.Parameters.AddWithValue("@Root", rootId);
                cmd.Parameters.AddWithValue("@Old", sourceId);

                conn.Open();
                return (int)cmd.ExecuteScalar(); // ה־Id החדש (מה-OUTPUT הראשון)
            }
        }

        // ===== עדכון שדות בסיסיים =====
        public static int UpdateBasic(WorkForm wf)
        {
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                UPDATE WorkForms
                SET Site=@Site, WorkDateTime=@DT, WorkType=@Type
                WHERE Id=@Id
            ", conn))
            {
                cmd.Parameters.AddWithValue("@Id", wf.Id);
                cmd.Parameters.AddWithValue("@Site", (object)wf.Site ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DT", wf.WorkDateTime);
                cmd.Parameters.AddWithValue("@Type", (object)wf.WorkType ?? DBNull.Value);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        // ====================================================================
        // =============== שיוכים: עובדים / סיכונים ==========================
        // ====================================================================

        /// <summary>
        /// קובע את רשימת העובדים המשויכים לטופס (מוחק ומכניס מחדש).
        /// </summary>
        public static void SetEmployees(int formId, IEnumerable<int> employeeIds)
        {
            var ids = (employeeIds ?? Enumerable.Empty<int>()).Distinct().ToList();

            using (var conn = Db.GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        using (var del = new SqlCommand(@"DELETE FROM WorkFormEmployees WHERE WorkFormId=@F", conn, tx))
                        {
                            del.Parameters.AddWithValue("@F", formId);
                            del.ExecuteNonQuery();
                        }

                        if (ids.Count > 0)
                        {
                            using (var ins = new SqlCommand(@"INSERT INTO WorkFormEmployees (WorkFormId, EmployeeId) VALUES (@F, @E)", conn, tx))
                            {
                                var pF = ins.Parameters.Add("@F", System.Data.SqlDbType.Int);
                                var pE = ins.Parameters.Add("@E", System.Data.SqlDbType.Int);
                                pF.Value = formId;

                                foreach (var eid in ids)
                                {
                                    pE.Value = eid;
                                    ins.ExecuteNonQuery();
                                }
                            }
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// קובע את רשימת הסיכונים המשויכים לטופס (מוחק ומכניס מחדש).
        /// בטוח גם אם טבלת WorkFormRiskItems לא קיימת – לא ייזרק חריג.
        /// </summary>
        // קובץ: app/v1/DAL/WorkFormDAL.cs  (רק המקטע הזה)
        public static void SetRiskItems(int formId, IEnumerable<int> riskItemIds)
        {
            var ids = (riskItemIds ?? Enumerable.Empty<int>()).Distinct().ToList();

            using (var conn = Db.GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        // בדיקה עדינה לקיום הטבלה
                        using (var chk = new SqlCommand(@"SELECT OBJECT_ID('dbo.WorkFormRiskItems','U')", conn, tx))
                        {
                            var objId = chk.ExecuteScalar();
                            bool hasTable = objId != null && objId != DBNull.Value;
                            if (!hasTable)
                            {
                                tx.Commit(); // אין טבלת שיוך – יוצאים בשקט
                                return;
                            }
                        }

                        using (var del = new SqlCommand(@"DELETE FROM WorkFormRiskItems WHERE WorkFormId=@F", conn, tx))
                        {
                            del.Parameters.AddWithValue("@F", formId);
                            del.ExecuteNonQuery();
                        }

                        if (ids.Count > 0)
                        {
                            using (var ins = new SqlCommand(@"INSERT INTO WorkFormRiskItems (WorkFormId, RiskItemId) VALUES (@F, @R)", conn, tx))
                            {
                                var pF = ins.Parameters.Add("@F", System.Data.SqlDbType.Int);
                                var pR = ins.Parameters.Add("@R", System.Data.SqlDbType.Int);
                                pF.Value = formId;

                                foreach (var rid in ids)
                                {
                                    pR.Value = rid;
                                    ins.ExecuteNonQuery();
                                }
                            }
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}

