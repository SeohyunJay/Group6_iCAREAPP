using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Group6_iCAREAPP.Models;

namespace Group6_iCAREAPP.Controllers
{
    public class DocumentController : Controller
    {
        private Group6_iCAREDBEntities db = new Group6_iCAREDBEntities();

        // GET: DisplayPalette - List all documents and add button
        //public ActionResult DisplayPalette(string searchQuery)
        //{
        //    ViewBag.Patients = db.PatientRecord.Select(p => new { p.patientID, p.name }).ToList();

        //    // Fetch documents
        //    var documentQuery = db.DocumentMetadata.AsQueryable();
        //    if (!string.IsNullOrEmpty(searchQuery))
        //    {
        //        documentQuery = documentQuery.Where(d => d.docName.Contains(searchQuery) ||
        //                                                 d.patientID.Contains(searchQuery) ||
        //                                                 d.documentType.Contains(searchQuery));
        //    }

        //    var documentResults = documentQuery.OrderByDescending(d => d.dateOfCreation)
        //                                       .Select(d => new DisplayPaletteViewModel
        //                                       {
        //                                           DocID = d.docID,
        //                                           DocName = d.docName,
        //                                           PatientName = db.PatientRecord
        //                                                           .Where(p => p.patientID == d.patientID)
        //                                                           .Select(p => p.name).FirstOrDefault(),
        //                                           DateOfCreation = d.dateOfCreation,
        //                                           DocumentType = d.documentType,
        //                                           CreatedBy = db.iCAREUser
        //                                                         .Where(u => u.ID == d.createdByID)
        //                                                         .Select(u => u.name).FirstOrDefault(),
        //                                           LastModified = d.modificationDate,
        //                                           ModifiedBy = db.iCAREUser
        //                                                         .Where(u => u.ID == d.modifiedByID)
        //                                                         .Select(u => u.name).FirstOrDefault(),
        //                                           IsDocument = true // Set the flag to indicate it's a document
        //                                       }).ToList();

        //    // Fetch treatments
        //    var treatmentQuery = db.TreatmentRecord.AsQueryable();
        //    if (!string.IsNullOrEmpty(searchQuery))
        //    {
        //        treatmentQuery = treatmentQuery.Where(t => t.treatmentID.Contains(searchQuery) ||
        //                                                   t.description.Contains(searchQuery) ||
        //                                                   t.patientID.Contains(searchQuery));
        //    }

        //    var treatmentResults = treatmentQuery.OrderByDescending(t => t.treatmentDate)
        //                                         .Select(t => new DisplayPaletteViewModel
        //                                         {
        //                                             TreatmentID = t.treatmentID,
        //                                             TreatmentDescription = t.description,
        //                                             TreatmentDate = t.treatmentDate,
        //                                             PatientName = db.PatientRecord
        //                                                             .Where(p => p.patientID == t.patientID)
        //                                                             .Select(p => p.name).FirstOrDefault(),
        //                                             WorkerName = db.iCAREUser
        //                                                           .Where(u => u.ID == t.workerID)
        //                                                           .Select(u => u.name).FirstOrDefault(),
        //                                             IsDocument = false // Set the flag to indicate it's a treatment
        //                                         }).ToList();

        //    // Combine results and pass to the view
        //    var combinedResults = documentResults.Concat(treatmentResults).ToList();
        //    return View(combinedResults);
        //}
        public ActionResult DisplayPalette(string searchQuery)
        {
            // Ensure we're always returning a list of DisplayPaletteViewModel
            var documentQuery = db.DocumentMetadata.AsQueryable();
            var treatmentQuery = db.TreatmentRecord.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                documentQuery = documentQuery.Where(d => d.docName.Contains(searchQuery) ||
                                                         d.patientID.Contains(searchQuery) ||
                                                         d.documentType.Contains(searchQuery));
                treatmentQuery = treatmentQuery.Where(t => t.patientID.Contains(searchQuery) ||
                                                           t.description.Contains(searchQuery));
            }

            var documentResults = documentQuery.OrderByDescending(d => d.dateOfCreation)
                .Select(d => new DisplayPaletteViewModel
                {
                    IsDocument = true,
                    DocID = d.docID,
                    PatientName = db.PatientRecord.Where(p => p.patientID == d.patientID).Select(p => p.name).FirstOrDefault(),
                    DateOfCreation = d.dateOfCreation,
                    DocumentType = d.documentType,
                    CreatedBy = db.iCAREUser.Where(u => u.ID == d.createdByID).Select(u => u.name).FirstOrDefault(),
                    LastModified = d.modificationDate,
                    ModifiedBy = db.iCAREUser.Where(u => u.ID == d.modifiedByID).Select(u => u.name).FirstOrDefault(),
                    TreatmentDescription = "-" // Placeholder since this is a document
                }).ToList();

            var treatmentResults = treatmentQuery.OrderByDescending(t => t.treatmentDate)
                .Select(t => new DisplayPaletteViewModel
                {
                    IsDocument = false,
                    TreatmentID = t.treatmentID,
                    PatientName = db.PatientRecord.Where(p => p.patientID == t.patientID).Select(p => p.name).FirstOrDefault(),
                    TreatmentDate = t.treatmentDate,
                    WorkerName = db.iCAREUser.Where(u => u.ID == t.workerID).Select(u => u.name).FirstOrDefault(),
                    TreatmentDescription = t.description,
                    DocumentType = "Treatment", // Placeholder to differentiate
                    DateOfCreation = null, // Placeholder since this is a treatment
                    CreatedBy = null, // Placeholder since this is a treatment
                    LastModified = null, // Placeholder since this is a treatment
                    ModifiedBy = null // Placeholder since this is a treatment
                }).ToList();

            var combinedResults = documentResults.Concat(treatmentResults)
                .OrderByDescending(x => x.DateOfCreation ?? x.TreatmentDate)
                .ToList();

            // Return the list to DisplayPalette view
            return View("DisplayPalette", combinedResults);
        }

        // GET: AddDocument
        [HttpGet]
        public ActionResult AddDocument()
        {
            ViewBag.Patients = new SelectList(db.PatientRecord.Select(p => new { p.patientID, p.name }), "patientID", "name");
            return View();
        }

        // POST: AddDocument
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddDocument(DocumentMetadata document, HttpPostedFileBase uploadedFile)
        {
            if (ModelState.IsValid)
            {
                // Assign values for auto-generated/system-set fields
                document.docID = Guid.NewGuid().ToString();
                document.dateOfCreation = DateTime.Now;
                document.createdByID = Session["LoggedUserID"]?.ToString();
                document.modificationDate = DateTime.Now;
                document.modifiedByID = Session["LoggedUserID"]?.ToString();
                document.documentType = Request.Form["documentType"];  // Make sure this is set in the form

                // Handle file upload
                if (uploadedFile != null && uploadedFile.ContentLength > 0)
                {
                    // Define file path (adjust as needed)
                    string uploadPath = Server.MapPath("~/Uploads");
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }
                    string filePath = Path.Combine(uploadPath, Path.GetFileName(uploadedFile.FileName));

                    // Save the file to the server
                    uploadedFile.SaveAs(filePath);

                    // If you have a field to store the file path or name, save it
                    document.FileName = uploadedFile.FileName;  // Ensure DocumentMetadata has a FileName property
                }

                // Insert new document record into DocumentMetadata table
                string sqlInsert = @"
                    INSERT INTO DocumentMetadata (docID, docName, patientID, dateOfCreation, createdByID, modificationDate, modifiedByID, documentType)
                    VALUES (@docID, @docName, @patientID, @dateOfCreation, @createdByID, @modificationDate, @modifiedByID, @documentType)";

                db.Database.ExecuteSqlCommand(
                    sqlInsert,
                    new SqlParameter("@docID", document.docID),
                    new SqlParameter("@docName", document.docName),
                    new SqlParameter("@patientID", document.patientID),
                    new SqlParameter("@dateOfCreation", document.dateOfCreation),
                    new SqlParameter("@createdByID", document.createdByID),
                    new SqlParameter("@modificationDate", document.modificationDate),
                    new SqlParameter("@modifiedByID", document.modifiedByID),
                    new SqlParameter("@documentType", document.documentType)
                );

                return RedirectToAction("DisplayPalette");
            }

            // If model state is invalid, reload the patient list
            ViewBag.Patients = new SelectList(db.PatientRecord.Select(p => new { p.patientID, p.name }), "patientID", "name");
            ViewBag.DocumentTypes = new SelectList(new List<string> { "Treatment Record", "Lab Report", "Imaging Result", "Prescription" }); // Add more if needed

            return View(document);
        }

        [HttpGet]
        public ActionResult EditDocument(string docID)
        {
            if (string.IsNullOrEmpty(docID))
            {
                return RedirectToAction("DisplayPalette");
            }

            // Retrieve the document by ID
            var document = db.DocumentMetadata.FirstOrDefault(d => d.docID == docID);
            if (document == null)
            {
                TempData["ErrorMessage"] = "Document not found.";
                return RedirectToAction("DisplayPalette");
            }

            // Load patients and document types for dropdowns
            ViewBag.Patients = new SelectList(db.PatientRecord.Select(p => new {
                p.patientID,
                p.name
            }).ToList(), "patientID", "name", document.patientID);

            ViewBag.DocumentTypes = new SelectList(new List<SelectListItem>
            {
            new SelectListItem { Text = "Treatment Record", Value = "Treatment Record" },
            new SelectListItem { Text = "Report", Value = "Report" },
            new SelectListItem { Text = "Other", Value = "Other" }
            }, "Value", "Text", document.documentType);

            // Set the current file name in ViewBag, assuming it is stored in document.docName
            ViewBag.FileName = document.docName;

            return View(document);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDocument(DocumentMetadata document, HttpPostedFileBase uploadedFile, bool deleteFile = false)
        {
            if (ModelState.IsValid)
            {
                // Automatically set "Modified By" and modification date
                document.modifiedByID = Session["LoggedUserID"]?.ToString();
                document.modificationDate = DateTime.Now;

                // Check if "Delete Current File" is selected
                if (deleteFile && !string.IsNullOrEmpty(document.docName))
                {
                    // Define the path for the existing file
                    string existingFilePath = Path.Combine(Server.MapPath("~/Uploads"), document.docName);

                    // Check if the file exists and delete it
                    if (System.IO.File.Exists(existingFilePath))
                    {
                        System.IO.File.Delete(existingFilePath);
                    }

                    // Clear the file reference in the database
                    document.docName = null;
                }

                // If a new file is uploaded, save it
                if (uploadedFile != null && uploadedFile.ContentLength > 0)
                {
                    // Define the path for the new file
                    string filePath = Path.Combine(Server.MapPath("~/Uploads"), Path.GetFileName(uploadedFile.FileName));

                    // Save the new file to the server
                    uploadedFile.SaveAs(filePath);

                    // Update the document's file name in the database
                    document.docName = uploadedFile.FileName;
                }

                // Update the document metadata in the database
                string sqlUpdate = @"
                    UPDATE DocumentMetadata 
                    SET docName = @docName, patientID = @patientID, documentType = @documentType, 
                        modificationDate = @modificationDate, modifiedByID = @modifiedByID 
                    WHERE docID = @docID";

                db.Database.ExecuteSqlCommand(
                    sqlUpdate,
                    new SqlParameter("@docID", document.docID),
                    new SqlParameter("@docName", (object)document.docName ?? DBNull.Value),
                    new SqlParameter("@patientID", document.patientID),
                    new SqlParameter("@documentType", document.documentType),
                    new SqlParameter("@modificationDate", document.modificationDate),
                    new SqlParameter("@modifiedByID", document.modifiedByID)
                );

                return RedirectToAction("DisplayPalette");
            }

            // Reload dropdowns if validation fails
            ViewBag.Patients = new SelectList(db.PatientRecord.ToList(), "patientID", "name", document.patientID);
            ViewBag.DocumentTypes = new SelectList(new List<SelectListItem>
    {
        new SelectListItem { Text = "Treatment Record", Value = "Treatment Record" },
        new SelectListItem { Text = "Lab Report", Value = "Lab Report" },
        new SelectListItem { Text = "Imaging Result", Value = "Imaging Result" },
        new SelectListItem { Text = "Prescription", Value = "Prescription" }
    }, "Value", "Text", document.documentType);

            return View(document);
        }

        [HttpGet]
        public ActionResult DeleteDocument(string docID)
        {
            var document = db.DocumentMetadata.FirstOrDefault(d => d.docID == docID);
            if (document == null)
            {
                TempData["ErrorMessage"] = "Document not found.";
                return RedirectToAction("DisplayPalette");
            }

            db.DocumentMetadata.Remove(document);
            db.SaveChanges();

            return RedirectToAction("DisplayPalette");
        }

    }
}
