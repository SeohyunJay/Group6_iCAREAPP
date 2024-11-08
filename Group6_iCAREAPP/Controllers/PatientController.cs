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

        public ActionResult ManagePatient(string selectedGeoID)
        {
            var geoCodes = db.Database.SqlQuery<GeoCodes>("SELECT geoID, description FROM GeoCodes").ToList();
            ViewBag.GeoCodes = new SelectList(geoCodes, "geoID", "description");

            List<PatientRecord> patients;
            if (string.IsNullOrEmpty(selectedGeoID))
            {
                string sqlQuery = "SELECT * FROM PatientRecord";
                patients = db.Database.SqlQuery<PatientRecord>(sqlQuery).ToList();
            }
            else
            {
                string sqlQuery = "SELECT * FROM PatientRecord WHERE geoID = @geoID";
                patients = db.Database.SqlQuery<PatientRecord>(sqlQuery, new SqlParameter("@geoID", selectedGeoID)).ToList();
            }

            ViewBag.SelectedGeoID = selectedGeoID;
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
            string roleID = Session["RoleID"]?.ToString();

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
                    continue;
                }

                var patientRecord = db.PatientRecord.FirstOrDefault(p => p.patientID == patientID);
                if (patientRecord != null)
                {
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
                    else if (roleID == "3") // Nurse
                    {
                        if (patientRecord.numOfNurses < 3)
                        {
                            patientRecord.numOfNurses += 1;
                            db.SaveChanges();

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
            string workerID = Session["LoggedUserID"]?.ToString();
            string roleID = Session["RoleID"]?.ToString();

            if (string.IsNullOrEmpty(workerID))
            {
                TempData["ErrorMessage"] = "You are not logged in. Please log in first.";
                return RedirectToAction("Login", "Account");
            }

            var treatmentRecord = db.TreatmentRecord.FirstOrDefault(t => t.workerID == workerID && t.patientID == patientID);
            if (treatmentRecord == null)
            {
                TempData["ErrorMessage"] = "Assignment not found or already removed.";
                return RedirectToAction("MyBoard", "Patient");
            }

            db.TreatmentRecord.Remove(treatmentRecord);
            db.SaveChanges();

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

                var remainingDoctors = db.TreatmentRecord.Any(t => t.patientID == patientID && t.workerID != workerID && db.iCAREUser.Any(u => u.ID == t.workerID && u.roleID == "2"));
                if (!remainingDoctors)
                {
                    patientRecord.hasDoctor = false;
                }

                db.SaveChanges();
            }

            return RedirectToAction("MyBoard", "Patient");
        }

        [HttpGet]
        public ActionResult MyBoard()
        {
            ViewBag.UserName = Session["LoggedUser"];

            string workerID = Session["LoggedUserID"]?.ToString();
            if (workerID == null)
            {
                return RedirectToAction("Login", "Account");
            }

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
