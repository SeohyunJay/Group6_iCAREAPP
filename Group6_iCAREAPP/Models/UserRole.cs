//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Group6_iCAREAPP.Models
{
    using System;
    using System.Collections.Generic;

    // Class representing a user role within the iCARE system
    public partial class UserRole
    {
        // Constructor initializing the collection property
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public UserRole()
        {
            this.iCAREUser = new HashSet<iCAREUser>(); // Collection of users associated with this role
        }

        public string ID { get; set; } // Unique identifier for the role
        public string roleName { get; set; } // Name of the role (e.g., "Administrator", "Doctor")

        // Navigation property for the collection of users who have this role
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<iCAREUser> iCAREUser { get; set; }
    }
}
