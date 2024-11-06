using System;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using Group6_iCAREAPP.Models;

namespace Group6_iCAREAPP.Controllers
{
    public class TreatmentController : Controller
    {
        private Group6_iCAREDBEntities db = new Group6_iCAREDBEntities();

        // GET: Treatment/EditTreatment
        [HttpGet]
        public ActionResult EditTreatment(string treatmentID)
        {
            if (string.IsNullOrEmpty(treatmentID))
            {
                TempData["ErrorMessage"] = "Treatment ID is required.";
                return RedirectToAction("DisplayPalette", "Document");
            }

            // Retrieve the treatment record by ID
            var treatment = db.TreatmentRecord.FirstOrDefault(t => t.treatmentID == treatmentID);
            if (treatment == null)
            {
                TempData["ErrorMessage"] = "Treatment record not found.";
                return RedirectToAction("DisplayPalette", "Document");
            }

            return View(treatment); // Ensure this returns the TreatmentRecord model
        }


        // POST: Treatment/EditTreatment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTreatment(TreatmentRecord treatment)
        {
            if (ModelState.IsValid)
            {
                string sqlUpdate = @"
                    UPDATE TreatmentRecord
                    SET patientID = @patientID, workerID = @workerID, treatmentDate = @treatmentDate, description = @description
                    WHERE treatmentID = @treatmentID";

                db.Database.ExecuteSqlCommand(
                    sqlUpdate,
                    new SqlParameter("@treatmentID", treatment.treatmentID),
                    new SqlParameter("@patientID", treatment.patientID),
                    new SqlParameter("@workerID", treatment.workerID),
                    new SqlParameter("@treatmentDate", treatment.treatmentDate),
                    new SqlParameter("@description", treatment.description)
                );

                TempData["SuccessMessage"] = "Treatment record updated successfully!";
                return RedirectToAction("DisplayPalette", "Document");
            }

            // If the model is invalid, return to the edit view with the current data
            return View(treatment);
        }

        [HttpGet]
        public ActionResult DeleteTreatment(string treatmentID)
        {
            var treatment = db.TreatmentRecord.FirstOrDefault(t => t.treatmentID == treatmentID);
            if (treatment == null)
            {
                TempData["ErrorMessage"] = "Treatment record not found.";
                return RedirectToAction("DisplayPalette", "Document");
            }

            db.TreatmentRecord.Remove(treatment);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Treatment record deleted successfully.";
            return RedirectToAction("DisplayPalette", "Document");
        }
    }
}
