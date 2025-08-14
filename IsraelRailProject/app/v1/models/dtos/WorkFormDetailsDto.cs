using System;
using System.Collections.Generic;

namespace IsraelRailProject.app.v1.models.dtos
{
    public class WorkFormEmployeeSignDto
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; }
        public DateTime? SignedAt { get; set; }
    }

    public class WorkFormDetailsDto
    {
        public int Id { get; set; }
        public DateTime WorkDateTime { get; set; }
        public string Site { get; set; }
        public string WorkType { get; set; }
        public int ManagerId { get; set; }
        public string ManagerName { get; set; }

        public List<WorkFormEmployeeSignDto> Employees { get; set; } = new List<WorkFormEmployeeSignDto>();

        // חדש: הסיכונים ששויכו לטופס
        public List<RiskItemDto> Risks { get; set; } = new List<RiskItemDto>();
    }
}
