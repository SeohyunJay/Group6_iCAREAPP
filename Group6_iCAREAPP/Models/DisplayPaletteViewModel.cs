using System;
using System.Collections.Generic;

namespace Group6_iCAREAPP.Models
{
    // ViewModel class to represent patient details and associated documents in the UI
    public class DisplayPaletteViewModel
    {
        public string PatientName { get; set; } // The name of the patient
        public string PatientID { get; set; } // Unique identifier for the patient
        public DateTime? dateOfBirth { get; set; } // Date of birth of the patient (nullable)

        // List of documents related to the patient
        public List<DocumentLink> PatientDocuments { get; set; } = new List<DocumentLink>();
    }

    // Class to represent a link to a document with related metadata
    public class DocumentLink
    {
        public string DocName { get; set; } // Name of the document
        public string DocID { get; set; } // Unique identifier for the document
        public DateTime? DateOfCreation { get; set; } // Date the document was created (nullable)
        public string CreatedBy { get; set; } // Name of the user who created the document
        public DateTime? ModificationDate { get; set; } // Date the document was last modified (nullable)
        public string ModifiedBy { get; set; } // Name of the user who last modified the document
        public string DocumentType { get; set; } // Type or category of the document (e.g., "Lab Report")
    }
}
