using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IsraelRailProject.app.v1.models
{
    public class WorkFormRiskItem

        {
            public int Id { get; set; }         // מזהה רשומה
            public int WorkFormId { get; set; } // מזהה הטופס
            public int RiskItemId { get; set; } // מזהה פריט הסיכון
        }
    }
