using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IsraelRailProject.app.v1.models
{
	public class User
	{
        public int Uid { get; set; } // מזהה ייחודי
        public string FullName { get; set; } // שם מלא
        public string Email { get; set; } // כתובת אימייל
        public string Phone { get; set; } // מספר טלפון
        public string Role { get; set; } // תפקיד: 'Manager' או 'Employee'
    }
}