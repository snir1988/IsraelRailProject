using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IsraelRailProject.app.v1.models
{
    public class WorkForm
    {
        public int Id { get; set; }              // מזהה טופס
        public int ManagerId { get; set; }       // מזהה המנהל
        public string Site { get; set; }         // שם/מיקום האתר
        public DateTime WorkDateTime { get; set; } // תאריך ושעת עבודה
        public string WorkType { get; set; }     // סוג העבודה
        public string Status { get; set; }       // Draft / Open / Completed
        public int Version { get; set; }         // מספר גרסה
        public int? OriginalFormId { get; set; } // טופס מקורי לגרסאות
        public DateTime CreatedAt { get; set; }  // תאריך יצירה

        public List<WorkFormEmployee> Employees { get; set; }
        public List<WorkFormRiskItem> RiskItems { get; set; }
    }
}