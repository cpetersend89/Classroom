using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Classroom.Models;
using Classroom.ViewModels;

namespace Classroom.Controllers
{
    public class DepartmentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Departments
        public ActionResult Index()
        {
            return View(db.Departments.ToList());
        }

        // GET: Departments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Department department = db.Departments.Find(id);
            if (department == null)
            {
                return HttpNotFound();
            }
            return View(department);
        }

        // GET: Departments/Create
        public ActionResult Create()
        {
            var department = new Department();
            department.Instructors = new List<Instructor>();
            PopulateAssignedInstructorData(department);
            return View();
        }

        // POST: Departments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "DepartmentId,DepartmentName")] Department department, string[] selectedInstructors)
        {
            if (selectedInstructors != null)
            {
                department.Instructors = new List<Instructor>();
                foreach (var instructor in selectedInstructors)
                {
                    var instructorToAdd = db.Instructors.Find(int.Parse(instructor));
                    department.Instructors.Add(instructorToAdd);
                }
            }
            if (ModelState.IsValid)
            {
                db.Departments.Add(department);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            PopulateAssignedInstructorData(department);
            return View(department);
        }

        // GET: Departments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Department departmentInstructor =
                db.Departments
                    .Include(s => s.Instructors)
                    .Single(v => v.DepartmentId == id);
            PopulateAssignedInstructorData(departmentInstructor);
            Department department = db.Departments.Find(id);
            if (department == null)
            {
                return HttpNotFound();
            }
            return View(department);
        }

        // POST: Departments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, string[] selectedInstructors)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Department departmentInstructor =
                db.Departments
                    .Include(s => s.Instructors)
                    .Single(v => v.DepartmentId == id);
            if (TryUpdateModel(departmentInstructor, "",
                new string[] { "DepartmentId", "DepartmentName" }))
            {
                try
                {
                    UpdateInstructors(selectedInstructors, departmentInstructor);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            PopulateAssignedInstructorData(departmentInstructor);
            Department department = db.Departments.Find(id);
            if (ModelState.IsValid)
            {
                db.Entry(department).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(department);
        }
        private void PopulateAssignedInstructorData(Department department)
        {
            var allInstructors = db.Instructors.OrderBy(i => i.LastName);
            var classroomInstructors = new HashSet<int>(department.Instructors.Select(c => c.InstructorId));
            var viewModel = new List<AssignedInstructorData>();
            foreach (var instructor in allInstructors)
            {
                viewModel.Add(new AssignedInstructorData
                {
                    InstructorId = instructor.InstructorId,
                    FullName = instructor.FullName,
                    Assigned = classroomInstructors.Contains(instructor.InstructorId)
                });
            }
            ViewBag.Instructors = viewModel;
        }
        private void UpdateInstructors(string[] selectedInstructors, Department departmentToUpdate)
        {
            if (selectedInstructors == null)
            {
                departmentToUpdate.Instructors = new List<Instructor>();
                return;
            }

            var selectedInstructorsHs = new HashSet<string>(selectedInstructors);
            var departmentInstructors = new HashSet<int>
                (departmentToUpdate.Instructors.Select(i => i.InstructorId));
            foreach (var instructor in db.Instructors)
            {
                if (selectedInstructorsHs.Contains(instructor.InstructorId.ToString()))
                {
                    if (!departmentInstructors.Contains(instructor.InstructorId))
                    {
                        departmentToUpdate.Instructors.Add(instructor);
                    }
                }
                else
                {
                    if (departmentInstructors.Contains(instructor.InstructorId))
                    {
                        departmentToUpdate.Instructors.Remove(instructor);
                    }
                }
            }
        }

        // GET: Departments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Department department = db.Departments.Find(id);
            if (department == null)
            {
                return HttpNotFound();
            }
            return View(department);
        }

        // POST: Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Department department = db.Departments.Find(id);
            db.Departments.Remove(department);
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
