using System;

namespace IsraelRailProject.app.v1.models.dtos
{
    public class LoginDto
    {
        public int EmployeeId { get; set; }
        public string Pass { get; set; }
    }

    public class CurrentUserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; } // "Manager" / "Employee"
    }
}
