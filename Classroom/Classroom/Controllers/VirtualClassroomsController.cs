using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Classroom.Models;
using Classroom.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace Classroom.Controllers
{
    public class VirtualClassroomsController : Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public VirtualClassroomsController()
        {
        }

        public VirtualClassroomsController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        // GET: VirtualClassrooms
        public async Task<ActionResult> Index()
        {
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var adminVirtualClassrooms = db.VirtualClassrooms.Include(v => v.Course).Include(v => v.Semester);
            var instructorVirtualClassrooms =
                db.VirtualClassrooms.Include(i => i.Course)
                    .Include(i => i.Semester)
                    .Where(x => x.Instructors.All(i => i.InstructorId == user.Instructor.InstructorId));
            var studentVirtualClassrooms =
                db.VirtualClassrooms.Include(s => s.Course)
                    .Include(s => s.Semester)
                    .Where(x => x.Students.All(i => i.StudentId == user.Student.StudentId));
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Instructor"))
                {
                    return View(instructorVirtualClassrooms.ToList());
                }
                if (User.IsInRole("Student"))
                {
                    return View(studentVirtualClassrooms.ToList());
                }
            }
            return View(adminVirtualClassrooms.ToList());
        }
        public ActionResult VirtualClassroomIndex()
        {
            VirtualClassroomViewModel classroom = new VirtualClassroomViewModel();
            classroom.Assignments = db.Assignments.ToList();
            classroom.CompletedAssignments = db.CompletedAssignments.ToList();
            return View(classroom);
        }

        // GET: VirtualClassrooms/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VirtualClassroom virtualClassroom = db.VirtualClassrooms.Find(id);
            if (virtualClassroom == null)
            {
                return HttpNotFound();
            }
            return View(virtualClassroom);
        }

        // GET: VirtualClassrooms/Create
        public ActionResult Create()
        {
            var virtualInstructor = new VirtualClassroom();
            virtualInstructor.Instructors = new List<Instructor>();
            PopulateAssignedInstructorData(virtualInstructor);
            var virtualStudent = new VirtualClassroom();
            virtualStudent.Students = new List<Student>();
            PopulateAssignedStudentData(virtualStudent);
            ViewBag.CourseId = new SelectList(db.Courses, "CourseId", "CourseTitle");
            ViewBag.SemesterId = new SelectList(db.Semesters, "SemesterId", "SemesterTitle");
            return View();
        }

        // POST: VirtualClassrooms/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "VirtualClassroomId,ClassroomTitle,CourseId,SemesterId")] VirtualClassroom virtualClassroom, string[] selectedInstructors, string[] selectedStudents)
        {
            if (selectedInstructors != null)
            {
                virtualClassroom.Instructors = new List<Instructor>();
                foreach (var instructor in selectedInstructors)
                {
                    var instructorToAdd = db.Instructors.Find(int.Parse(instructor));
                    virtualClassroom.Instructors.Add(instructorToAdd);
                }
            }
            if (selectedStudents != null)
            {
                virtualClassroom.Students = new List<Student>();
                foreach (var student in selectedStudents)
                {
                    var studentToAdd = db.Students.Find(int.Parse(student));
                    virtualClassroom.Students.Add(studentToAdd);
                }
            }
            if (ModelState.IsValid)
            {
                db.VirtualClassrooms.Add(virtualClassroom);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CourseId = new SelectList(db.Courses, "CourseId", "CourseTitle", virtualClassroom.CourseId);
            ViewBag.SemesterId = new SelectList(db.Semesters, "SemesterId", "SemesterTitle", virtualClassroom.SemesterId);
            PopulateAssignedInstructorData(virtualClassroom);
            return View(virtualClassroom);
        }

        // GET: VirtualClassrooms/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VirtualClassroom virtualInstructor =
                db.VirtualClassrooms
                    .Include(s => s.Instructors)
                    .Single(v => v.VirtualClassroomId == id);
            VirtualClassroom virtualStudent =
                db.VirtualClassrooms
                    .Include(s => s.Students)
                    .Single(v => v.VirtualClassroomId == id);
            PopulateAssignedInstructorData(virtualInstructor);
            PopulateAssignedStudentData(virtualStudent);
            VirtualClassroom virtualClassroom = db.VirtualClassrooms.Find(id);
            if (virtualClassroom == null)
            {
                return HttpNotFound();
            }
            ViewBag.CourseId = new SelectList(db.Courses, "CourseId", "CourseTitle", virtualClassroom.CourseId);
            ViewBag.SemesterId = new SelectList(db.Semesters, "SemesterId", "SemesterTitle", virtualClassroom.SemesterId);
            return View(virtualClassroom);
        }

        // POST: VirtualClassrooms/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, string[] selectedInstructors, string[] selectedStudents)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VirtualClassroom virtualInstructor =
                db.VirtualClassrooms
                    .Include(s => s.Instructors)
                    .Single(v => v.VirtualClassroomId == id);
            VirtualClassroom virtualStudent =
                db.VirtualClassrooms
                    .Include(s => s.Students)
                    .Single(v => v.VirtualClassroomId == id);
            if (TryUpdateModel(virtualInstructor, "",
                new string[] { "VirtualClassroomId", "ClassroomTitle", "CourseId", "SemesterId" }))
            {
                try
                {
                    UpdateClassroomInstructors(selectedInstructors, virtualInstructor);
                    UpdateClassroomStudents(selectedStudents, virtualStudent);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            else if (TryUpdateModel(virtualStudent, "",
                new string[] { "VirtualClassroomId", "ClassroomTitle", "CourseId", "SemesterId" }))
            {
                try
                {
                    UpdateClassroomStudents(selectedStudents, virtualStudent);
                    UpdateClassroomInstructors(selectedInstructors, virtualInstructor);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            PopulateAssignedInstructorData(virtualInstructor);
            PopulateAssignedStudentData(virtualStudent);
            VirtualClassroom virtualClassroom = db.VirtualClassrooms.Find(id);
            if (ModelState.IsValid)
            {
                db.Entry(virtualClassroom).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CourseId = new SelectList(db.Courses, "CourseId", "CourseTitle", virtualClassroom.CourseId);
            ViewBag.SemesterId = new SelectList(db.Semesters, "SemesterId", "SemesterTitle", virtualClassroom.SemesterId);
            return View(virtualClassroom);
        }

        private void PopulateAssignedInstructorData(VirtualClassroom virtualClassroom)
        {
            var allInstructors = db.Instructors.OrderBy(i => i.LastName);
            var classroomInstructors = new HashSet<int>(virtualClassroom.Instructors.Select(c => c.InstructorId));
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
        private void PopulateAssignedStudentData(VirtualClassroom virtualClassroom)
        {
            var allStudents = db.Students.OrderBy(i => i.LastName);
            var classroomStudents = new HashSet<int>(virtualClassroom.Students.Select(c => c.StudentId));
            var viewModel = new List<AssignedStudentData>();
            foreach (var student in allStudents)
            {
                viewModel.Add(new AssignedStudentData
                {
                    StudentId = student.StudentId,
                    FullName = student.FullName,
                    Assigned = classroomStudents.Contains(student.StudentId)
                });
            }
            ViewBag.Students = viewModel;
        }

        private void UpdateClassroomInstructors(string[] selectedInstructors, VirtualClassroom classroomToUpdate)
        {
            if (selectedInstructors == null)
            {
                classroomToUpdate.Instructors = new List<Instructor>();
                return;
            }

            var selectedInstructorsHs = new HashSet<string>(selectedInstructors);
            var classroomInstructors = new HashSet<int>
                (classroomToUpdate.Instructors.Select(i => i.InstructorId));
            foreach (var instructor in db.Instructors)
            {
                if (selectedInstructorsHs.Contains(instructor.InstructorId.ToString()))
                {
                    if (!classroomInstructors.Contains(instructor.InstructorId))
                    {
                        classroomToUpdate.Instructors.Add(instructor);
                    }
                }
                else
                {
                    if (classroomInstructors.Contains(instructor.InstructorId))
                    {
                        classroomToUpdate.Instructors.Remove(instructor);
                    }
                }
            }
        }

        private void UpdateClassroomStudents(string[] selectedStudents, VirtualClassroom classroomToUpdate)
        {
            if (selectedStudents == null)
            {
                classroomToUpdate.Students = new List<Student>();
                return;
            }

            var selectedStudentsHs = new HashSet<string>(selectedStudents);
            var classroomStudents = new HashSet<int>
                (classroomToUpdate.Students.Select(i => i.StudentId));
            foreach (var student in db.Students)
            {
                if (selectedStudentsHs.Contains(student.StudentId.ToString()))
                {
                    if (!classroomStudents.Contains(student.StudentId))
                    {
                        classroomToUpdate.Students.Add(student);
                    }
                }
                else
                {
                    if (classroomStudents.Contains(student.StudentId))
                    {
                        classroomToUpdate.Students.Remove(student);
                    }
                }
            }
        }

        public ActionResult CompletedAssignment(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Assignment assignment = db.Assignments.Include(s => s.FileDetails).SingleOrDefault(x => x.AssignmentId == id);
            if (assignment == null)
            {
                return HttpNotFound();
            }
            return View(assignment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CompletedAssignment(CompletedAssignment assignment, Assignment assignments)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (ModelState.IsValid)
            {
                List<CompletedAssignmentFileDetails> fileDetails = new List<CompletedAssignmentFileDetails>();
                var studentAssignment = new CompletedAssignment()
                {
                    CompletedAssignmentId = assignment.CompletedAssignmentId,
                    CompletedDateTime = assignment.CompletedDateTime,
                    StudentId = user.Student.StudentId,
                    AssignmentId = assignments.AssignmentId
                };
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];

                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        CompletedAssignmentFileDetails fileDetail = new CompletedAssignmentFileDetails()
                        {
                            FileName = fileName,
                            Extension = Path.GetExtension(fileName),
                            FileId = Guid.NewGuid(),
                            StudentId = user.Student.StudentId,
                            AssignmentId = assignments.AssignmentId
                        };
                        fileDetails.Add(fileDetail);

                        var path = Path.Combine(Server.MapPath("~/App_Data/Upload/"),
                            fileDetail.FileId + fileDetail.Extension);
                        file.SaveAs(path);
                    }
                }
                studentAssignment.CompletedAssignments = fileDetails;
                db.CompletedAssignments.Add(studentAssignment);
                db.SaveChanges();
                return RedirectToAction("VirtualClassroomIndex");
            }

            return View(assignments);
        }

        // GET: VirtualClassrooms/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VirtualClassroom virtualClassroom = db.VirtualClassrooms.Find(id);
            if (virtualClassroom == null)
            {
                return HttpNotFound();
            }
            return View(virtualClassroom);
        }

        // POST: VirtualClassrooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            VirtualClassroom virtualClassroom = db.VirtualClassrooms.Find(id);
            db.VirtualClassrooms.Remove(virtualClassroom);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> _assignments([Bind(Include = "AssignmentId,TaskTitle,TaskDescription,TaskAvailable,AvailableDate,DueDate")] Assignment assignment)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (ModelState.IsValid)
            {
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];

                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        CompletedAssignmentFileDetails fileDetail = new CompletedAssignmentFileDetails()
                        {
                            FileName = fileName,
                            Extension = Path.GetExtension(fileName),
                            FileId = Guid.NewGuid(),
                            AssignmentId = assignment.AssignmentId,
                            StudentId = user.Student.StudentId
                        };
                        var path = Path.Combine(Server.MapPath("~/App_Data/Upload/"), fileDetail.FileId + fileDetail.Extension);
                        file.SaveAs(path);

                        db.Entry(fileDetail).State = EntityState.Added;
                    }
                }
                db.Entry(assignment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("VirtualClassroomIndex");
            }
            return PartialView(db.Assignments.ToList());
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
