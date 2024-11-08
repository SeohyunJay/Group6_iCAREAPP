using System;
using System.Collections.Generic;

namespace Group6_iCAREAPP.Models
{
    public class DisplayPaletteViewModel
    {
        public string PatientName { get; set; }
        public string PatientID { get; set; }
        public DateTime? dateOfBirth { get; set; }
        public List<DocumentLink> PatientDocuments { get; set; } = new List<DocumentLink>();

    }

    public class DocumentLink
    {
        public string DocName { get; set; }
        public string DocID { get; set; }
        public DateTime? DateOfCreation { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string ModifiedBy { get; set; }
        public string DocumentType { get; set; }
    }

}
