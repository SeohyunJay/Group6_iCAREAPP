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
        private Group6_iCAREDBEntities db = new Group6_iCAREDBEntities();

        public ActionResult DisplayPalette(string searchQuery)
        {
            var patientQuery = db.PatientRecord.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                patientQuery = patientQuery.Where(p => p.name.Contains(searchQuery) ||
                                                       db.DocumentMetadata.Any(d => d.patientID == p.patientID && d.docName.Contains(searchQuery)) ||
                                                       db.TreatmentRecord.Any(t => t.patientID == p.patientID && t.description.Contains(searchQuery)));
            }

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
        
        [HttpGet]
        public ActionResult AddDocument(string patientID)
        {
            ViewBag.Patients = new SelectList(db.PatientRecord.Select(p => new { p.patientID, p.name }), "patientID", "name");
            ViewBag.Drugs = new SelectList(db.DrugsDictionary.Select(d => new { d.drugID, d.drugName }), "drugID", "drugName");

            ViewBag.SelectedPatientID = patientID;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddDocument(DocumentMetadata document, HttpPostedFileBase uploadedFile, string selectedDrugID)
        {
            if (ModelState.IsValid)
            {
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
                    TempData["ErrorMessage"] = "File upload is required.";
                    return View(document);
                }

                document.docID = Guid.NewGuid().ToString();
                document.dateOfCreation = DateTime.Now;
                document.createdByID = Session["LoggedUserID"]?.ToString();
                document.modificationDate = DateTime.Now;
                document.modifiedByID = Session["LoggedUserID"]?.ToString();
                document.documentType = Request.Form["documentType"];
                document.drugID = string.IsNullOrEmpty(selectedDrugID) ? null : selectedDrugID;

                if (uploadedFile != null && uploadedFile.ContentLength > 0)
                {
                    string uploadPath = Server.MapPath("~/Uploads");
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }
                    string filePath = Path.Combine(uploadPath, Path.GetFileName(uploadedFile.FileName));

                    uploadedFile.SaveAs(filePath);

                    document.FileName = uploadedFile.FileName;
                }

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

            ViewBag.Patients = new SelectList(db.PatientRecord.Select(p => new { p.patientID, p.name }), "patientID", "name");
            ViewBag.DocumentTypes = new SelectList(new List<string> { "Treatment Record", "Lab Report", "Imaging Result", "Prescription" }); // Add more if needed
            ViewBag.Drugs = new SelectList(db.DrugsDictionary.Select(d => new { d.drugID, d.drugName }), "drugID", "drugName");

            return View(document);
        }

        [HttpGet]
        public ActionResult EditDocument(string docID)
        {
            if (string.IsNullOrEmpty(docID))
            {
                return RedirectToAction("DisplayPalette");
            }

            var document = db.DocumentMetadata.FirstOrDefault(d => d.docID == docID);
            if (document == null)
            {
                TempData["ErrorMessage"] = "Document not found.";
                return RedirectToAction("DisplayPalette");
            }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDocument(DocumentMetadata document, HttpPostedFileBase uploadedFile, bool deleteFile = false)
        {
            if (ModelState.IsValid)
            {
                document.modifiedByID = Session["LoggedUserID"]?.ToString();
                document.modificationDate = DateTime.Now;

                if (deleteFile && !string.IsNullOrEmpty(document.docName))
                {
                    string existingFilePath = Path.Combine(Server.MapPath("~/Uploads"), document.docName);
                    if (System.IO.File.Exists(existingFilePath))
                    {
                        System.IO.File.Delete(existingFilePath);
                    }
                    document.docName = null;
                }

                if (uploadedFile != null && uploadedFile.ContentLength > 0)
                {
                    string filePath = Path.Combine(Server.MapPath("~/Uploads"), Path.GetFileName(uploadedFile.FileName));
                    uploadedFile.SaveAs(filePath);
                    document.docName = uploadedFile.FileName;
                }

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
            if (string.IsNullOrEmpty(docID))
            {
                TempData["ErrorMessage"] = "Invalid document ID.";
                return RedirectToAction("DisplayPalette");
            }

            string sqlDeleteModificationHistory = "DELETE FROM ModificationHistory WHERE docID = @docID";
            db.Database.ExecuteSqlCommand(sqlDeleteModificationHistory, new SqlParameter("@docID", docID));

            string sqlDeleteDocument = "DELETE FROM DocumentMetadata WHERE docID = @docID";
            db.Database.ExecuteSqlCommand(sqlDeleteDocument, new SqlParameter("@docID", docID));

            return RedirectToAction("DisplayPalette");
        }

        public ActionResult ViewPatientDocuments(string patientID)
        {
            var patient = db.PatientRecord.FirstOrDefault(p => p.patientID == patientID);
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction("ManagePatient");
            }

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

            var viewModel = new DisplayPaletteViewModel
            {
                PatientName = patient.name,
                PatientID = patient.patientID,
                dateOfBirth = patient.dateOfBirth,
                PatientDocuments = patientDocuments
            };

            return View(viewModel);
        }

        [HttpGet]
        public ActionResult ViewDocument(string docID)
        {
            if (string.IsNullOrEmpty(docID))
            {
                TempData["ErrorMessage"] = "Document ID is required.";
                return RedirectToAction("DisplayPalette");
            }

            var document = db.DocumentMetadata.FirstOrDefault(d => d.docID == docID);
            if (document == null)
            {
                TempData["ErrorMessage"] = "Document not found.";
                return RedirectToAction("DisplayPalette");
            }

            string filePath = Path.Combine(Server.MapPath("~/Uploads/"), document.docName);
            if (!System.IO.File.Exists(filePath))
            {
                TempData["ErrorMessage"] = "File not found on the server.";
                return RedirectToAction("DisplayPalette");
            }

            string fileExtension = Path.GetExtension(filePath).ToLower();
            string contentType = GetContentType(fileExtension);

            return File(filePath, contentType, document.docName);
        }
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
