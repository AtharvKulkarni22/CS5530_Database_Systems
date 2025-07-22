using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = from e in db.Enrollments where e.StudentId == uid
                        join cl in db.Classes on e.ClassId equals cl.ClassId
                        join co in db.Courses on cl.OfferingOf equals co.CourseId
                        join d in db.Departments on co.Listing equals d.Subject
                        select new
                        {
                            subject = d.Subject,
                            number = co.Number,
                            name = co.CName,
                            season = cl.SemesterSeason,
                            year = cl.SemesterYear,
                            grade = e.Grade
                        };
            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            var assignmentsQuery = from assignment in db.Assignments
                                   join category in db.AssignmentCategories on assignment.AssignedIn equals category.AssignmentCategoryId
                                   join courseClass in db.Classes on category.CategoryOf equals courseClass.ClassId
                                   join course in db.Courses on courseClass.OfferingOf equals course.CourseId
                                   where course.Listing == subject &&
                                         course.Number == num &&
                                         courseClass.SemesterSeason == season &&
                                         courseClass.SemesterYear == year
                                   select new
                                   {
                                       aname = assignment.AName,
                                       cname = category.AcName,
                                       due = assignment.DueDate,
                                       score = (from submission in db.Submissions
                                                where submission.StudentId == uid && submission.AssignmentId == assignment.AssignmentId
                                                select submission.Score).FirstOrDefault()
                                   };

            return Json(assignmentsQuery.ToArray());

        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            var classInfoQuery = from c in db.Classes
                                 join course in db.Courses on c.OfferingOf equals course.CourseId
                                 join d in db.Departments on course.Listing equals d.Subject
                                 where d.Subject == subject && course.Number == num
                                     && c.SemesterSeason == season && c.SemesterYear == year
                                 select c;

            var classInfo = classInfoQuery.FirstOrDefault();

            if (classInfo == null)
            {
                return Json(new { success = false });
            }

            var assignmentQuery = from a in db.Assignments
                                      //where a.AssignedIn == classInfo.ClassId
                                  join ac in db.AssignmentCategories on a.AssignedIn equals ac.AssignmentCategoryId
                                      where asgname == a.AName && ac.CategoryOf == classInfo.ClassId
                                  select a;

            var assignment = assignmentQuery.FirstOrDefault();

            if (assignment == null)
            {
                return Json(new { success = false });
            }

            var submissionQuery = from s in db.Submissions
                                  where s.AssignmentId == assignment.AssignmentId && s.StudentId == uid
                                  select s;

            var existingSubmission = submissionQuery.FirstOrDefault();

            if (existingSubmission != null)
            {
                existingSubmission.SContents = contents;
                existingSubmission.SubmissionTime = DateTime.Now;
                //existingSubmission.Score = 0;

                db.SaveChanges();

                return Json(new { success = true });
            }
            else
            {
                var newSubmission = new Submission
                {
                    AssignmentId = assignment.AssignmentId,
                    StudentId = uid,
                    SContents = contents,
                    SubmissionTime = DateTime.Now,
                    Score = 0 
                };

                db.Submissions.Add(newSubmission);
                db.SaveChanges();

                return Json(new { success = true });
            }
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {

            var classInfoQuery = from c in db.Classes
                                 join course in db.Courses on c.OfferingOf equals course.CourseId
                                 join d in db.Departments on course.Listing equals d.Subject
                                 where d.Subject == subject && course.Number == num
                                     && c.SemesterSeason == season && c.SemesterYear == year
                                 select c;

            var classInfo = classInfoQuery.FirstOrDefault();

            if (classInfo == null)
            {
                return Json(new { success = false });
            }

            var enrollmentQuery = from e in db.Enrollments
                                  where e.StudentId == uid && e.ClassId == classInfo.ClassId
                                  select e;

            var enrollment = enrollmentQuery.FirstOrDefault();

            if (enrollment != null) 
            {
                return Json(new { success = false });
            }
            else
            {
                var newEnrollment = new Enrollment()
                {
                    StudentId = uid,
                    ClassId = classInfo.ClassId,
                    Grade = "--"
                };

                db.Enrollments.Add(newEnrollment);
                db.SaveChanges();

                return Json(new { success = true });
            }

            //return Json(new { success = false});
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {            
            var enrollmentQuery = from e in db.Enrollments
                                  where e.StudentId == uid && e.Grade != "--"
                                  select e;

            int totalClasses = 0;
            double gradePoints = 0;

            foreach(var e in enrollmentQuery) 
            { 
                if (e.Grade == "A")
                {
                    gradePoints += 4.0;
                }
                else if (e.Grade == "A-")
                {
                    gradePoints += 3.7;
                }
                else if (e.Grade == "B+")
                {
                    gradePoints += 3.3;
                }
                else if (e.Grade == "B")
                {
                    gradePoints += 3.0;
                }
                else if (e.Grade == "B-")
                {
                    gradePoints += 2.7;
                }
                else if (e.Grade == "C+")
                {
                    gradePoints += 2.3;
                }
                else if (e.Grade == "C")
                {
                    gradePoints += 2.0;
                }
                else if (e.Grade == "C-")
                {
                    gradePoints += 1.7;
                }
                else if (e.Grade == "D+")
                {
                    gradePoints += 1.3;
                }
                else if (e.Grade == "D")
                {
                    gradePoints += 1.0;
                }
                else if (e.Grade == "D-")
                {
                    gradePoints += 0.7;
                }
                else if (e.Grade == "E")
                {
                    gradePoints += 0.0;
                }
                totalClasses++;
            }

            double GPA = 0;

            if (totalClasses != 0)
            {
                GPA = (double)(gradePoints * 4) / (double)(totalClasses * 4);
            }

            var json_GPA = new
            {
                gpa = GPA,
            };

            return Json(json_GPA);
        }
                
        /*******End code to modify********/

    }
}

