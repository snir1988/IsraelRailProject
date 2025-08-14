using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using IsraelRailProject.app.v1.models.dtos;

namespace IsraelRailProject.app.v1.DAL
{
    public static class UserDAL
    {
        // שליפת כל העובדים (לטבלה/בחירה)
        public static List<EmployeeDto> GetEmployees()
        {
            {
                var list = new List<EmployeeDto>();
                using (var conn = Db.GetConnection())
                using (var cmd = new SqlCommand(@"
                SELECT Id, FullName, Email, Role
                FROM Users
                WHERE UPPER(ISNULL(Role, N'Employee')) <> N'MANAGER'
                ORDER BY FullName;", conn))
                {
                    conn.Open();
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            list.Add(new EmployeeDto
                            {
                                Id = rd.GetInt32(0),
                                FullName = rd.IsDBNull(1) ? "" : rd.GetString(1),
                                Email = rd.IsDBNull(2) ? "" : rd.GetString(2),
                                // אם הוספת שדה Role ב-DTO:
                                Role = rd.IsDBNull(3) ? "" : rd.GetString(3)
                            });
                        }
                    }
                }
                return list;
            }
        }

        // עזרים קיימים
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

        // --- CRUD ---

        public static EmployeeDto GetEmployeeById(int id)
        {
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                SELECT Id, FullName, Email
                FROM Users
                WHERE Id = @id AND Role = N'Employee'", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                    {
                        return new EmployeeDto
                        {
                            Id = rd.GetInt32(0),
                            FullName = rd.GetString(1),
                            Email = rd.IsDBNull(2) ? "" : rd.GetString(2)
                        };
                    }
                }
            }
            return null;
        }

        // הוספה (Email ו-Pass חובה כי NOT NULL)
        public static int AddEmployee(EmployeeDto employee)
        {
            if (employee == null) throw new ArgumentNullException(nameof(employee));
            if (string.IsNullOrWhiteSpace(employee.FullName)) throw new ArgumentException("FullName is required");
            if (string.IsNullOrWhiteSpace(employee.Email)) throw new ArgumentException("Email is required");
            if (string.IsNullOrWhiteSpace(employee.Pass)) throw new ArgumentException("Pass is required");

            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                INSERT INTO Users (FullName, Email, Pass, Role)
                VALUES (@FullName, @Email, @Pass, N'Employee');
                SELECT CAST(SCOPE_IDENTITY() AS int);", conn))
            {
                cmd.Parameters.AddWithValue("@FullName", employee.FullName.Trim());
                cmd.Parameters.AddWithValue("@Email", employee.Email.Trim());
                cmd.Parameters.AddWithValue("@Pass", employee.Pass.Trim()); // הערה: טקסט פשוט; אפשר להחליף ל-Hash בהמשך
                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        // עדכון (Pass אופציונלי)
        public static bool UpdateEmployee(EmployeeDto employee)
        {
            if (employee == null) throw new ArgumentNullException(nameof(employee));
            if (employee.Id <= 0) throw new ArgumentException("Id is required");
            if (string.IsNullOrWhiteSpace(employee.FullName)) throw new ArgumentException("FullName is required");
            if (string.IsNullOrWhiteSpace(employee.Email)) throw new ArgumentException("Email is required");

            bool updatePass = !string.IsNullOrWhiteSpace(employee.Pass);

            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(updatePass
                ? @"UPDATE Users
                    SET FullName = @FullName, Email = @Email, Pass = @Pass
                    WHERE Id = @Id AND Role = N'Employee';"
                : @"UPDATE Users
                    SET FullName = @FullName, Email = @Email
                    WHERE Id = @Id AND Role = N'Employee';", conn))
            {
                cmd.Parameters.AddWithValue("@Id", employee.Id);
                cmd.Parameters.AddWithValue("@FullName", employee.FullName.Trim());
                cmd.Parameters.AddWithValue("@Email", employee.Email.Trim());
                if (updatePass) cmd.Parameters.AddWithValue("@Pass", employee.Pass.Trim());

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public static bool DeleteEmployee(int id)
        {
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                DELETE FROM Users
                WHERE Id = @Id AND Role = N'Employee';", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
        public static CurrentUserDto Authenticate(int id, string pass)
        {
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
                SELECT Id, FullName, Role
                FROM Users
                WHERE Id = @Id AND Pass = @Pass;", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Pass", pass ?? string.Empty);
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                    {
                        return new CurrentUserDto
                        {
                            Id = rd.GetInt32(0),
                            FullName = rd.GetString(1),
                            Role = rd.GetString(2)
                        };
                    }
                }
            }
            return null;
        }
        public static CurrentUserDto FindById(int id)
        {
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
        SELECT Id, FullName, Role, Email,
               CASE WHEN FirstLogin = 1 THEN 1 ELSE 0 END AS FirstLogin,
               Pass
        FROM Users
        WHERE Id = @Id;", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                    {
                        return new CurrentUserDto
                        {
                            Id = rd.GetInt32(0),
                            FullName = rd.IsDBNull(1) ? "" : rd.GetString(1),
                            Role = rd.IsDBNull(2) ? "Employee" : rd.GetString(2),
                            // שדות עזר דרך Session.Temp (נשתמש ב-AuthController)
                        };
                    }
                }
            }
            return null;
        }

        // אימות רגיל (למנהלים/עובדים אחרי השלמה)
        public static CurrentUserDto Authenticate(int id, string pass, out bool isFirstLoginCandidate)
        {
            isFirstLoginCandidate = false;

            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
        SELECT Id, FullName, Role,
               CASE WHEN FirstLogin = 1 THEN 1 ELSE 0 END AS FirstLogin,
               Pass
        FROM Users
        WHERE Id = @Id;", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) return null;

                    var userId = rd.GetInt32(0);
                    var fullName = rd.IsDBNull(1) ? "" : rd.GetString(1);
                    var role = rd.IsDBNull(2) ? "Employee" : rd.GetString(2);
                    var first = rd.GetInt32(3) == 1;
                    var dbPass = rd.IsDBNull(4) ? "" : rd.GetString(4);

                    // אם FirstLogin=true או שאין סיסמה שמורה, נאפשר "כניסה ראשונה"
                    if (first || string.IsNullOrWhiteSpace(dbPass))
                    {
                        isFirstLoginCandidate = true;
                        return new CurrentUserDto { Id = userId, FullName = fullName, Role = role };
                    }

                    // אימות סיסמה רגילה
                    if (string.Equals(dbPass ?? "", pass ?? "", StringComparison.Ordinal))
                    {
                        return new CurrentUserDto { Id = userId, FullName = fullName, Role = role };
                    }

                    return null;
                }
            }
        }

        // השלמת פרטים ראשונה
        public static bool CompleteFirstLogin(int id, string fullName, string email, string pass, out string error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(pass))
            {
                error = "יש למלא שם מלא, אימייל וסיסמה.";
                return false;
            }

            using (var conn = Db.GetConnection())
            {
                conn.Open();

                // 1) בדוק אימייל כפול למשתמש אחר
                using (var check = new SqlCommand(
                    "SELECT COUNT(1) FROM Users WHERE Email = @Email AND Id <> @Id", conn))
                {
                    check.Parameters.AddWithValue("@Email", email.Trim());
                    check.Parameters.AddWithValue("@Id", id);
                    var exists = (int)check.ExecuteScalar() > 0;
                    if (exists)
                    {
                        error = "האימייל כבר קיים במערכת. נא לבחור אימייל אחר.";
                        return false;
                    }
                }

                // 2) עדכן את המשתמש והסר FirstLogin
                using (var cmd = new SqlCommand(@"
            UPDATE Users
            SET FullName = @FullName,
                Email    = @Email,
                Pass     = @Pass,
                FirstLogin = 0
            WHERE Id = @Id;", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@FullName", fullName.Trim());
                    cmd.Parameters.AddWithValue("@Email", email.Trim());
                    cmd.Parameters.AddWithValue("@Pass", pass.Trim());

                    try
                    {
                        return cmd.ExecuteNonQuery() > 0;
                    }
                    catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601) // Unique key violation (גיבוי)
                    {
                        error = "האימייל כבר קיים במערכת. נא לבחור אימייל אחר.";
                        return false;
                    }
                    catch (Exception ex)
                    {
                        error = "שגיאת שרת בעדכון: " + ex.Message;
                        return false;
                    }
                }
            }
        }

        // אופציונלי: "הזמנה" מצד מנהל עם placeholder ייחודי (אם תרצה לאסוף רק מספר עובד ואז העובד ישלים)
        // שימוש: צור משתמש Employee בלי סיסמה וקבע FirstLogin=1
        public static int InviteEmployeeStub(int? externalNumber = null)
        {
            // כדי לא להיתקע ב-UNIQUE של Email, נשים placeholder ייחודי לפי GUID
            var placeholderEmail = $"pending+{Guid.NewGuid():N}@local";
            using (var conn = Db.GetConnection())
            using (var cmd = new SqlCommand(@"
        INSERT INTO Users (FullName, Email, Pass, Role, FirstLogin)
        VALUES (N'', @Email, N'', N'Employee', 1);
        SELECT CAST(SCOPE_IDENTITY() AS int);", conn))
            {
                cmd.Parameters.AddWithValue("@Email", placeholderEmail);
                conn.Open();
                return (int)cmd.ExecuteScalar(); // זהו ה-Id שהעובד יקליד בהתחברות ראשונה
            }
        }
    }
}
    
