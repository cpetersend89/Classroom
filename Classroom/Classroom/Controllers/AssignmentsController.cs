using Classroom.Models;
using Classroom.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Classroom.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class AssignmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AssignmentsController()
        {
            _context = new ApplicationDbContext();
        }

        public AssignmentsController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get { return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>(); }
            private set { _signInManager = value; }
        }

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        // GET: Assignments
        public async Task<ActionResult> Index()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var classrooms = user.Instructor.VirtualClassrooms;
            var viewModel = new VirtualClassroomViewModel()
            {
                Assignments = new List<Assignment>(),
                CompletedAssignments = new List<CompletedAssignment>(),
                VirtualClassrooms = classrooms
            };
            foreach (var classroom in classrooms)
            {
                foreach (var assignment in classroom.Assignments)
                {
                    viewModel.Assignments.Add(assignment);
                    foreach (var completedAssignment in assignment.CompletedAssignments.Where(s => s.Submitted))
                    {
                        viewModel.CompletedAssignments.Add(completedAssignment);
                    }
                }
            }
            return View(viewModel);
        }

        public async Task<ActionResult> ClassroomAssignments(int? id)
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var classrooms = user.Instructor.VirtualClassrooms;
            var classroom = _context.VirtualClassrooms.Find(id);
            var viewModel = new VirtualClassroomViewModel()
            {
                VirtualClassroom = classroom,
                VirtualClassrooms = classrooms,
                Assignments = classroom.Assignments.ToList(),
                CompletedAssignments = _context.CompletedAssignments
                .Where(x => x.VirtualClassroom.Id == id
                && x.Submitted
                && x.Graded == false).ToList(),
            };
            return View(viewModel);
        }

        // GET: Assignments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Assignment assignment = _context.Assignments.Find(id);
            if (assignment == null)
            {
                return HttpNotFound();
            }
            return View(assignment);
        }

        // GET: Assignments/Create
        public async Task<ActionResult> Create()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var viewModel = new AssignmentsFormViewModel
            {
                VirtualClassrooms = user.Instructor.VirtualClassrooms,
                AvailableDate = null,
                DueDate = null,
            };
            return View(viewModel);
        }

        public ActionResult _Create()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var viewModel = new VirtualClassroomViewModel()
            {
                VirtualClassrooms = user.Instructor.VirtualClassrooms
            };
            return PartialView(viewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(VirtualClassroomViewModel viewModel, string[] selectedClassrooms)
        {
            var assignment = new Assignment
            {
                TaskTitle = viewModel.Assignment.TaskTitle,
                TaskDescription = viewModel.Assignment.TaskDescription,
                TaskAvailable = viewModel.Assignment.TaskAvailable,
                AvailableDate = (DateTime) viewModel.Assignment.AvailableDate,
                DueDate = (DateTime) viewModel.Assignment.DueDate,
                PointsWorth = viewModel.Assignment.PointsWorth,
                Classrooms = new List<VirtualClassroom>()
            };
            foreach (var classroomId in selectedClassrooms)
            {
                var classroom = _context.VirtualClassrooms.Find(int.Parse(classroomId));
                assignment.Classrooms.Add(classroom);
            }
            foreach (var classroom in assignment.Classrooms)
            {
                foreach (var student in classroom.Students)
                {
                    Grade grade = new Grade()
                    {
                        Id = Guid.NewGuid(),
                        PointsReceived = 0
                    };
                    _context.Grades.Add(grade);

                    var completedAssignment = new CompletedAssignment
                    {
                        AssignmentId = assignment.Id,
                        StudentId = student.StudentId,
                        CompletedDateTime = null,
                        VirtualClassroomId = classroom.Id,
                        GradeId = grade.Id
                    };
                    _context.CompletedAssignments.Add(completedAssignment);
                }
            }
            assignment.FileDetails = AddFilesDetailsToAssignment();
            _context.Assignments.Add(assignment);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        private List<AssignmentFileDetail> AddFilesDetailsToAssignment()
        {
            List<AssignmentFileDetail> fileDetails = new List<AssignmentFileDetail>();
            for (int i = 0; i < Request.Files.Count; i++)
            {
                var file = Request.Files[i];

                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    AssignmentFileDetail fileDetail = new AssignmentFileDetail()
                    {
                        FileName = fileName,
                        Extension = Path.GetExtension(fileName),
                        FileId = Guid.NewGuid()
                    };
                    fileDetails.Add(fileDetail);

                    var path = Path.Combine(Server.MapPath("~/App_Data/Upload/"),
                        fileDetail.FileId + fileDetail.Extension);
                    file.SaveAs(path);
                }
            }
            return fileDetails;
        }

        public FileResult Download(string p, string d)
        {
            return File(Path.Combine(Server.MapPath("~/App_Data/Upload/"), p),
                System.Net.Mime.MediaTypeNames.Application.Octet, d);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Assignment assignment = _context.Assignments
                .Include(f => f.FileDetails)
                .SingleOrDefault(a => a.Id == id);
            if (assignment == null)
            {
                return HttpNotFound();
            }
            return View(assignment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "Id,TaskTitle,TaskDescription,TaskAvailable,AvailableDate,DueDate,PointsWorth")] Assignment
                assignment)
        {
            if (ModelState.IsValid)
            {
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];

                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        AssignmentFileDetail fileDetail = new AssignmentFileDetail()
                        {
                            FileName = fileName,
                            Extension = Path.GetExtension(fileName),
                            FileId = Guid.NewGuid(),
                            AssignmentId = assignment.Id
                        };
                        var path = Path.Combine(Server.MapPath("~/App_Data/Upload/"),
                            fileDetail.FileId + fileDetail.Extension);
                        file.SaveAs(path);

                        _context.Entry(fileDetail).State = EntityState.Added;
                    }
                }
                _context.Entry(assignment).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(assignment);
        }

        [HttpPost]
        public JsonResult DeleteFile(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return Json(new {Result = "Error"});
            }
            try
            {
                Guid guid = new Guid(id);
                AssignmentFileDetail fileDetail = _context.AssignmentFileDetails.Find(guid);
                if (fileDetail == null)
                {
                    Response.StatusCode = (int) HttpStatusCode.NotFound;
                    return Json(new {Result = "Error"});
                }

                //Remove from database
                _context.AssignmentFileDetails.Remove(fileDetail);
                _context.SaveChanges();

                //Delete file from the file system
                var path = Path.Combine(Server.MapPath("~/App_Data/Upload/"), fileDetail.FileId + fileDetail.Extension);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                return Json(new {Result = "OK"});
            }
            catch (Exception ex)
            {
                return Json(new {Result = "ERROR", Message = ex.Message});
            }
        }

        // GET: Assignments/Delete/5
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                Assignment assignment = _context.Assignments.Find(id);
                if (assignment == null)
                {
                    Response.StatusCode = (int) HttpStatusCode.NotFound;
                    return Json(new {Result = "Error"});
                }

                //delete files from the file system

                foreach (var item in assignment.FileDetails)
                {
                    String path = Path.Combine(Server.MapPath("~/App_Data/Upload/"), item.FileId + item.Extension);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }

                _context.Assignments.Remove(assignment);
                _context.SaveChanges();
                return Json(new {Result = "OK"});
            }
            catch (Exception ex)
            {
                return Json(new {Result = "ERROR", Message = ex.Message});
            }
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