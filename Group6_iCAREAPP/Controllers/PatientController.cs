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
        private Group6_iCAREDBEntities db = new Group6_iCAREDBEntities();

        // GET: Patient/Index - List all patients
        public ActionResult ManagePatient()
        {
            string sqlQuery = "SELECT * FROM PatientRecord";
            var patients = db.Database.SqlQuery<PatientRecord>(sqlQuery).ToList();
            return View("ManagePatient", patients);
        }

        [HttpGet]
        public ActionResult AddPatient()
        {
            var geoCodes = db.GeoCodes.Select(g => new { g.geoID, g.description }).ToList();
            ViewBag.GeoCodes = new SelectList(geoCodes, "geoID", "description");
            return View("AddPatient");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddPatient(PatientRecord patient)
        {
            if (ModelState.IsValid)
            {
                string generatedID = "PAT-" + Guid.NewGuid().ToString();

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
            var geoCodes = db.GeoCodes.Select(g => new { g.geoID, g.description }).ToList();
            ViewBag.GeoCodes = new SelectList(geoCodes, "geoID", "description");
            return View("AddPatient", patient);
        }

        [HttpGet]
        public ActionResult EditPatient(string id)
        {
            string sqlSelect = "SELECT * FROM PatientRecord WHERE patientID = @ID";
            var patient = db.Database.SqlQuery<PatientRecord>(sqlSelect, new SqlParameter("@ID", id)).FirstOrDefault();

            if (patient == null)
            {
                return HttpNotFound();
            }

            var geoCodes = db.GeoCodes.Select(g => new { g.geoID, g.description }).ToList();
            ViewBag.GeoCodes = new SelectList(geoCodes, "geoID", "description");

            return View("EditPatient", patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPatient(PatientRecord patient)
        {
            if (ModelState.IsValid)
            {
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
            var geoCodes = db.GeoCodes.Select(g => new { g.geoID, g.description }).ToList();
            ViewBag.GeoCodes = new SelectList(geoCodes, "geoID", "description");

            return View("EditPatient", patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePatient(string id)
        {
            string sqlDelete = "DELETE FROM PatientRecord WHERE patientID = @ID";
            db.Database.ExecuteSqlCommand(sqlDelete, new SqlParameter("@ID", id));

            return RedirectToAction("ManagePatient");
        }

        // GET: Patient/AssignPatient - Display a list of patients for assignment
        [HttpGet]
        public ActionResult AssignPatient(string selectedGeoID)
        {
            var geoCodes = db.Database.SqlQuery<GeoCodes>("SELECT geoID, description FROM GeoCodes").ToList();
            ViewBag.GeoCodes = new SelectList(geoCodes, "geoID", "description");

            List<PatientRecord> patients;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignPatient(List<string> selectedPatients)
        {
            string workerID = Session["LoggedUserID"]?.ToString();
            if (string.IsNullOrEmpty(workerID))
            {
                TempData["ErrorMessage"] = "You are not logged in. Please log in first.";
                return RedirectToAction("Login", "Account");
            }

            var workerExists = db.iCAREUser.Any(w => w.ID == workerID);
            if (!workerExists)
            {
                TempData["ErrorMessage"] = "Invalid worker ID. Please contact the administrator.";
                return RedirectToAction("AssignPatient");
            }

            if (selectedPatients == null || !selectedPatients.Any())
            {
                TempData["Message"] = "Please select at least one patient to assign.";
                return RedirectToAction("AssignPatient");
            }

            bool assignmentSuccess = false;
            foreach (var patientID in selectedPatients)
            {
                var alreadyAssigned = db.TreatmentRecord.Any(t => t.workerID == workerID && t.patientID == patientID);
                if (alreadyAssigned)
                {
                    TempData["ErrorMessage"] = $"Worker is already assigned to patient {db.PatientRecord.FirstOrDefault(p => p.patientID == patientID)?.name}.";
                    return RedirectToAction("AssignPatient");
                }

                // Generate treatmentID and assign the patient
                string treatmentID = "TR" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

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

                // Increment the numOfNurses field in PatientRecord table
                var patientRecord = db.PatientRecord.FirstOrDefault(p => p.patientID == patientID);
                if (patientRecord != null)
                {
                    // Ensure numOfNurses doesn't exceed the limit (e.g., 3)
                    if (patientRecord.numOfNurses < 3)
                    {
                        patientRecord.numOfNurses += 1;
                        db.SaveChanges();
                    }
                    else
                    {
                        TempData["ErrorMessage"] = $"Patient {patientRecord.name} already has the maximum number of nurses assigned.";
                    }
                }

                assignmentSuccess = true;
            }

            if (assignmentSuccess)
            {
                TempData["SuccessMessage"] = "Patients assigned successfully.";
            }

            return RedirectToAction("AssignPatient");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeassignPatient(string patientID)
        {
            // Check if the user is logged in and retrieve workerID
            string workerID = Session["LoggedUserID"]?.ToString();
            if (string.IsNullOrEmpty(workerID))
            {
                TempData["ErrorMessage"] = "You are not logged in. Please log in first.";
                return RedirectToAction("Login", "Account");
            }

            // Find the treatment record for this worker and patient
            var treatmentRecord = db.TreatmentRecord.FirstOrDefault(tr => tr.workerID == workerID && tr.patientID == patientID);
            if (treatmentRecord == null)
            {
                TempData["ErrorMessage"] = "No assignment found to deassign.";
                return RedirectToAction("MyBoard");
            }

            // Delete the treatment record
            db.TreatmentRecord.Remove(treatmentRecord);

            // Update the nurse count for the patient
            var patient = db.PatientRecord.FirstOrDefault(p => p.patientID == patientID);
            if (patient != null && patient.numOfNurses > 0)
            {
                patient.numOfNurses -= 1;
                db.Entry(patient).State = System.Data.Entity.EntityState.Modified;
            }

            db.SaveChanges();

            return RedirectToAction("MyBoard");
        }

        [HttpGet]
        public ActionResult MyBoard()
        {
            // Retrieve the logged-in user's name from the session and assign it to ViewBag.UserName
            ViewBag.UserName = Session["LoggedUser"];

            // Your logic to get assigned patients
            string workerID = Session["LoggedUserID"]?.ToString();
            if (workerID == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Fetch assigned patients
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
