using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IsraelRailProject.app.v1.models
{
    public class Signature
    {
        public int Id { get; set; }         // מזהה חתימה
        public int WorkFormId { get; set; } // מזהה הטופס
        public int EmployeeId { get; set; } // מזהה העובד
        public DateTime SignedAt { get; set; } // מתי נחתם
        public int Version { get; set; }    // גרסת הטופס עליו חתם
    }
}