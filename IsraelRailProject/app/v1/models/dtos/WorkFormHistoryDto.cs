using System;

namespace IsraelRailProject.app.v1.models.dtos
{
    public class WorkFormHistoryDto
    {
        public int Id { get; set; }
        public DateTime WorkDateTime { get; set; }
        public string Site { get; set; }
        public string WorkType { get; set; }
        public string ManagerName { get; set; }
        public int EmployeesCount { get; set; }
        public int SignedCount { get; set; }
    }
}
