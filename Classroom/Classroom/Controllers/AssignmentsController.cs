using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
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
    public class AssignmentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AssignmentsController()
        {
        }

        public AssignmentsController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        // GET: Assignments
        public ActionResult Index()
        {
            return View(db.Assignments.ToList());
        }

        // GET: Assignments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Assignment assignment = db.Assignments.Find(id);
            if (assignment == null)
            {
                return HttpNotFound();
            }
            return View(assignment);
        }

        // GET: Assignments/Create
        public ActionResult Create()
        {
            var assignment = new Assignment();
            assignment.VirtualClassrooms = new List<VirtualClassroom>();
            PopulateAssignedClassroomData(assignment);
            return View();
        }

        // POST: Assignments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AssignmentId,TaskTitle,TaskDescription,TaskAvailable,AvailableDate,DueDate")] Assignment assignment, string[] selectedClassrooms)
        {
            if (selectedClassrooms != null)
            {
                assignment.VirtualClassrooms = new List<VirtualClassroom>();
                foreach (var classroom in selectedClassrooms)
                {
                    var classroomToAdd = db.VirtualClassrooms.Find(int.Parse(classroom));
                    assignment.VirtualClassrooms.Add(classroomToAdd);
                }
            }
            if (ModelState.IsValid)
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
                assignment.FileDetails = fileDetails;
                db.Assignments.Add(assignment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            PopulateAssignedClassroomData(assignment);
            return View(assignment);
        }

        // GET: Assignments/Edit/5
        public ActionResult Edit(int? id)
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

        public FileResult Download(string p, string d)
        {
            return File(Path.Combine(Server.MapPath("~/App_Data/Upload/"), p), System.Net.Mime.MediaTypeNames.Application.Octet, d);
        }

        // POST: Assignments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AssignmentId,TaskTitle,TaskDescription,TaskAvailable,AvailableDate,DueDate")] Assignment assignment)
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
                            AssignmentId = assignment.AssignmentId
                        };
                        var path = Path.Combine(Server.MapPath("~/App_Data/Upload/"), fileDetail.FileId + fileDetail.Extension);
                        file.SaveAs(path);

                        db.Entry(fileDetail).State = EntityState.Added;
                    }
                }
                db.Entry(assignment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(assignment);
        }

        private void PopulateAssignedClassroomData(Assignment assignment)
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
            var adminVirtualClassrooms = db.VirtualClassrooms;
            var instructorVirtualClassrooms =
                db.VirtualClassrooms
                    .Where(x => x.Instructors.All(i => i.InstructorId == user.Instructor.InstructorId));
            var assignmentsClassroom = new HashSet<int>(assignment.VirtualClassrooms.Select(c => c.VirtualClassroomId));
            var viewModel = new List<AssignedClassroomData>();
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                {
                    foreach (var classroom in adminVirtualClassrooms)
                    {
                        viewModel.Add(new AssignedClassroomData
                        {
                            VirtualClassroomId = classroom.VirtualClassroomId,
                            ClassroomTitle = classroom.ClassroomTitle,
                            Assigned = assignmentsClassroom.Contains(classroom.VirtualClassroomId)
                        });
                    }
                }
                if (User.IsInRole("Instructor"))
                {
                    foreach (var classroom in instructorVirtualClassrooms)
                    {
                        viewModel.Add(new AssignedClassroomData
                        {
                            VirtualClassroomId = classroom.VirtualClassroomId,
                            ClassroomTitle = classroom.ClassroomTitle,
                            Assigned = assignmentsClassroom.Contains(classroom.VirtualClassroomId)
                        });
                    }
                }
            }

            ViewBag.VirtualClassrooms = viewModel;
        }

        [HttpPost]
        public JsonResult DeleteFile(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { Result = "Error" });
            }
            try
            {
                Guid guid = new Guid(id);
                AssignmentFileDetail fileDetail = db.AssignmentFileDetails.Find(guid);
                if (fileDetail == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return Json(new { Result = "Error" });
                }

                //Remove from database
                db.AssignmentFileDetails.Remove(fileDetail);
                db.SaveChanges();

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

        // GET: Assignments/Delete/5
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                Assignment assignment = db.Assignments.Find(id);
                if (assignment == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return Json(new { Result = "Error" });
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

                db.Assignments.Remove(assignment);
                db.SaveChanges();
                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
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
