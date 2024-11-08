using System;
using System.Dynamic;

namespace Group6_iCAREAPP.Models
{
    public class LoginUserModel
    {
        public string ID { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string userName { get; set; }
        public string roleID { get; set; }
        public string roleName { get; set; }
        public string geoID { get; set; }
        public string encryptedPassword { get; set; }
        public string departmentName { get; set; }
        public DateTime? contractExpirationDate { get; set; }
    }

}
