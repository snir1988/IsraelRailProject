using System;

namespace IsraelRailProject.app.v1.models.dtos
{
    // DTO להצגת טפסים שממתינים לחתימת העובד
    public class WorkFormForSignDto
    {
        public int Id { get; set; }
        public DateTime WorkDateTime { get; set; }
        public string Site { get; set; }
        public string WorkType { get; set; }
        public string ManagerName { get; set; }
        public DateTime? SignedAt { get; set; }
    }
}
