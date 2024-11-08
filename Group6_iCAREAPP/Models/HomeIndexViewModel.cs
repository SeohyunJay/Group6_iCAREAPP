using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Group6_iCAREAPP.Models
{
    // ViewModel class to represent data for the home/index view
    public class HomeIndexViewModel
    {
        public string RoleID { get; set; } // The ID representing the role of the logged-in user (e.g., "1" for admin)
        public string RoleName { get; set; } // The name of the role of the logged-in user (e.g., "Administrator")
        public List<PatientRecord> Patients { get; set; } // List of patient records to display if the user has relevant permissions
        public IEnumerable<LoginUserModel> Users { get; set; } // Collection of user models to display if the user is an admin
    }
}
