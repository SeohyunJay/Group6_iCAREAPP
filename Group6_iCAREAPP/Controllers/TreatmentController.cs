using System;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using Group6_iCAREAPP.Models;

namespace Group6_iCAREAPP.Controllers
{
    public class TreatmentController : Controller
    {
        // Database context for accessing database entities
        private Group6_iCAREDBEntities db = new Group6_iCAREDBEntities();

        // GET: Displays the form to edit a treatment record by treatment ID
        [HttpGet]
        public ActionResult EditTreatment(string treatmentID)
        {
            // Check if the treatment ID is provided
            if (string.IsNullOrEmpty(treatmentID))
            {
                TempData["ErrorMessage"] = "Treatment ID is required.";
                return RedirectToAction("DisplayPalette", "Document"); // Redirects if ID is missing
            }

            // Retrieve the treatment record by ID
            var treatment = db.TreatmentRecord.FirstOrDefault(t => t.treatmentID == treatmentID);
            if (treatment == null)
            {
                TempData["ErrorMessage"] = "Treatment record not found.";
                return RedirectToAction("DisplayPalette", "Document"); // Redirects if treatment not found
            }

            return View(treatment); // Returns the treatment record to the view for editing
        }

        // POST: Updates the treatment record in the database
        [HttpPost]
        [ValidateAntiForgeryToken] // Prevents CSRF attacks
        public ActionResult EditTreatment(TreatmentRecord treatment)
        {
            if (ModelState.IsValid) // Checks if the form data is valid
            {
                // SQL command to update the treatment record
                string sqlUpdate = @"
                    UPDATE TreatmentRecord
                    SET patientID = @patientID, workerID = @workerID, treatmentDate = @treatmentDate, description = @description
                    WHERE treatmentID = @treatmentID";

                // Execute the SQL command to update the record
                db.Database.ExecuteSqlCommand(
                    sqlUpdate,
                    new SqlParameter("@treatmentID", treatment.treatmentID),
                    new SqlParameter("@patientID", treatment.patientID),
                    new SqlParameter("@workerID", treatment.workerID),
                    new SqlParameter("@treatmentDate", treatment.treatmentDate),
                    new SqlParameter("@description", treatment.description)
                );

                TempData["SuccessMessage"] = "Treatment record updated successfully!";
                return RedirectToAction("DisplayPalette", "Document"); // Redirects after a successful update
            }

            return View(treatment); // Returns the view if validation fails
        }

        // GET: Deletes a treatment record by treatment ID
        [HttpGet]
        public ActionResult DeleteTreatment(string treatmentID)
        {
            // Retrieve the treatment record by ID
            var treatment = db.TreatmentRecord.FirstOrDefault(t => t.treatmentID == treatmentID);
            if (treatment == null)
            {
                TempData["ErrorMessage"] = "Treatment record not found.";
                return RedirectToAction("DisplayPalette", "Document"); // Redirects if record not found
            }

            // Remove the treatment record from the database
            db.TreatmentRecord.Remove(treatment);
            db.SaveChanges(); // Save changes to persist the deletion

            TempData["SuccessMessage"] = "Treatment record deleted successfully.";
            return RedirectToAction("DisplayPalette", "Document"); // Redirects after deletion
        }
    }
}
