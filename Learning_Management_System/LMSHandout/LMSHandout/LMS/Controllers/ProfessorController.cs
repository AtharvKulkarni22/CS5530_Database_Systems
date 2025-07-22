using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
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

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
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

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var query = from c in db.Courses
                        join cl in db.Classes on c.CourseId equals cl.OfferingOf
                        join e in db.Enrollments on cl.ClassId equals e.ClassId
                        join s in db.Students on e.StudentId equals s.UId
                        where c.Listing == subject && c.Number == num
                             && cl.SemesterSeason == season && cl.SemesterYear == year
                        select new
                        {
                            fname = s.FName,
                            lname = s.LName,
                            uid = s.UId,
                            dob = s.Dob,
                            grade = e.Grade,
                        };

            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            var classQuery = from cl in db.Classes
                             join c in db.Courses on cl.OfferingOf equals c.CourseId
                             join asc in db.AssignmentCategories on cl.ClassId equals asc.CategoryOf
                             join a in db.Assignments on asc.AssignmentCategoryId equals a.AssignedIn
                             where c.Listing == subject && c.Number == num
                                && cl.SemesterSeason == season && cl.SemesterYear == year 
                                && (asc.AcName == category || category == null)
                             select new
                             {
                                 aname = a.AName,
                                 cname = asc.AcName,
                                 due = a.DueDate,
                                 submissions = a.Submissions.Count(),
                             };


            return Json(classQuery.ToArray());
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var query = from cl in db.Classes
                             join c in db.Courses on cl.OfferingOf equals c.CourseId
                             join asc in db.AssignmentCategories on cl.ClassId equals asc.CategoryOf
                             where c.Listing == subject && c.Number == num
                                && cl.SemesterSeason == season && cl.SemesterYear == year
                             select new
                             {
                                 name = asc.AcName,
                                 weight = asc.Weight,
                             };


            return Json(query.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            var classQuery = from cl in db.Classes
                             join c in db.Courses on cl.OfferingOf equals c.CourseId
                             where c.Listing == subject && c.Number == num
                             && cl.SemesterSeason == season && cl.SemesterYear == year
                             select cl;

            var classInfo = classQuery.FirstOrDefault();

            if (classInfo == null) { return Json(new { success = false }); }

            var Categories = from ac in db.AssignmentCategories
                             where ac.CategoryOf == classInfo.ClassId && ac.AcName == category
                             select ac;

            if (Categories.FirstOrDefault() != null) { return Json(new { success = false }); }

            var newAssignmentCategory = new AssignmentCategory
            {
                AcName = category,
                CategoryOf = classInfo.ClassId,
                Weight = (uint)catweight,
            };

            db.AssignmentCategories.Add(newAssignmentCategory);
            db.SaveChanges();

            return Json(new { success = true });

        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            var assignmentCategoryQuery = (from cl in db.Classes
                             join c in db.Courses on cl.OfferingOf equals c.CourseId
                             join asc in db.AssignmentCategories on cl.ClassId equals asc.CategoryOf
                             where c.Listing == subject && c.Number == num
                             && cl.SemesterSeason == season && cl.SemesterYear == year && asc.AcName == category
                             select asc).ToList();

            var categoryInfo = assignmentCategoryQuery.FirstOrDefault();

            if (categoryInfo == null) { return Json(new { success = false }); }

            var Assignments = (from a in db.Assignments
                             where a.AssignedIn == categoryInfo.AssignmentCategoryId && a.AName == asgname
                             select a).ToList();

            if (Assignments.FirstOrDefault() != null) { return Json(new { success = false }); }

            var newAssignment = new Assignment
            {
                AName = asgname,
                AssignedIn = categoryInfo.AssignmentCategoryId,
                AContents = asgcontents,
                DueDate = asgdue,
                MaxPoints = (uint)asgpoints,
            };

            db.Assignments.Add(newAssignment);
            db.SaveChanges();

            var enrollmentQuery = (from e in db.Enrollments
                                  join cl in db.Classes on e.ClassId equals cl.ClassId
                                  join c in db.Courses on cl.OfferingOf equals c.CourseId
                                  where c.Listing == subject && c.Number == num
                                    && cl.SemesterSeason == season && cl.SemesterYear == year
                                  select e).ToList();

            foreach(var e in enrollmentQuery)
            {
                updateGrade(e.StudentId, (int)e.ClassId);
            }
            return Json(new { success = true });
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            var query = from st in db.Students
                        join sb in db.Submissions on st.UId equals sb.StudentId
                        join a in db.Assignments on sb.AssignmentId equals a.AssignmentId
                        join asc in db.AssignmentCategories on a.AssignedIn equals asc.AssignmentCategoryId
                        join cl in db.Classes on asc.CategoryOf equals cl.ClassId
                        join c in db.Courses on cl.OfferingOf equals c.CourseId
                        where c.Listing == subject && c.Number == num && cl.SemesterSeason == season && cl.SemesterYear == year
                        && asc.AcName == category && a.AName == asgname
                        select new
                        {
                            fname = st.FName,
                            lname = st.LName,
                            uid = st.UId,
                            time = sb.SubmissionTime,
                            score = sb.Score,
                        };

            return Json(query.ToArray());
           
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            var query = (from sb in db.Submissions
                        join a in db.Assignments on sb.AssignmentId equals a.AssignmentId
                        join asc in db.AssignmentCategories on a.AssignedIn equals asc.AssignmentCategoryId
                        join cl in db.Classes on asc.CategoryOf equals cl.ClassId
                        join c in db.Courses on cl.OfferingOf equals c.CourseId
                        where c.Listing == subject && c.Number == num && cl.SemesterSeason == season && cl.SemesterYear == year
                        && asc.AcName == category && a.AName == asgname && sb.StudentId == uid
                        select sb).ToList();

            Submission? submission = query.FirstOrDefault();

            if (submission == null) { return Json(new { success = false }); }

            submission.Score = (uint)score;
            db.SaveChanges();

            var classQuery = (from cl in db.Classes
                             join c in db.Courses on cl.OfferingOf equals c.CourseId
                             where c.Listing == subject && c.Number == num && cl.SemesterSeason == season && cl.SemesterYear == year
                             select cl).ToList();

            Class? classinfo = classQuery.FirstOrDefault();

            updateGrade(submission.StudentId, (int)classinfo!.ClassId);

            return Json(new { success = true });
        }

        public void updateGrade(string studentID, int classID)
        {
            var query = (from asc in db.AssignmentCategories
                        where asc.CategoryOf == classID
                        select asc).ToList();

            AssignmentCategory? catinfo = query.FirstOrDefault();

            double scaledTotal = 0;
            int totalWeights = 0;

            var queryList = query.ToList();
            foreach (var c in queryList)
            {
                int earnedPoints = 0;
                int maxPoints = 0;

                var Assignmentquery = from a in db.Assignments
                                      where a.AssignedIn == c.AssignmentCategoryId
                                      select a;

                //Assignment? assignmentinfo = Assignmentquery.FirstOrDefault();
               

                var AssignmentqueryList = Assignmentquery.ToList();
                if (AssignmentqueryList.Count == 0)
                {
                    continue;
                }

                foreach (var a in AssignmentqueryList)
                {
                    maxPoints += (int)a.MaxPoints;

                    var SubmissionQuery = from s in db.Submissions
                                          where s.StudentId == studentID && s.AssignmentId == a.AssignmentId
                                          select s;

                    Submission? submissionInfo = SubmissionQuery.FirstOrDefault();
                    if (submissionInfo != null) 
                    {
                        earnedPoints += (int)submissionInfo.Score;
                    }

                    scaledTotal += ((double)earnedPoints / (double)maxPoints) * (double)c.Weight;
                    totalWeights += (int)c.Weight;
                } 
            }   
            double percentage = scaledTotal * ((double)100 / (double)totalWeights);

            string letterGrade = "--";
            if (percentage < 60)
            {
                letterGrade = "E";
            }
            else if (percentage < 63)
            {
                letterGrade= "D-";
            }
            else if (percentage < 67)
            {
                letterGrade = "D";
            }
            else if (percentage < 70)
            {
                letterGrade = "D+";
            }
            else if (percentage < 73)
            {
                letterGrade = "C-";
            }
            else if (percentage < 77)
            {
                letterGrade = "C";
            }
            else if (percentage < 80)
            {
                letterGrade = "C+";
            }
            else if (percentage < 83)
            {
                letterGrade = "B-";
            }
            else if (percentage < 87)
            {
                letterGrade = "B";
            }
            else if (percentage < 90)
            {
                letterGrade = "B+";
            }
            else if (percentage < 93)
            {
                letterGrade = "A-";
            }
            else
            {
                letterGrade = "A";
            }

            var enrollmentQuery = from e in db.Enrollments
                                  where e.StudentId == studentID && e.ClassId == classID
                                  select e;

            var enrollment = enrollmentQuery.FirstOrDefault();
            enrollment!.Grade = letterGrade;
            db.SaveChanges();
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var classQuery = from cl in db.Classes
                             join c in db.Courses on cl.OfferingOf equals c.CourseId
                             where cl.ProfessorId == uid
                             select new
                             {
                                 subject = c.Listing,
                                 number = c.Number,
                                 name = c.CName,
                                 season = cl.SemesterSeason,
                                 year = cl.SemesterYear,
                             };


            return Json(classQuery.ToArray());
        }


        
        /*******End code to modify********/
    }
}

