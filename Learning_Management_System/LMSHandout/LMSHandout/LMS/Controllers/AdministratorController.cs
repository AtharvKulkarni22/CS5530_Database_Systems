using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly LMSContext db;

        public AdministratorController(LMSContext _db)
        {
            db = _db;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a department which is uniquely identified by it's subject code
        /// </summary>
        /// <param name="subject">the subject code</param>
        /// <param name="name">the full name of the department</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the department already exists, true otherwise.</returns>
        public IActionResult CreateDepartment(string subject, string name)
        {
            var deptQuery = from d in db.Departments
                            where d.Subject == subject
                            select d;

            var existingDept = deptQuery.FirstOrDefault();
            if (existingDept != null)
            {
                return Json(new { success = false });
            }

            var newDepartment = new Department
            {
                Subject = subject,
                DName = name,
            };

            db.Departments.Add(newDepartment);
            db.SaveChanges();
            
            return Json(new { success = true});
        }


        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subjCode">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {
            var CourseQuery = from course in db.Courses
                              where course.Listing == subject
                              select new 
                              { 
                                  number =  course.Number,
                                  name = course.CName,
                              };

            return Json(CourseQuery.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {
            var professorsQuery = from professor in db.Professors
                                  where professor.WorksIn == subject
                                  select new
                                  {
                                      lname = professor.LName,
                                      fname = professor.FName,
                                      uid = professor.UId,
                                  };

            return Json(professorsQuery.ToArray());
            
        }



        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {     
            var CourseQuery = from c in db.Courses
                                 where c.Number == number && c.Listing == subject
                                 select c;

            var ExistingCourse = CourseQuery.FirstOrDefault();

            if (ExistingCourse != null)
            {
                return Json(new { success = false });
            }

            var newCourse = new Course
            {
                Number = (uint)number,
                CName = name,
                Listing = subject,
            };

            db.Courses.Add(newCourse);
            db.SaveChanges();

            return Json(new { success = true });

        }



        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {
            var CourseQuery = from c in db.Courses
                              where c.Number == number && c.Listing == subject
                              select c;

            var ExistingCourse = CourseQuery.FirstOrDefault();

            if (ExistingCourse == null)
            {
                return Json(new { success = false });
            }


            var ClassQuery = from cl in db.Classes where ExistingCourse.CourseId == cl.OfferingOf
                                  && cl.SemesterSeason == season
                                  && cl.SemesterYear == year
                              select cl;

            var ExistingClass = ClassQuery.FirstOrDefault();

            if (ExistingClass != null)
            {
                return Json(new { success = false });
            }

            var filterConflictQuery = from c in db.Classes
                                      where c.Location == location
                                      && c.SemesterSeason == season
                                      && c.SemesterYear == year
                                      && c.EndTime > TimeOnly.FromDateTime(start)
                                      && c.StartTime < TimeOnly.FromDateTime(end)
                                      select c;


            if (filterConflictQuery.FirstOrDefault() != null)
            {
                return Json(new { success = false });
            }

            var newClass = new Class
            {
                OfferingOf = ExistingCourse.CourseId,
                SemesterSeason = season,
                SemesterYear = (uint)year,
                Location = location,
                StartTime = TimeOnly.FromDateTime(start),
                EndTime = TimeOnly.FromDateTime(end),
                ProfessorId = instructor
            };

            db.Classes.Add(newClass);
            db.SaveChanges();

            return Json(new { success = true });
        }

        /*******End code to modify********/

    }
}

