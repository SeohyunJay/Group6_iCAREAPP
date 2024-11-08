using System;

namespace Group6_iCAREAPP.Models
{
    // ViewModel class to represent the details of a document for use in the UI
    public class DocumentViewModel
    {
        public string DocID { get; set; } // Unique identifier for the document
        public string DocName { get; set; } // Name of the document
        public string PatientName { get; set; } // Name of the patient associated with the document
        public DateTime DateOfCreation { get; set; } // Date when the document was created
        public string CreatedBy { get; set; } // Name of the user who created the document
        public DateTime? LastModified { get; set; } // Date when the document was last modified (nullable)
        public string ModifiedBy { get; set; } // Name of the user who last modified the document
        public string DocumentType { get; set; } // Type or category of the document (e.g., "Prescription", "Report")
        public string DrugID { get; set; } // Identifier for any drug associated with the document (if applicable)
    }
}
