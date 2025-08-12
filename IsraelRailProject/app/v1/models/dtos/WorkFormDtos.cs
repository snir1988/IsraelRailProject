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

        // אופציונלי: עובדים לשיוך לטופס בעת יצירה
        public List<int> EmployeeIds { get; set; }
    }

    // DTO לעדכון טופס (יוצר גרסה חדשה)
    public class WorkFormUpdateDto
    {
        public string Site { get; set; }
        public DateTime WorkDateTime { get; set; }
        public string WorkType { get; set; }

        // אופציונלי: החלפת רשימת עובדים/סיכונים בגרסה החדשה
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
    }

    // פריטי סיכון לתצוגה/בחירה
    public class RiskItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

}
