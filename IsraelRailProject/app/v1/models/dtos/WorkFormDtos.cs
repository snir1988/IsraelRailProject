using System;
using System.Collections.Generic;

namespace IsraelRailProject.app.v1.models.dtos
{
    // DTO ליצירת טופס עבודה
    public class WorkFormCreateDto
    {
        public int ManagerId { get; set; }
        public string Site { get; set; }
        public DateTime WorkDateTime { get; set; }
        public string WorkType { get; set; }
        public List<int> EmployeeIds { get; set; }
        public List<int> RiskItemIds { get; set; }

    }

    // DTO לעדכון טופס (יוצר גרסה חדשה)
    public class WorkFormUpdateDto
    {
        public string Site { get; set; }
        public DateTime WorkDateTime { get; set; }
        public string WorkType { get; set; }
        public List<int> EmployeeIds { get; set; }
        public List<int> RiskItemIds { get; set; }
    }

    // DTO לבקשת חתימה של עובד על טופס
    public class SignRequestDto
    {
        public int WorkFormId { get; set; }
        public int EmployeeId { get; set; }
    }

    // עובדים לתצוגה/בחירה
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }

        // בהתאם לסכמת הטבלה Users (NOT NULL)
        public string Email { get; set; }
        public string Pass { get; set; } // ביצירה חובה; בעריכה אופציונלי

        // חדש: תפקיד (למשל "Manager" או "Employee")
        public string Role { get; set; }
    }

    // פריטי סיכון לתצוגה/בחירה
    public class RiskItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
