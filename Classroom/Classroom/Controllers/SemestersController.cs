using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Classroom.Models;
using Classroom.ViewModels;

namespace Classroom.Controllers
{
    public class SemestersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Semesters
        public ActionResult Index(int? id, int? courseId)
        {
            var viewModel = new SemesterIndexData
            {
                Semesters =
                    db.Semesters.Include(s => s.Courses.Select(c => c.Department)).OrderBy(s => s.SemesterStartDate)
            };
            if (id != null)
            {
                ViewBag.SemesterId = id.Value;
                viewModel.Courses = viewModel.Semesters.Single(s => s.SemesterId == id.Value).Courses;
            }
            return View(db.Semesters.ToList());
        }

        // GET: Semesters/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Semester semester = db.Semesters.Include(s => s.Courses).Single(s => s.SemesterId == id);
            if (semester == null)
            {
                return HttpNotFound();
            }
            return View(semester);
        }
        private void PopulateAssignedCourseData(Semester semester)
        {
            var allCourses = db.Courses;
            var semesterCourses = new HashSet<int>(semester.Courses.Select(c => c.CourseId));
            var viewModel = new List<AssignedCourseData>();
            foreach (var course in allCourses)
            {
                viewModel.Add(new AssignedCourseData
                {
                    CourseId = course.CourseId,
                    Title = course.CourseTitle,
                    Assigned = semesterCourses.Contains(course.CourseId)
                });
            }
            ViewBag.Courses = viewModel;
        }
        // POST: Instructor/Edit/5 

        // GET: Semesters/Create
        public ActionResult Create()
        {
            var semester = new Semester();
            semester.Courses = new List<Course>();
            PopulateAssignedCourseData(semester);
            return View();
        }

        // POST: Semesters/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SemesterId,SemesterTitle,SemesterStartDate,SemesterEndDate")] Semester semester, string[] selectedCourses)
        {
            if (selectedCourses != null)
            {
                semester.Courses = new List<Course>();
                foreach (var course in selectedCourses)
                {
                    var courseToAdd = db.Courses.Find(int.Parse(course));
                    semester.Courses.Add(courseToAdd);
                }
            }
            if (ModelState.IsValid)
            {
                db.Semesters.Add(semester);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            PopulateAssignedCourseData(semester);
            return View(semester);
        }
        // GET: Semesters/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Semester semester = db.Semesters.Include(s => s.Courses).Single(s => s.SemesterId == id);
            PopulateAssignedCourseData(semester);
            if (semester == null)
            {
                return HttpNotFound();
            }
            return View(semester);
        }

        // POST: Semesters/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, string[] selectedCourses)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var semesterToUpdate = db.Semesters.Include(s => s.Courses).Single(s => s.SemesterId == id);
            if (TryUpdateModel(semesterToUpdate, "",
                new string[] {"SemesterId", "SemesterTitle", "SemesterStartDate", "SemesterEndDate"}))
            {
                try
                {
                    UpdateInstructorCourses(selectedCourses, semesterToUpdate);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                    throw;
                }
            }
            PopulateAssignedCourseData(semesterToUpdate);
            return View(semesterToUpdate);
        }

        private void UpdateInstructorCourses(string[] selectedCourses, Semester semesterToUpdate)
        {
            if (selectedCourses == null)
            {
                semesterToUpdate.Courses = new List<Course>();
                return;
            }

            var selectedCoursesHs = new HashSet<string>(selectedCourses);
            var semesterCourses = new HashSet<int>
                (semesterToUpdate.Courses.Select(c => c.CourseId));
            foreach (var course in db.Courses)
            {
                if (selectedCoursesHs.Contains(course.CourseId.ToString()))
                {
                    if (!semesterCourses.Contains(course.CourseId))
                    {
                        semesterToUpdate.Courses.Add(course);
                    }
                }
                else
                {
                    if (semesterCourses.Contains(course.CourseId))
                    {
                        semesterToUpdate.Courses.Remove(course);
                    }
                }
            }
        }

        // GET: Semesters/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Semester semester = db.Semesters.Find(id);
            if (semester == null)
            {
                return HttpNotFound();
            }
            return View(semester);
        }

        // POST: Semesters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Semester semester = db.Semesters.Find(id);
            db.Semesters.Remove(semester);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
