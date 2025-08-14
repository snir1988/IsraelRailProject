using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using IsraelRailProject.app.v1.models.dtos;

namespace IsraelRailProject.app.v1.DAL
{
    public static class RiskItemDAL
    {
        // שמרתי את השם המקורי כדי לא לשבור קוד קיים
        public static List<RiskItemDto> GetAll()
        {
            var list = new List<RiskItemDto>();
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                SELECT Id, Name
                FROM RiskItems
                ORDER BY Name;", conn))
            {
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new RiskItemDto
                        {
                            Id = rd.GetInt32(0),
                            Name = rd.GetString(1)
                        });
                    }
                }
            }
            return list;
        }

        // alias נוח – תואם לשימושים מהקונטרולר
        public static List<RiskItemDto> GetRiskItems() => GetAll();

        public static RiskItemDto GetById(int id)
        {
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                SELECT Id, Name
                FROM RiskItems
                WHERE Id = @Id;", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                    {
                        return new RiskItemDto
                        {
                            Id = rd.GetInt32(0),
                            Name = rd.GetString(1)
                        };
                    }
                }
            }
            return null;
        }

        public static int Add(RiskItemDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Name is required", nameof(dto));

            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                INSERT INTO RiskItems (Name)
                VALUES (@Name);
                SELECT CAST(SCOPE_IDENTITY() AS int);", conn))
            {
                cmd.Parameters.AddWithValue("@Name", dto.Name.Trim());
                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public static bool Update(RiskItemDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.Id <= 0) throw new ArgumentException("Id is required", nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Name is required", nameof(dto));

            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                UPDATE RiskItems
                SET Name = @Name
                WHERE Id = @Id;", conn))
            {
                cmd.Parameters.AddWithValue("@Id", dto.Id);
                cmd.Parameters.AddWithValue("@Name", dto.Name.Trim());
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // מוחק קודם קשרי N:N ואז את הסיכון עצמו
        public static bool Delete(int id, out string error)
        {
            error = null;

            using (var conn = Db.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    cmd.Transaction = tx;

                    try
                    {
                        // נקה קישורים לטפסים
                        cmd.CommandText = "DELETE FROM WorkFormRiskItems WHERE RiskItemId = @Id";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.ExecuteNonQuery();

                        // מחיקת הסיכון
                        cmd.CommandText = "DELETE FROM RiskItems WHERE Id = @Id";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@Id", id);
                        var rows = cmd.ExecuteNonQuery();

                        tx.Commit();

                        if (rows == 0)
                        {
                            error = "לא נמצא סיכון למחיקה.";
                            return false;
                        }
                        return true;
                    }
                    catch (SqlException ex) when (ex.Number == 547) // FK violation
                    {
                        tx.Rollback();
                        error = "לא ניתן למחוק: קיימות רשומות שתלויות בסיכון זה.";
                        return false;
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        error = "שגיאת שרת במחיקה: " + ex.Message;
                        return false;
                    }
                }
            }
        }
    }
}
