# CS 4320 Project 2 - Group 6
### Contributors:
- Jay Kim
- Connor Joyce
- Spencer Loy
- Isaac Rider
- Trenton Roney

## Project Overview
We implemented the iCARE system using the ASP.NET framework, leveraging Microsoft Visual Studio as our code editor 
and Microsoft SQL Server to host our database.

## Setting Up the Database
To test our application, you will need to set up the database first:
1. Download the ```Group6_iCAREDB.bak``` file (found in our .zip), which is a backup of our database.
2. Open SQL Server Management Studio, right-click on Databases, and select Restore Database.
3. Find the Group6_iCAREDB.bak file (included in our zip folder) and restore it.
4. Once restored, you will see data populated in each of the tables.

## Connecting to the Database
1. Open the project in Visual Studio.
2. Delete any connections that are already displayed in the project.
3. Add a connection to the restored database (Group6_iCAREDB) from within Visual Studio.


## Configuring Parameters
1. Open the Web.config file in the project.
2. Find the connection string for Group6_iCAREDBEntities (should be line 64).
3. Update the data source value with the name of your machine or server (as configured in SQL Server).

## Running the Application
1. Clean and Build the project in Visual Studio.
2. Run the project and login using these admin credentials:<br><br>
Username: ```admin```<br>
Password: ```admin```<br><br>
4. As an admin, you can test the functionality like managing users.
5. To test the doctor functionality you may login using these credentials:<br><br>
Username: ```doctor```<br>
Password: ```doctor```<br><br>
6. As a doctor you can test functionality such as iCARE Board, Manage Patients, My Board, Assign Patients or Display Palette.
7. To test the nurse functinality you may login using these credentials:<br><br>
Username: ```nurse```<br>
Password: ```nurse```<br><br>
8. As a nurse you can test functionality such as iCARE Board, Manage Patients, My Board, Assign Patients or Display Palette.

