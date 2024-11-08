using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Data.Entity;
using Group6_iCAREAPP.Models;
using System.Data.SqlClient;

namespace Group6_iCAREAPP
{
    public class DatabaseInitializer
    {
        // Method to seed the admin account into the database if it doesn't already exist
        public static void SeedAdminAccount(Group6_iCAREDBEntities db)
        {
            // SQL query to check if an admin user already exists in the database
            string checkAdminQuery = @"
            SELECT COUNT(*) FROM iCAREUser WHERE userName = @userName";
            var adminExists = db.Database.SqlQuery<int>(checkAdminQuery, new SqlParameter("@userName", "admin")).FirstOrDefault();

            // If no admin account exists, create one
            if (adminExists == 0)
            {
                // Generate a unique ID for the new admin user
                string adminID = Guid.NewGuid().ToString();

                // SQL query to insert the admin user into the iCAREUser table
                string insertUserQuery = @"
                INSERT INTO iCAREUser (ID, userName, name, email, registrationDate, roleID)
                VALUES (@ID, @userName, @name, @Email, @RegistrationDate, @roleID)";

                // Execute the query to insert the admin user
                db.Database.ExecuteSqlCommand(
                    insertUserQuery,
                    new SqlParameter("@ID", adminID),
                    new SqlParameter("@userName", "admin"),
                    new SqlParameter("@name", "Administrator"),
                    new SqlParameter("@Email", "admin@example.com"),
                    new SqlParameter("@RegistrationDate", DateTime.Now),
                    new SqlParameter("@roleID", "1") // Role ID '1' typically represents an admin role
                );

                // SQL query to insert the admin's password into the UserPassword table
                string insertPasswordQuery = @"
                INSERT INTO UserPassword (ID, userName, encryptedPassword, passwordExpiryTime, userAccountExpiryDate)
                VALUES (@ID, @userName, @encryptedPassword, @passwordExpiryTime, @userAccountExpiryDate)";

                // Encrypt the password "admin" using a static encryption method
                string encryptedPassword = UserPassword.StaticEncryptPassword("admin");

                // Execute the query to insert the encrypted password
                db.Database.ExecuteSqlCommand(
                    insertPasswordQuery,
                    new SqlParameter("@ID", adminID),
                    new SqlParameter("@userName", "admin"),
                    new SqlParameter("@encryptedPassword", encryptedPassword),
                    new SqlParameter("@passwordExpiryTime", (int)DateTimeOffset.Now.AddMonths(6).ToUnixTimeSeconds()), // Password expiry time in Unix format
                    new SqlParameter("@userAccountExpiryDate", DateTime.Now.AddYears(1)) // Account expiry date set to one year from now
                );
            }
        }
    }
}
