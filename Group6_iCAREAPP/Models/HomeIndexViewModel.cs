using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Group6_iCAREAPP.Models
{
    public class HomeIndexViewModel
    {

        public string RoleID { get; set; }
        public string RoleName { get; set; }
        public List<PatientRecord> Patients { get; set; }
        public IEnumerable<LoginUserModel> Users { get; set; }

    }
}
