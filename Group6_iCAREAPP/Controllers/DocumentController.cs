using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
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
        // Database context for accessing database entities
        private Group6_iCAREDBEntities db = new Group6_iCAREDBEntities();

        // Displays a list of patients and their documents based on a search query
        public ActionResult DisplayPalette(string searchQuery)
        {
            var patientQuery = db.PatientRecord.AsQueryable();

            // Filter patient records based on the search query
            if (!string.IsNullOrEmpty(searchQuery))
            {
                patientQuery = patientQuery.Where(p => p.name.Contains(searchQuery) ||
                                                       db.DocumentMetadata.Any(d => d.patientID == p.patientID && d.docName.Contains(searchQuery)) ||
                                                       db.TreatmentRecord.Any(t => t.patientID == p.patientID && t.description.Contains(searchQuery)));
            }

            // Project patient results and related documents into a view model
            var patientResults = patientQuery.Select(p => new DisplayPaletteViewModel
            {
                PatientName = p.name,
                PatientID = p.patientID,
                dateOfBirth = p.dateOfBirth,

                PatientDocuments = db.DocumentMetadata
                         .Where(d => d.patientID == p.patientID)
                         .Join(db.iCAREUser, d => d.createdByID, u => u.ID, (d, u) => new { d, u })
                         .Join(db.iCAREUser, temp => temp.d.modifiedByID, u => u.ID, (temp, u) => new { temp.d, temp.u, modifiedBy = u })
                         .Select(temp => new DocumentLink
                         {
                             DocName = temp.d.docName,
                             DocID = temp.d.docID.ToString(),
                             DateOfCreation = temp.d.dateOfCreation,
                             CreatedBy = temp.u.name,
                             ModificationDate = temp.d.modificationDate,
                             ModifiedBy = temp.modifiedBy.name,
                             DocumentType = temp.d.documentType
                         })
                         .ToList()
            }).ToList();

            return View("DisplayPalette", patientResults);
        }

        // GET: Display the form to add a new document
        [HttpGet]
        public ActionResult AddDocument(string patientID)
        {
            // Populate ViewBag with patient and drug options for the form
            ViewBag.Patients = new SelectList(db.PatientRecord.Select(p => new { p.patientID, p.name }), "patientID", "name");
            ViewBag.Drugs = new SelectList(db.DrugsDictionary.Select(d => new { d.drugID, d.drugName }), "drugID", "drugName");
            ViewBag.SelectedPatientID = patientID;

            return View();
        }

        // POST: Add a new document with file upload functionality
        [HttpPost]
        [ValidateAntiForgeryToken] // Prevents CSRF attacks
        public ActionResult AddDocument(DocumentMetadata document, HttpPostedFileBase uploadedFile, string selectedDrugID)
        {
            if (ModelState.IsValid)
            {
                // Handle file upload and save to server
                if (uploadedFile != null && uploadedFile.ContentLength > 0)
                {
                    string uploadPath = Server.MapPath("~/Uploads/");
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    string fileName = Path.GetFileName(uploadedFile.FileName);
                    string filePath = Path.Combine(uploadPath, fileName);
                    uploadedFile.SaveAs(filePath);

                    document.docName = fileName;
                }
                else
                {
                    // Display an error if the file upload is missing
                    TempData["ErrorMessage"] = "File upload is required.";
                    return View(document);
                }

                // Set document metadata properties
                document.docID = Guid.NewGuid().ToString();
                document.dateOfCreation = DateTime.Now;
                document.createdByID = Session["LoggedUserID"]?.ToString();
                document.modificationDate = DateTime.Now;
                document.modifiedByID = Session["LoggedUserID"]?.ToString();
                document.documentType = Request.Form["documentType"];
                document.drugID = string.IsNullOrEmpty(selectedDrugID) ? null : selectedDrugID;

                // Insert document metadata into the database
                string sqlInsert = @"
                    INSERT INTO DocumentMetadata (docID, docName, patientID, dateOfCreation, createdByID, modificationDate, modifiedByID, documentType, drugID)
                    VALUES (@docID, @docName, @patientID, @dateOfCreation, @createdByID, @modificationDate, @modifiedByID, @documentType, @drugID)";

                db.Database.ExecuteSqlCommand(
                    sqlInsert,
                    new SqlParameter("@docID", document.docID),
                    new SqlParameter("@docName", document.docName),
                    new SqlParameter("@patientID", document.patientID),
                    new SqlParameter("@dateOfCreation", document.dateOfCreation),
                    new SqlParameter("@createdByID", document.createdByID),
                    new SqlParameter("@modificationDate", document.modificationDate),
                    new SqlParameter("@modifiedByID", document.modifiedByID),
                    new SqlParameter("@documentType", document.documentType),
                    new SqlParameter("@drugID", (object)document.drugID ?? DBNull.Value)
                );

                return RedirectToAction("DisplayPalette");
            }

            // Repopulate dropdowns for return to view in case of validation failure
            ViewBag.Patients = new SelectList(db.PatientRecord.Select(p => new { p.patientID, p.name }), "patientID", "name");
            ViewBag.DocumentTypes = new SelectList(new List<string> { "Treatment Record", "Lab Report", "Imaging Result", "Prescription" }); // Add more if needed
            ViewBag.Drugs = new SelectList(db.DrugsDictionary.Select(d => new { d.drugID, d.drugName }), "drugID", "drugName");

            return View(document);
        }

        // GET: Display the form to edit an existing document
        [HttpGet]
        public ActionResult EditDocument(string docID)
        {
            // Redirect if docID is not provided
            if (string.IsNullOrEmpty(docID))
            {
                return RedirectToAction("DisplayPalette");
            }

            // Retrieve the document to be edited
            var document = db.DocumentMetadata.FirstOrDefault(d => d.docID == docID);
            if (document == null)
            {
                TempData["ErrorMessage"] = "Document not found.";
                return RedirectToAction("DisplayPalette");
            }

            // Populate ViewBag with patient and document type options
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

            ViewBag.FileName = document.docName;

            return View(document);
        }

        // POST: Save changes to an edited document
        [HttpPost]
        [ValidateAntiForgeryToken] // Prevents CSRF attacks
        public ActionResult EditDocument(DocumentMetadata document, HttpPostedFileBase uploadedFile, bool deleteFile = false)
        {
            if (ModelState.IsValid)
            {
                // Update document modification details
                document.modifiedByID = Session["LoggedUserID"]?.ToString();
                document.modificationDate = DateTime.Now;

                // Handle file deletion if requested
                if (deleteFile && !string.IsNullOrEmpty(document.docName))
                {
                    string existingFilePath = Path.Combine(Server.MapPath("~/Uploads"), document.docName);
                    if (System.IO.File.Exists(existingFilePath))
                    {
                        System.IO.File.Delete(existingFilePath);
                    }
                    document.docName = null;
                }

                // Handle new file upload if provided
                if (uploadedFile != null && uploadedFile.ContentLength > 0)
                {
                    string filePath = Path.Combine(Server.MapPath("~/Uploads"), Path.GetFileName(uploadedFile.FileName));
                    uploadedFile.SaveAs(filePath);
                    document.docName = uploadedFile.FileName;
                }

                // Update document details in the database
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

                // Insert a modification history record
                string modID = Guid.NewGuid().ToString();
                string sqlInsertModificationHistory = @"
                    INSERT INTO ModificationHistory (modID, docID, description, modificationDate)
                    VALUES (@modID, @docID, @description, @modificationDate)";

                db.Database.ExecuteSqlCommand(
                    sqlInsertModificationHistory,
                    new SqlParameter("@modID", modID),
                    new SqlParameter("@docID", document.docID),
                    new SqlParameter("@description", "Document modified"),
                    new SqlParameter("@modificationDate", document.modificationDate)
                );

                return RedirectToAction("DisplayPalette");
            }

            // Repopulate ViewBag dropdowns in case of validation failure
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

        // GET: Delete a document by ID
        [HttpGet]
        public ActionResult DeleteDocument(string docID)
        {
            // Check if document ID is valid
            if (string.IsNullOrEmpty(docID))
            {
                TempData["ErrorMessage"] = "Invalid document ID.";
                return RedirectToAction("DisplayPalette");
            }

            // Delete modification history and document record
            string sqlDeleteModificationHistory = "DELETE FROM ModificationHistory WHERE docID = @docID";
            db.Database.ExecuteSqlCommand(sqlDeleteModificationHistory, new SqlParameter("@docID", docID));

            string sqlDeleteDocument = "DELETE FROM DocumentMetadata WHERE docID = @docID";
            db.Database.ExecuteSqlCommand(sqlDeleteDocument, new SqlParameter("@docID", docID));

            return RedirectToAction("DisplayPalette");
        }

        // GET: View documents for a specific patient
        public ActionResult ViewPatientDocuments(string patientID)
        {
            // Check if the patient exists
            var patient = db.PatientRecord.FirstOrDefault(p => p.patientID == patientID);
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction("ManagePatient");
            }

            // Retrieve documents related to the patient
            var patientDocuments = db.DocumentMetadata
                .Where(d => d.patientID == patientID)
                .Join(db.iCAREUser, d => d.createdByID, u => u.ID, (d, createdBy) => new { d, createdBy })
                .Join(db.iCAREUser, combined => combined.d.modifiedByID, u => u.ID, (combined, modifiedBy) => new DocumentLink
                {
                    DocID = combined.d.docID,
                    DocName = combined.d.docName,
                    DateOfCreation = combined.d.dateOfCreation,
                    CreatedBy = combined.createdBy.name,
                    ModificationDate = combined.d.modificationDate,
                    ModifiedBy = modifiedBy.name,
                    DocumentType = combined.d.documentType
                }).ToList();

            // Create and return the view model
            var viewModel = new DisplayPaletteViewModel
            {
                PatientName = patient.name,
                PatientID = patient.patientID,
                dateOfBirth = patient.dateOfBirth,
                PatientDocuments = patientDocuments
            };

            return View(viewModel);
        }

        // GET: View a specific document by ID
        [HttpGet]
        public ActionResult ViewDocument(string docID)
        {
            // Check if document ID is provided
            if (string.IsNullOrEmpty(docID))
            {
                TempData["ErrorMessage"] = "Document ID is required.";
                return RedirectToAction("DisplayPalette");
            }

            // Retrieve the document metadata
            var document = db.DocumentMetadata.FirstOrDefault(d => d.docID == docID);
            if (document == null)
            {
                TempData["ErrorMessage"] = "Document not found.";
                return RedirectToAction("DisplayPalette");
            }

            // Get the file path and check if it exists on the server
            string filePath = Path.Combine(Server.MapPath("~/Uploads/"), document.docName);
            if (!System.IO.File.Exists(filePath))
            {
                TempData["ErrorMessage"] = "File not found on the server.";
                return RedirectToAction("DisplayPalette");
            }

            // Determine the content type based on the file extension
            string fileExtension = Path.GetExtension(filePath).ToLower();
            string contentType = GetContentType(fileExtension);

            // Return the file for download/view
            return File(filePath, contentType, document.docName);
        }

        // Helper method to get content type based on file extension
        private string GetContentType(string fileExtension)
        {
            switch (fileExtension)
            {
                case ".pdf": return "application/pdf";
                case ".doc": return "application/msword";
                case ".docx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".txt": return "text/plain";
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".png": return "image/png";
                case ".gif": return "image/gif";
                default: return "application/octet-stream";
            }
        }
    }
}
