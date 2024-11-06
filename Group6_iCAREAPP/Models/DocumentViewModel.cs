using System;

namespace Group6_iCAREAPP.Models
{
    public class DocumentViewModel
    {
        public string DocID { get; set; }
        public string DocName { get; set; }
        public string PatientName { get; set; }
        public DateTime DateOfCreation { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastModified { get; set; }
        public string ModifiedBy { get; set; }
        public string DocumentType { get; set; }
    }
}
