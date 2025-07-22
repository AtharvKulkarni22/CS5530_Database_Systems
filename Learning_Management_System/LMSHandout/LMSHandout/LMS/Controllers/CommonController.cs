using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    public class CommonController : Controller
    {
        private readonly LMSContext db;

        public CommonController(LMSContext _db)
        {
            db = _db;
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {
            var query = from d in db.Departments
                        select new
                        {
                            name = d.DName,
                            subject = d.Subject
                        };
            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {
            var query = from d in db.Departments
                        select new
                        {
                            subject = d.Subject,
                            dname = d.DName,
                            courses = from c in d.Courses
                                      select new
                                      {
                                          number = c.Number,
                                          cname = c.CName
                                      }
                        };
            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {
            var query = from courses in db.Courses
                        where courses.Number == number && courses.Listing == subject
                        select new
                        {
                            // get the classes for the course
                            classes = from classes in courses.Classes
                                      select new
                                      {
                                          season = classes.SemesterSeason,
                                          year = classes.SemesterYear,
                                          location = classes.Location,
                                          start = classes.StartTime.ToString("HH:mm:ss"),
                                          end = classes.EndTime.ToString("HH:mm:ss"),
                                          // if the professor id is null use empty string for the professor names
                                          fname = (classes.ProfessorId == null ? "" : classes.Professor.FName),
                                          lname = (classes.ProfessorId == null ? "" : classes.Professor.LName)
                                      }
                        };

            foreach (var selectedClasses in query)
            {
                // return the classes for the course
                return Json(selectedClasses.classes.ToArray());
            }

            return Json("");
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {
            var query = from courses in db.Courses
                        where courses.Number == num && courses.Listing == subject
                        select new
                        {
                            // get the class for the course
                            classes = from classes in courses.Classes
                                      where classes.SemesterSeason == season && classes.SemesterYear == year
                                      select new
                                      {
                                          // get the assignment category for the class
                                          assignmentCategories = from assignmentCategories in classes.AssignmentCategories
                                                                 where assignmentCategories.AcName == category
                                                                 select new
                                                                 {
                                                                     // get the contents for the assignment
                                                                     assignmentContents = from assignment in assignmentCategories.Assignments
                                                                                  where assignment.AName == asgname
                                                                                  select assignment.AContents
                                                                 }
                                      }
                        };

            foreach (var selectedClasses in query)
            {
                // look at the assingment category for the class
                foreach (var selectedAssignmentCategories in selectedClasses.classes)
                {
                    // look at the assignment for the assignment category
                    foreach (var selectedAssignments in selectedAssignmentCategories.assignmentCategories)
                    {
                        // look at the contents for the assignment
                        foreach (var selectedContents in selectedAssignments.assignmentContents)
                        {
                            return Content(selectedContents);
                        }
                    }
                }
            }

            return Content("");
        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {
            var query = from sb in db.Submissions
                        join a in db.Assignments on sb.AssignmentId equals a.AssignmentId
                        join asc in db.AssignmentCategories on a.AssignedIn equals asc.AssignmentCategoryId
                        join cl in db.Classes on asc.CategoryOf equals cl.ClassId
                        join c in db.Courses on cl.OfferingOf equals c.CourseId
                        where c.Listing == subject && c.Number == num && cl.SemesterSeason == season && cl.SemesterYear == year
                        && asc.AcName == category && a.AName == asgname && sb.StudentId == uid
                        select sb;

            Submission? submission = query.FirstOrDefault();

           if (submission == null)
           {
                return Content("");
           }
           else
           {
                return Content(submission.SContents);
           }

            
            //return Content("");
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {
            var query = from s in db.Students where s.UId == uid
                        join d in db.Departments on s.Major equals d.Subject
                        select new
                        {
                            fname = s.FName,
                            lname = s.LName,
                            uid = s.UId,
                            department = d.Subject
                        };
            foreach (var selected in query)
            {
                return Json(selected);
            }

            // if uid not found for students try professors
            query = from p in db.Professors where p.UId == uid
                    join d in db.Departments on p.WorksIn equals d.Subject
                    select new
                    {
                        fname = p.FName,
                        lname = p.LName,
                        uid = p.UId,
                        department = d.Subject
                    };
            foreach (var selected in query)
            {
                return Json(selected);
            }

            // if uid not found for students or professors try administrators
            var queryAdmin = from a in db.Administrators where a.UId == uid
                    select new
                    {
                        fname = a.FName,
                        lname = a.LName,
                        uid = a.UId
                    };
            foreach (var selected in queryAdmin)
            {
                return Json(selected);
            }

            // if uid not found
            return Json(new { success = false });
        }


        /*******End code to modify********/
    }
}

