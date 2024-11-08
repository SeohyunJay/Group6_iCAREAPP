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
2. Find the connection string for Group6_iCAREDBEntities (should be line 69).
3. Scroll to the right and find where it says 'data source=DESKTOP-7I4C25N'  
4. Update the data source value with the name of your machine or server (as configured in SQL Server).

## Running the Application
Clean and Build the project in Visual Studio.
### Overview
Run the project, which will open the web browser and take you to our login page.
On the navigation bar you may select the ```About``` tab to get an overview of the iCARE system features and functionalities.
### Admin
To test the admin account, login using these admin credentials:<br>
Username: ```admin```<br>
Password: ```admin```<br>
#### Admin Functions:<br>
- create, view, edit and delete user accounts by selecting ```Manage Users``` on the naviagtion bar.
- create, delete and view roles by selecting ```Manage Roles``` on the navigation bar.
- create, delete and view depertments by selecting ```Manage Departments``` on the navigation bar.
- view the current user information by selecting ```MyInfo``` on the navigation bar.
- to logout of the current user account, select the ```Logout``` tab on the navigation bar.
### Doctor
To test the doctor account, login using these doctor credentials:<br>
Username: ```doctor```<br>
Password: ```doctor```<br>
#### Doctor Functions:
Upon login, you will see the 'Patient List' where you will see all patients and their associated medical information.
##### Manage Patients
- select ```Manage Patients``` on the naviagtion bar to go to the manage patients page
- view the existing patients and their information including name, email, role, department, contract expiration date
- create a new patient with the create patient button
- edit an existing patient's information using the edit button
- delete an existing patient by selecting the delete button and confirming
##### iCARE Board
- select ```iCARE Board``` on the navigation bar to go to iCARE Board
- view the existing patients, as well as their assignment status
- select a patient and select assign to assign them to the current user
- a patient must have one nurse assigned before a doctor can be assigned
- a patient may only have a maximum of 3 nurses assigned
- only one doctor can be assigned to a patient at a time
- select the deassign button to unassign a patient from the current user
##### Display Palette
- select ```Display Palette``` on the navigation bar to go to the Palette view
- existing patients, as well as the status of their documents
- if the patient have no documents, there will be 'no documents' displayed under the document column for that patient
- if the patient has one or more documents, you will see a view documents button under the document column for that patient
- to create a new document click on the add document button on the Palette view
- to view the document, click on the view button which will download the document for you to open it
- to edit the document click on edit button where you can edit the document
- each document saves the user who created it, the date it was created, and the modification history including modifier ID and modification date
- delete a document by clicking the delete button and confirming that document's deletion
##### MyBoard
- select ```MyBoard``` on the navigation bar to My Board
- view all patients that are currently assinged to the current user
- you will be able to see how many nurses each patient has, and if they have a doctor assigned or not
##### MyInfo
- select ```MyInfo``` on the navigation bar to got to the My Info page
- My Info shows the current user's information such as name, email, role, department
##### Logout
- select ```Logout``` on the navigation bar to logout of the current user account
- you will be redirected to the iCARE login page



### Nurse
To test the nurse account, login using these nurse credentials:<br>
Username: ```nurse```<br>
Password: ```nurse```<br>
#### Nurse Functions:
Upon login, you will see the 'Patient List' where you will see all patients and their associated medical information.
##### Manage Patients
- select ```Manage Patients``` on the naviagtion bar to go to the manage patients page
- view the existing patients and their information including name, email, role, department, contract expiration date
- create a new patient with the create patient button
- edit an existing patient's information using the edit button
- delete an existing patient by selecting the delete button and confirming
##### iCARE Board
- select ```iCARE Board``` on the navigation bar to go to iCARE Board
- view the existing patients, as well as their assignment status
- select a patient and select assign to assign them to the current user
- a patient must have one nurse assigned before a doctor can be assigned
- a patient may only have a maximum of 3 nurses assigned
- only one doctor can be assigned to a patient at a time
- select the deassign button to unassign a patient from the current user
##### Display Palette
- select ```Display Palette``` on the navigation bar to go to the Palette view
- existing patients, as well as the status of their documents
- if the patient have no documents, there will be 'no documents' displayed under the document column for that patient
- if the patient has one or more documents, you will see a view documents button under the document column for that patient
- to create a new document click on the add document button on the Palette view
- to view the document, click on the view button which will download the document for you to open it
- to edit the document click on edit button where you can edit the document
- each document saves the user who created it, the date it was created, and the modification history including modifier ID and modification date
- delete a document by clicking the delete button and confirming that document's deletion
##### MyBoard
- select ```MyBoard``` on the navigation bar to My Board
- view all patients that are currently assinged to the current user
- you will be able to see how many nurses each patient has, and if they have a doctor assigned or not
##### MyInfo
- select ```MyInfo``` on the navigation bar to got to the My Info page
- My Info shows the current user's information such as name, email, role, department
##### Logout
- select ```Logout``` on the navigation bar to logout of the current user account
- you will be redirected to the iCARE login page


