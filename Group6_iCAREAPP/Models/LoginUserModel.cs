using System;
using System.Dynamic;

namespace Group6_iCAREAPP.Models
{
    // Model class to represent a user with login and role details
    public class LoginUserModel
    {
        public string ID { get; set; } // Unique identifier for the user
        public string name { get; set; } // Full name of the user
        public string email { get; set; } // Email address of the user
        public string userName { get; set; } // Username for login purposes
        public string roleID { get; set; } // ID representing the user's role (e.g., "1" for admin)
        public string roleName { get; set; } // Name of the user's role (e.g., "Administrator")
        public string geoID { get; set; } // Identifier for the user's geographic or department location
        public string encryptedPassword { get; set; } // The encrypted version of the user's password
        public string departmentName { get; set; } // Name of the department the user belongs to
        public DateTime? contractExpirationDate { get; set; } // Expiration date for the user's contract (nullable)
    }
}
