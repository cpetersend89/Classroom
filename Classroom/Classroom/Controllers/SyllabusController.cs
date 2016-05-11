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
    public class SyllabusController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public SyllabusController()
        {
        }

        public SyllabusController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
            return View(db.Syllabus.ToList());
        }

        // GET: Assignments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Syllabus syllabus = db.Syllabus.Find(id);
            if (syllabus == null)
            {
                return HttpNotFound();
            }
            return View(syllabus);
        }

        // GET: Assignments/Create
        public ActionResult Create()
        {
            var syllabus = new Syllabus();
            syllabus.VirtualClassrooms = new List<VirtualClassroom>();
            PopulateAssignedClassroomData(syllabus);
            return View();
        }

        // POST: Assignments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SyllabusId,SyllabusTitle,SyllabusDescription")] Syllabus syllabus, string[] selectedClassrooms)
        {
            if (selectedClassrooms != null)
            {
                syllabus.VirtualClassrooms = new List<VirtualClassroom>();
                foreach (var classroom in selectedClassrooms)
                {
                    var classroomToAdd = db.VirtualClassrooms.Find(int.Parse(classroom));
                    syllabus.VirtualClassrooms.Add(classroomToAdd);
                }
            }
            if (ModelState.IsValid)
            {
                List<SyllabusFileDetails> fileDetails = new List<SyllabusFileDetails>();
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];

                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        SyllabusFileDetails fileDetail = new SyllabusFileDetails()
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
                syllabus.FileDetails = fileDetails;
                db.Syllabus.Add(syllabus);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            PopulateAssignedClassroomData(syllabus);
            return View(syllabus);
        }

        // GET: Assignments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Syllabus syllabus = db.Syllabus.Include(s => s.FileDetails).SingleOrDefault(x => x.SyllabusId == id);
            if (syllabus == null)
            {
                return HttpNotFound();
            }
            return View(syllabus);
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
        public ActionResult Edit([Bind(Include = "SyllabusId,SyllabusTitle,SyllabusDescription")] Syllabus syllabus)
        {
            if (ModelState.IsValid)
            {
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];

                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        SyllabusFileDetails fileDetail = new SyllabusFileDetails()
                        {
                            FileName = fileName,
                            Extension = Path.GetExtension(fileName),
                            FileId = Guid.NewGuid(),
                            SyllabusId = syllabus.SyllabusId
                        };
                        var path = Path.Combine(Server.MapPath("~/App_Data/Upload/"), fileDetail.FileId + fileDetail.Extension);
                        file.SaveAs(path);

                        db.Entry(fileDetail).State = EntityState.Added;
                    }
                }
                db.Entry(syllabus).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(syllabus);
        }

        private void PopulateAssignedClassroomData(Syllabus syllabus)
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
            var adminVirtualClassrooms = db.VirtualClassrooms;
            var instructorVirtualClassrooms =
                db.VirtualClassrooms
                    .Where(x => x.Instructors.All(i => i.InstructorId == user.Instructor.InstructorId));
            var syllabusClassroom = new HashSet<int>(syllabus.VirtualClassrooms.Select(c => c.VirtualClassroomId));
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
                            Assigned = syllabusClassroom.Contains(classroom.VirtualClassroomId)
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
                            Assigned = syllabusClassroom.Contains(classroom.VirtualClassroomId)
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
                SyllabusFileDetails fileDetail = db.SyllabusFileDetails.Find(guid);
                if (fileDetail == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return Json(new { Result = "Error" });
                }

                //Remove from database
                db.SyllabusFileDetails.Remove(fileDetail);
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
                Syllabus syllabus = db.Syllabus.Find(id);
                if (syllabus == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return Json(new { Result = "Error" });
                }

                //delete files from the file system

                foreach (var item in syllabus.FileDetails)
                {
                    String path = Path.Combine(Server.MapPath("~/App_Data/Upload/"), item.FileId + item.Extension);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }

                db.Syllabus.Remove(syllabus);
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
