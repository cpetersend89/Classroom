using Classroom.Models;
using Classroom.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Classroom.Controllers
{
    [Authorize]
    public class VirtualClassroomsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public VirtualClassroomsController()
        {
            _context = new ApplicationDbContext();
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

        [Authorize(Roles = "Student")]
        public async Task<ActionResult> StudentClassroom(VirtualClassroomViewModel viewModel)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var classroom = new VirtualClassroomViewModel
            {
                VirtualClassrooms = user.Student.VirtualClassrooms,
                VirtualClassroom = _context.VirtualClassrooms.Find(viewModel.VirtualClassroom.Id)
            };
            return View(classroom);
        }

        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult> InstructorClassroom(VirtualClassroomViewModel viewModel)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var classroom = new VirtualClassroomViewModel
            {
                VirtualClassrooms = user.Instructor.VirtualClassrooms,
                VirtualClassroom = _context.VirtualClassrooms.Find(viewModel.VirtualClassroom.Id)
            };
            return View(classroom);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AdminIndex()
        {
            var classrooms = _context.VirtualClassrooms
                .Include(s => s.Semester)
                .Include(a => a.Administrator)
                .Include(c => c.Course)
                .Include(i => i.Instructors)
                .Include(s => s.Students);
            return View(classrooms);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            var viewModel = new VirtualClassroomFormViewModel
            {
                Courses = _context.Courses.ToList(),
                Semesters = _context.Semesters.ToList(),
                Instructors = _context.Instructors.ToList(),
                Students = _context.Students.ToList()
            };
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult _Create()
        {
            var viewModel = new VirtualClassroomFormViewModel
            {
                Courses = _context.Courses.ToList(),
                Semesters = _context.Semesters.ToList(),
                Instructors = _context.Instructors.ToList(),
                Students = _context.Students.ToList()
            };
            return PartialView(viewModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(VirtualClassroomFormViewModel viewModel, string[] selectedInstructors, string[] selectedStudents)
        {
            if (!ModelState.IsValid)
            {
                viewModel.Courses = _context.Courses.ToList();
                return View("_Create", viewModel);
            }
            var virtualClassroom = new VirtualClassroom
            {
                AdministratorId = User.Identity.GetUserId(),
                ClassroomName = viewModel.ClassroomName,
                SemesterId = viewModel.SemesterId,
                CourseId = viewModel.CourseId,
                Instructors = new List<Instructor>(),
                Students = new List<Student>()
            };
            foreach (var instructorId in selectedInstructors)
            {
                var instructors = _context.Instructors.Find(int.Parse(instructorId));
                virtualClassroom.Instructors.Add(instructors);
            }
            foreach (var studentId in selectedStudents)
            {
                var students = _context.Students.Find(int.Parse(studentId));
                virtualClassroom.Students.Add(students);
            }


            _context.VirtualClassrooms.Add(virtualClassroom);
            _context.SaveChanges();

            return RedirectToAction("AdminIndex");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var classroom = _context.VirtualClassrooms.Find(id);
            if (classroom == null)
            {
                return HttpNotFound();
            }
            return View(classroom);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VirtualClassroom virtualInstructor =
                _context.VirtualClassrooms
                    .Include(s => s.Instructors)
                    .Single(v => v.Id == id);
            VirtualClassroom virtualStudent =
                _context.VirtualClassrooms
                    .Include(s => s.Students)
                    .Single(v => v.Id == id);
            PopulateAssignedInstructorData(virtualInstructor);
            PopulateAssignedStudentData(virtualStudent);
            VirtualClassroom virtualClassroom = _context.VirtualClassrooms.Find(id);
            if (virtualClassroom == null)
            {
                return HttpNotFound();
            }
            ViewBag.CourseId = new SelectList(_context.Courses, "Id", "Name", virtualClassroom.CourseId);
            ViewBag.SemesterId = new SelectList(_context.Semesters, "Id", "Title", virtualClassroom.SemesterId);
            return View(virtualClassroom);
        }

        // POST: VirtualClassrooms/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, string[] selectedInstructors, string[] selectedStudents)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VirtualClassroom virtualInstructor =
                _context.VirtualClassrooms
                    .Include(s => s.Instructors)
                    .Single(v => v.Id == id);
            VirtualClassroom virtualStudent =
                _context.VirtualClassrooms
                    .Include(s => s.Students)
                    .Single(v => v.Id == id);
            if (TryUpdateModel(virtualInstructor, "",
                new string[] { "Id", "ClassroomName", "CourseId", "SemesterId" }))
            {
                try
                {
                    UpdateClassroomInstructors(selectedInstructors, virtualInstructor);
                    UpdateClassroomStudents(selectedStudents, virtualStudent);
                    _context.SaveChanges();
                    return RedirectToAction("AdminIndex");
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            else if (TryUpdateModel(virtualStudent, "",
                new string[] { "Id", "ClassroomName", "CourseId", "SemesterId" }))
            {
                try
                {
                    UpdateClassroomStudents(selectedStudents, virtualStudent);
                    UpdateClassroomInstructors(selectedInstructors, virtualInstructor);
                    _context.SaveChanges();
                    return RedirectToAction("AdminIndex");
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            PopulateAssignedInstructorData(virtualInstructor);
            PopulateAssignedStudentData(virtualStudent);
            VirtualClassroom virtualClassroom = _context.VirtualClassrooms.Find(id);
            if (ModelState.IsValid)
            {
                _context.Entry(virtualClassroom).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("AdminIndex");
            }
            ViewBag.CourseId = new SelectList(_context.Courses, "Id", "Name", virtualClassroom.CourseId);
            ViewBag.SemesterId = new SelectList(_context.Semesters, "Id", "Title", virtualClassroom.SemesterId);
            return View(virtualClassroom);
        }

        private void PopulateAssignedInstructorData(VirtualClassroom virtualClassroom)
        {
            var allInstructors = _context.Instructors.OrderBy(i => i.LastName);
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
            var allStudents = _context.Students.OrderBy(i => i.LastName);
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
            foreach (var instructor in _context.Instructors)
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
            foreach (var student in _context.Students)
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

        [Authorize(Roles = "Student")]
        public async Task<ActionResult> ClassroomTasks(int? id, VirtualClassroomViewModel viewModel)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var classroom = _context.VirtualClassrooms.Find(id);
            var classroomModel = new VirtualClassroomViewModel
            {
                StudentId = user.Student.StudentId,
                VirtualClassroom = classroom,
                Assignments = classroom.Assignments.OrderBy(d => d.DueDate).ToList(),
                CompletedAssignments = _context.CompletedAssignments
                .Where(s => s.StudentId == user.Student.StudentId 
                && s.VirtualClassroomId == classroom.Id).ToList(),
                Tests = classroom.Tests.OrderBy(d => d.DueDate).ToList(),
                CompletedTests = _context.CompletedTests
                .Where(s => s.StudentId == user.Student.StudentId
                && s.VirtualClassroomId == classroom.Id).ToList(),
                VirtualClassrooms = user.Student.VirtualClassrooms

            };
            return View(classroomModel);
        }

        [Authorize(Roles = "Student")]
        public async Task<ActionResult> _Assignments(int? id, VirtualClassroomViewModel viewModel)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var classroom = _context.VirtualClassrooms.Find(id);
            var classroomModel = new VirtualClassroomViewModel
            {
                StudentId = user.Student.StudentId,
                VirtualClassroom = classroom,
                Assignments = classroom.Assignments.OrderBy(d => d.DueDate).ToList(),
                CompletedAssignments = _context.CompletedAssignments
                .Where(s => s.StudentId == user.Student.StudentId
                && s.VirtualClassroomId == classroom.Id).ToList(),

            };

            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Student"))
                {
                    classroomModel.VirtualClassrooms = user.Student.VirtualClassrooms.ToList();
                }
                if (User.IsInRole("Instructor"))
                {
                    classroomModel.VirtualClassrooms = user.Instructor.VirtualClassrooms.ToList();
                }
                if (User.IsInRole("Admin"))
                {
                    classroomModel.VirtualClassrooms = _context.VirtualClassrooms.ToList();
                }
            }
            return PartialView(classroomModel);
        }

        [Authorize(Roles = "Student")]
        public async Task<ActionResult> _Tests(int? id, VirtualClassroomViewModel viewModel)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var classroom = _context.VirtualClassrooms.Find(id);
            var classroomModel = new VirtualClassroomViewModel
            {
                StudentId = user.Student.StudentId,
                VirtualClassroom = classroom,
                Tests = classroom.Tests.OrderBy(d => d.DueDate).ToList(),
                CompletedTests = _context.CompletedTests
                .Where(s => s.StudentId == user.Student.StudentId
                && s.VirtualClassroomId == classroom.Id).ToList()

            };

            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Student"))
                {
                    classroomModel.VirtualClassrooms = user.Student.VirtualClassrooms.ToList();
                }
                if (User.IsInRole("Instructor"))
                {
                    classroomModel.VirtualClassrooms = user.Instructor.VirtualClassrooms.ToList();
                }
                if (User.IsInRole("Admin"))
                {
                    classroomModel.VirtualClassrooms = _context.VirtualClassrooms.ToList();
                }
            }
            return PartialView(classroomModel);
        }


        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CompletedAssignment(VirtualClassroomViewModel classroom, int id)
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            CompletedAssignment completedAssignment = _context.CompletedAssignments.Where(a => a.AssignmentId == id).Single(s => s.StudentId == user.Student.StudentId && s.VirtualClassroomId == classroom.VirtualClassroom.Id);
            completedAssignment.Title = classroom.CompletedAssignment.Title;
            completedAssignment.Description = classroom.CompletedAssignment.Description;
            completedAssignment.Submitted = true;
            
                List<CompletedAssignmentFileDetails> fileDetails = new List<CompletedAssignmentFileDetails>();
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
                            CompletedAssignmentId = completedAssignment.Id
                            
                        };
                        fileDetails.Add(fileDetail);

                        var path = Path.Combine(Server.MapPath("~/App_Data/Upload/"),
                            fileDetail.FileId + fileDetail.Extension);
                        file.SaveAs(path);
                    }
                }
                completedAssignment.CompletedAssignmentFileDetails = fileDetails;
                _context.CompletedAssignments.AddOrUpdate(completedAssignment);
                _context.SaveChanges();
                new JsonResult { Data = "Successfully " };
                return RedirectToAction("ClassroomTasks", new { id = classroom.VirtualClassroom.Id });
            
        }


        [HttpPost]
        public JsonResult DeleteCompletedAssignment(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { Result = "Error" });
            }
            try
            {
                Guid guid = new Guid(id);
                CompletedAssignmentFileDetails fileDetail = _context.CompletedAssignmentFileDetails.Find(guid);
                if (fileDetail == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return Json(new { Result = "Error" });
                }

                //Remove from database
                _context.CompletedAssignmentFileDetails.Remove(fileDetail);
                _context.SaveChanges();

                //Delete file from the file system
                var path = Path.Combine(Server.MapPath("~/App_Data/Upload/"), fileDetail.FileId + fileDetail.Extension);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<ActionResult> _CompletedAssignment(int id, int classroomId)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var viewModel = new VirtualClassroomViewModel()
            {
                CompletedAssignment = _context.CompletedAssignments
                    .Where(a => a.AssignmentId == id)
                    .Single(s => s.StudentId == user.Student.StudentId
                    && s.VirtualClassroomId == classroomId)
            };

            return PartialView(viewModel);
        }

        [Authorize(Roles = "Student")]
        public ActionResult _SubmitAssignment(int id, int classroomId)
        {
            var viewModel = new VirtualClassroomViewModel()
            {
                Assignment = _context.Assignments.Find(id),
                VirtualClassroom = _context.VirtualClassrooms.Find(classroomId)
            };
            return PartialView("_SubmitAssignment", viewModel);

        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CompletedTest(VirtualClassroomViewModel classroom, int id)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            CompletedTest completedTest = _context.CompletedTests.Where(a => a.TestId == id).Single(s => s.StudentId == user.Student.StudentId && s.VirtualClassroomId == classroom.VirtualClassroom.Id);
            completedTest.Title = classroom.CompletedTest.Title;
            completedTest.Description = classroom.CompletedTest.Description;
            completedTest.Submitted = true;

            List<CompletedTestFileDetails> fileDetails = new List<CompletedTestFileDetails>();
            for (int i = 0; i < Request.Files.Count; i++)
            {
                var file = Request.Files[i];

                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    CompletedTestFileDetails fileDetail = new CompletedTestFileDetails()
                    {
                        FileName = fileName,
                        Extension = Path.GetExtension(fileName),
                        FileId = Guid.NewGuid(),
                        CompletedTestId = completedTest.Id

                    };
                    fileDetails.Add(fileDetail);

                    var path = Path.Combine(Server.MapPath("~/App_Data/Upload/"),
                        fileDetail.FileId + fileDetail.Extension);
                    file.SaveAs(path);
                }
            }
            completedTest.CompletedTestFileDetails = fileDetails;
            _context.CompletedTests.AddOrUpdate(completedTest);
            _context.SaveChanges();
            new JsonResult { Data = "Successfully " };
            return RedirectToAction("ClassroomTasks", new { id = classroom.VirtualClassroom.Id });

        }

        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<ActionResult> _CompletedTest(int id, int classroomId)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var viewModel = new VirtualClassroomViewModel()
            {
                CompletedTest = _context.CompletedTests
                    .Where(a => a.TestId == id)
                    .Single(s => s.StudentId == user.Student.StudentId
                    && s.VirtualClassroomId == classroomId)
            };

            return PartialView(viewModel);
        }

        public ActionResult _SubmitTest(int id, int classroomId)
        {
            var viewModel = new VirtualClassroomViewModel()
            {
                Test = _context.Tests.Find(id),
                VirtualClassroom = _context.VirtualClassrooms.Find(classroomId)
            };
            return PartialView("_SubmitTest", viewModel);

        }

        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<ActionResult> _EditAssignment(int id)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var viewModel = new VirtualClassroomViewModel()
            {
                CompletedAssignment = _context.CompletedAssignments
                    .Where(a => a.AssignmentId == id)
                    .Single(s => s.StudentId == user.Student.StudentId)
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditAssignment(int classroomId, VirtualClassroomViewModel model)
        {
            var completedAssignment = _context.CompletedAssignments.Find(model.CompletedAssignment.Id);
            List<CompletedAssignmentFileDetails> fileDetails = new List<CompletedAssignmentFileDetails>();
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
                        CompletedAssignmentId = model.CompletedAssignment.Id

                    };
                    fileDetails.Add(fileDetail);

                    var path = Path.Combine(Server.MapPath("~/App_Data/Upload/"),
                        fileDetail.FileId + fileDetail.Extension);
                    file.SaveAs(path);
                }
            }


            completedAssignment.Id = model.CompletedAssignment.Id;
            completedAssignment.AssignmentId = model.CompletedAssignment.AssignmentId;
            completedAssignment.Title = model.CompletedAssignment.Title;
            completedAssignment.Description = model.CompletedAssignment.Description;
            completedAssignment.CompletedDateTime = DateTime.Now;
            completedAssignment.CompletedAssignmentFileDetails = fileDetails;

            _context.CompletedAssignments.AddOrUpdate(completedAssignment);
            return View("ClassroomTasks", new VirtualClassroomViewModel() { VirtualClassroom = _context.VirtualClassrooms.Find(classroomId) });

        }

        [Authorize(Roles = "Student")]
        public async Task<ActionResult> GradeBook(int classroomId)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var classroom = _context.VirtualClassrooms.Find(classroomId);
            var viewModel = new VirtualClassroomViewModel()
            {
                VirtualClassroom = classroom,
                GradeBookViewModel = new GradeBookViewModel()
                {
                    VirtualClassroom = classroom,
                    Assignments = classroom.Assignments,
                    CompletedAssignments = _context.CompletedAssignments
                    .Include(g => g.Grade)
                    .Where(x => x.VirtualClassroomId == classroomId 
                    && x.StudentId == user.Student.StudentId).ToList(),
                    Tests = classroom.Tests,
                    CompletedTests = _context.CompletedTests
                    .Include(g => g.Grade)
                    .Where(x => x.VirtualClassroomId == classroomId
                    && x.StudentId == user.Student.StudentId).ToList()
                }
            };
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Student"))
                {
                    viewModel.VirtualClassrooms = user.Student.VirtualClassrooms.ToList();
                }
                if (User.IsInRole("Instructor"))
                {
                    viewModel.VirtualClassrooms = user.Instructor.VirtualClassrooms.ToList();
                }
                if (User.IsInRole("Admin"))
                {
                    viewModel.VirtualClassrooms = _context.VirtualClassrooms.ToList();
                }
            }
            return View(viewModel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}