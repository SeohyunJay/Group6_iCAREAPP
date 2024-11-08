using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using Group6_iCAREAPP.Models;

namespace Group6_iCAREAPP.Controllers
{
    public class PatientController : Controller
    {
        // Database context for accessing database entities
        private Group6_iCAREDBEntities db = new Group6_iCAREDBEntities();

        // GET: Displays the Manage Patient page with optional filtering by geoID
        public ActionResult ManagePatient(string selectedGeoID)
        {
            // Retrieve geoCodes for the dropdown list
            var geoCodes = db.Database.SqlQuery<GeoCodes>("SELECT geoID, description FROM GeoCodes").ToList();
            ViewBag.GeoCodes = new SelectList(geoCodes, "geoID", "description");

            List<PatientRecord> patients;
            // Check if a geoID filter is applied
            if (string.IsNullOrEmpty(selectedGeoID))
            {
                // Retrieve all patient records if no filter
                string sqlQuery = "SELECT * FROM PatientRecord";
                patients = db.Database.SqlQuery<PatientRecord>(sqlQuery).ToList();
            }
            else
            {
                // Retrieve patient records filtered by geoID
                string sqlQuery = "SELECT * FROM PatientRecord WHERE geoID = @geoID";
                patients = db.Database.SqlQuery<PatientRecord>(sqlQuery, new SqlParameter("@geoID", selectedGeoID)).ToList();
            }

            ViewBag.SelectedGeoID = selectedGeoID;
            return View("ManagePatient", patients);
        }

        // GET: Displays the Add Patient form
        [HttpGet]
        public ActionResult AddPatient()
        {
            // Populate ViewBag with geoCodes for dropdown list
            var geoCodes = db.GeoCodes.Select(g => new { g.geoID, g.description }).ToList();
            ViewBag.GeoCodes = new SelectList(geoCodes, "geoID", "description");
            return View("AddPatient");
        }

        // POST: Adds a new patient to the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddPatient(PatientRecord patient)
        {
            if (ModelState.IsValid)
            {
                // Generate a unique patient ID
                string generatedID = "PAT-" + Guid.NewGuid().ToString();

                // SQL command to insert a new patient record
                string sqlInsert = @"
                    INSERT INTO PatientRecord (patientID, name, address, dateOfBirth, height, weight, bloodGroup, bedID, treatmentArea, geoID)
                    VALUES (@ID, @name, @address, @dateOfBirth, @height, @weight, @bloodGroup, @BedID, @TreatmentArea, @geoID)";

                db.Database.ExecuteSqlCommand(
                    sqlInsert,
                    new SqlParameter("@ID", generatedID),
                    new SqlParameter("@name", patient.name),
                    new SqlParameter("@address", patient.address),
                    new SqlParameter("@dateOfBirth", patient.dateOfBirth),
                    new SqlParameter("@height", patient.height),
                    new SqlParameter("@weight", patient.weight),
                    new SqlParameter("@bloodGroup", patient.bloodGroup),
                    new SqlParameter("@BedID", patient.bedID),
                    new SqlParameter("@TreatmentArea", patient.treatmentArea),
                    new SqlParameter("@geoID", patient.geoID)
                );

                return RedirectToAction("ManagePatient");
            }

            // Repopulate ViewBag for return to view on validation failure
            var geoCodes = db.GeoCodes.Select(g => new { g.geoID, g.description }).ToList();
            ViewBag.GeoCodes = new SelectList(geoCodes, "geoID", "description");
            return View("AddPatient", patient);
        }

        // GET: Displays the Edit Patient form
        [HttpGet]
        public ActionResult EditPatient(string id)
        {
            // SQL query to get the patient record by ID
            string sqlSelect = "SELECT * FROM PatientRecord WHERE patientID = @ID";
            var patient = db.Database.SqlQuery<PatientRecord>(sqlSelect, new SqlParameter("@ID", id)).FirstOrDefault();

            if (patient == null)
            {
                return HttpNotFound(); // Return if patient not found
            }

            // Populate ViewBag with geoCodes for dropdown list
            var geoCodes = db.GeoCodes.Select(g => new { g.geoID, g.description }).ToList();
            ViewBag.GeoCodes = new SelectList(geoCodes, "geoID", "description");

            return View("EditPatient", patient);
        }

        // POST: Updates an existing patient record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPatient(PatientRecord patient)
        {
            if (ModelState.IsValid)
            {
                // SQL command to update patient details
                string sqlUpdate = @"
                    UPDATE PatientRecord 
                    SET name = @name, address = @address, dateOfBirth = @dateOfBirth, height = @height, 
                        weight = @weight, bloodGroup = @bloodGroup, bedID = @BedID, treatmentArea = @TreatmentArea, geoID = @geoID
                    WHERE patientID = @ID";

                db.Database.ExecuteSqlCommand(
                    sqlUpdate,
                    new SqlParameter("@ID", patient.patientID),
                    new SqlParameter("@name", patient.name),
                    new SqlParameter("@address", patient.address),
                    new SqlParameter("@dateOfBirth", patient.dateOfBirth),
                    new SqlParameter("@height", patient.height),
                    new SqlParameter("@weight", patient.weight),
                    new SqlParameter("@bloodGroup", patient.bloodGroup),
                    new SqlParameter("@BedID", patient.bedID),
                    new SqlParameter("@TreatmentArea", patient.treatmentArea),
                    new SqlParameter("@geoID", patient.geoID)
                );

                return RedirectToAction("ManagePatient");
            }

            // Repopulate ViewBag for return to view on validation failure
            var geoCodes = db.GeoCodes.Select(g => new { g.geoID, g.description }).ToList();
            ViewBag.GeoCodes = new SelectList(geoCodes, "geoID", "description");

            return View("EditPatient", patient);
        }

        // POST: Deletes a patient and related records from the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePatient(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Invalid patient ID.";
                return RedirectToAction("ManagePatient");
            }

            try
            {
                // Delete related records from ModificationHistory, DocumentMetadata, and TreatmentRecord
                string sqlDeleteModificationHistory = @"
                    DELETE FROM ModificationHistory 
                    WHERE docID IN (SELECT docID FROM DocumentMetadata WHERE patientID = @patientID)";
                db.Database.ExecuteSqlCommand(sqlDeleteModificationHistory, new SqlParameter("@patientID", id));

                string sqlDeleteDocumentMetadata = "DELETE FROM DocumentMetadata WHERE patientID = @patientID";
                db.Database.ExecuteSqlCommand(sqlDeleteDocumentMetadata, new SqlParameter("@patientID", id));

                string sqlDeleteTreatmentRecord = "DELETE FROM TreatmentRecord WHERE patientID = @patientID";
                db.Database.ExecuteSqlCommand(sqlDeleteTreatmentRecord, new SqlParameter("@patientID", id));

                // Delete the patient record itself
                string sqlDeletePatient = "DELETE FROM PatientRecord WHERE patientID = @patientID";
                db.Database.ExecuteSqlCommand(sqlDeletePatient, new SqlParameter("@patientID", id));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting patient: {ex.Message}"; // Handle errors
            }

            return RedirectToAction("ManagePatient");
        }

        // GET: Displays the Assign Patient page with optional filtering by geoID
        [HttpGet]
        public ActionResult AssignPatient(string selectedGeoID)
        {
            // Retrieve geoCodes for the dropdown list
            var geoCodes = db.Database.SqlQuery<GeoCodes>("SELECT geoID, description FROM GeoCodes").ToList();
            ViewBag.GeoCodes = new SelectList(geoCodes, "geoID", "description");

            List<PatientRecord> patients;
            // Check if a geoID filter is applied
            if (string.IsNullOrEmpty(selectedGeoID))
            {
                string sqlGetAllPatients = "SELECT * FROM PatientRecord";
                patients = db.Database.SqlQuery<PatientRecord>(sqlGetAllPatients).ToList();
            }
            else
            {
                string sqlGetPatientsByGeoID = "SELECT * FROM PatientRecord WHERE geoID = @geoID";
                patients = db.Database.SqlQuery<PatientRecord>(sqlGetPatientsByGeoID, new SqlParameter("@geoID", selectedGeoID)).ToList();
            }

            ViewBag.SelectedGeoID = selectedGeoID;
            return View("AssignPatient", patients);
        }

        // POST: Assigns selected patients to the logged-in worker
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignPatient(List<string> selectedPatients)
        {
            string workerID = Session["LoggedUserID"]?.ToString();
            string roleID = Session["RoleID"]?.ToString();

            if (string.IsNullOrEmpty(workerID))
            {
                TempData["ErrorMessage"] = "You are not logged in. Please log in first.";
                return RedirectToAction("Login", "Account");
            }

            // Check if the worker ID is valid
            var workerExists = db.iCAREUser.Any(w => w.ID == workerID);
            if (!workerExists)
            {
                TempData["ErrorMessage"] = "Invalid worker ID. Please contact the administrator.";
                return RedirectToAction("AssignPatient");
            }

            // Check if any patients are selected for assignment
            if (selectedPatients == null || !selectedPatients.Any())
            {
                TempData["Message"] = "Please select at least one patient to assign.";
                return RedirectToAction("AssignPatient");
            }

            bool assignmentSuccess = false;

            // Iterate over selected patients and assign them to the worker
            foreach (var patientID in selectedPatients)
            {
                var alreadyAssigned = db.TreatmentRecord.Any(t => t.workerID == workerID && t.patientID == patientID);
                if (alreadyAssigned)
                {
                    TempData["ErrorMessage"] = $"Worker is already assigned to patient {db.PatientRecord.FirstOrDefault(p => p.patientID == patientID)?.name}.";
                    continue;
                }

                var patientRecord = db.PatientRecord.FirstOrDefault(p => p.patientID == patientID);
                if (patientRecord != null)
                {
                    // Assignment logic for a doctor
                    if (roleID == "2") // Doctor
                    {
                        if (patientRecord.hasDoctor == true)
                        {
                            TempData["ErrorMessage"] = $"Patient {patientRecord.name} already has a doctor assigned.";
                            continue;
                        }

                        if (patientRecord.numOfNurses < 1)
                        {
                            TempData["ErrorMessage"] = $"Patient {patientRecord.name} must have at least one nurse assigned before a doctor can be assigned.";
                            continue;
                        }

                        patientRecord.hasDoctor = true;
                        db.SaveChanges();

                        string treatmentID = "TR" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

                        // Insert treatment record for the assignment
                        string sqlInsertAssignment = @"
                            INSERT INTO TreatmentRecord (treatmentID, workerID, patientID, treatmentDate, description) 
                            VALUES (@treatmentID, @workerID, @patientID, @treatmentDate, @description)";

                        db.Database.ExecuteSqlCommand(
                            sqlInsertAssignment,
                            new SqlParameter("@treatmentID", treatmentID),
                            new SqlParameter("@workerID", workerID),
                            new SqlParameter("@patientID", patientID),
                            new SqlParameter("@treatmentDate", DateTime.Now),
                            new SqlParameter("@description", "Assigned patient")
                        );

                        assignmentSuccess = true;
                    }
                    // Assignment logic for a nurse
                    else if (roleID == "3") // Nurse
                    {
                        if (patientRecord.numOfNurses < 3)
                        {
                            patientRecord.numOfNurses += 1;
                            db.SaveChanges();

                            string treatmentID = "TR" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

                            // Insert treatment record for the assignment
                            string sqlInsertAssignment = @"
                            INSERT INTO TreatmentRecord (treatmentID, workerID, patientID, treatmentDate, description) 
                            VALUES (@treatmentID, @workerID, @patientID, @treatmentDate, @description)";

                            db.Database.ExecuteSqlCommand(
                                sqlInsertAssignment,
                                new SqlParameter("@treatmentID", treatmentID),
                                new SqlParameter("@workerID", workerID),
                                new SqlParameter("@patientID", patientID),
                                new SqlParameter("@treatmentDate", DateTime.Now),
                                new SqlParameter("@description", "Assigned patient")
                            );

                            assignmentSuccess = true;
                        }
                        else
                        {
                            TempData["ErrorMessage"] = $"Patient {patientRecord.name} already has the maximum number of nurses assigned.";
                            continue;
                        }
                    }
                }
            }

            // Display success message if at least one assignment was successful
            if (assignmentSuccess)
            {
                TempData["SuccessMessage"] = "Patients assigned successfully.";
            }

            return RedirectToAction("AssignPatient");
        }

        // POST: Deassigns a patient from the logged-in worker
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeassignPatient(string patientID)
        {
            string workerID = Session["LoggedUserID"]?.ToString();
            string roleID = Session["RoleID"]?.ToString();

            if (string.IsNullOrEmpty(workerID))
            {
                TempData["ErrorMessage"] = "You are not logged in. Please log in first.";
                return RedirectToAction("Login", "Account");
            }

            // Find the treatment record for the worker and patient
            var treatmentRecord = db.TreatmentRecord.FirstOrDefault(t => t.workerID == workerID && t.patientID == patientID);
            if (treatmentRecord == null)
            {
                TempData["ErrorMessage"] = "Assignment not found or already removed.";
                return RedirectToAction("MyBoard", "Patient");
            }

            // Remove the treatment record
            db.TreatmentRecord.Remove(treatmentRecord);
            db.SaveChanges();

            // Update patient record based on the role
            var patientRecord = db.PatientRecord.FirstOrDefault(p => p.patientID == patientID);
            if (patientRecord != null)
            {
                if (roleID == "3") // Nurse
                {
                    if (patientRecord.numOfNurses > 0)
                    {
                        patientRecord.numOfNurses -= 1;
                    }
                }
                else if (roleID == "2") // Doctor
                {
                    patientRecord.hasDoctor = false;
                }

                // Check if any other doctors remain assigned to the patient
                var remainingDoctors = db.TreatmentRecord.Any(t => t.patientID == patientID && t.workerID != workerID && db.iCAREUser.Any(u => u.ID == t.workerID && u.roleID == "2"));
                if (!remainingDoctors)
                {
                    patientRecord.hasDoctor = false;
                }

                db.SaveChanges();
            }

            return RedirectToAction("MyBoard", "Patient");
        }

        // GET: Displays the board with patients assigned to the logged-in worker
        [HttpGet]
        public ActionResult MyBoard()
        {
            ViewBag.UserName = Session["LoggedUser"];

            string workerID = Session["LoggedUserID"]?.ToString();
            if (workerID == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Retrieve patients assigned to the logged-in worker
            var patients = db.Database.SqlQuery<PatientRecord>(
                "SELECT p.* FROM PatientRecord p " +
                "JOIN TreatmentRecord t ON p.patientID = t.patientID " +
                "WHERE t.workerID = @workerID",
                new SqlParameter("@workerID", workerID)
            ).ToList();

            return View("MyBoard", patients);
        }
    }
}
