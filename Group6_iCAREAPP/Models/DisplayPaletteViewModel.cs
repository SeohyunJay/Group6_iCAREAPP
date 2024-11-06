//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;

//namespace Group6_iCAREAPP.Models
//{
//    public class DisplayPaletteViewModel
//    {
//        // Fields for Document Metadata
//        public string DocID { get; set; }
//        public string DocName { get; set; }
//        public string PatientName { get; set; }
//        public DateTime? DateOfCreation { get; set; }
//        public string DocumentType { get; set; }
//        public string CreatedBy { get; set; }
//        public DateTime? LastModified { get; set; }
//        public string ModifiedBy { get; set; }

//        // Fields for Treatment Record
//        public string TreatmentID { get; set; }
//        public string TreatmentDescription { get; set; }
//        public DateTime? TreatmentDate { get; set; }
//        public string WorkerName { get; set; }
//        public string Description { get; set; }

//        // Flag to differentiate between Document and Treatment records
//        public bool IsDocument { get; set; }
//    }

//}
using System;

namespace Group6_iCAREAPP.Models
{
    public class DisplayPaletteViewModel
    {
        public bool IsDocument { get; set; }
        public string DocID { get; set; }
        public string TreatmentID { get; set; }
        public string PatientName { get; set; }
        public string DocumentType { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? DateOfCreation { get; set; }
        public DateTime? LastModified { get; set; }
        public string ModifiedBy { get; set; }
        public string WorkerName { get; set; }
        public DateTime? TreatmentDate { get; set; }
        public string TreatmentDescription { get; set; }
        public string Description { get; set; }
    }

}
