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
        public static void SeedAdminAccount(Group6_iCAREDBEntities db)
        {
            string checkAdminQuery = @"
            SELECT COUNT(*) FROM iCAREUser WHERE userName = @userName";
            var adminExists = db.Database.SqlQuery<int>(checkAdminQuery, new SqlParameter("@userName", "admin")).FirstOrDefault();

            if (adminExists == 0)
            {
                string adminID = Guid.NewGuid().ToString();

                string insertUserQuery = @"
                INSERT INTO iCAREUser (ID, userName, name, email, registrationDate, roleID)
                VALUES (@ID, @userName, @name, @Email, @RegistrationDate, @roleID)";

                db.Database.ExecuteSqlCommand(
                    insertUserQuery,
                    new SqlParameter("@ID", adminID),
                    new SqlParameter("@userName", "admin"),
                    new SqlParameter("@name", "Administrator"),
                    new SqlParameter("@Email", "admin@example.com"),
                    new SqlParameter("@RegistrationDate", DateTime.Now),
                    new SqlParameter("@roleID", "1")
                );

                string insertPasswordQuery = @"
                INSERT INTO UserPassword (ID, userName, encryptedPassword, passwordExpiryTime, userAccountExpiryDate)
                VALUES (@ID, @userName, @encryptedPassword, @passwordExpiryTime, @userAccountExpiryDate)";

                string encryptedPassword = UserPassword.StaticEncryptPassword("admin");

                db.Database.ExecuteSqlCommand(
                    insertPasswordQuery,
                    new SqlParameter("@ID", adminID),
                    new SqlParameter("@userName", "admin"),
                    new SqlParameter("@encryptedPassword", encryptedPassword),
                    new SqlParameter("@passwordExpiryTime", (int)DateTimeOffset.Now.AddMonths(6).ToUnixTimeSeconds()),
                    new SqlParameter("@userAccountExpiryDate", DateTime.Now.AddYears(1))
                );
            }
        }
    }
}
