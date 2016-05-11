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
    public class TestsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public TestsController()
        {
        }

        public TestsController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        // GET: Tests
        public ActionResult Index()
        {
            return View(db.Tests.ToList());
        }

        // GET: Tests/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Test test = db.Tests.Find(id);
            if (test == null)
            {
                return HttpNotFound();
            }
            return View(test);
        }

        // GET: Tests/Create
        public ActionResult Create()
        {
            var test = new Test();
            test.VirtualClassrooms = new List<VirtualClassroom>();
            PopulateAssignedClassroomData(test);
            return View();
        }

        // POST: Tests/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TestId,TaskTitle,TaskDescription,TaskAvailable,AvailableDate,DueDate")] Test test, string[] selectedClassrooms)
        {
            if (selectedClassrooms != null)
            {
                test.VirtualClassrooms = new List<VirtualClassroom>();
                foreach (var classroom in selectedClassrooms)
                {
                    var classroomToAdd = db.VirtualClassrooms.Find(int.Parse(classroom));
                    test.VirtualClassrooms.Add(classroomToAdd);
                }
            }
            if (ModelState.IsValid)
            {
                List<TestFileDetail> fileDetails = new List<TestFileDetail>();
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];

                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        TestFileDetail fileDetail = new TestFileDetail()
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
                test.FileDetails = fileDetails;
                db.Tests.Add(test);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            PopulateAssignedClassroomData(test);
            return View(test);
        }

        // GET: Tests/Edit/5
        public ActionResult Edit(int? id)
        {
            var tests = new Test();
            tests.VirtualClassrooms = new List<VirtualClassroom>();
            PopulateAssignedClassroomData(tests);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Test test = db.Tests.Include(s => s.FileDetails).SingleOrDefault(x => x.TestId == id);
            if (test == null)
            {
                return HttpNotFound();
            }
            return View(test);
        }

        public FileResult Download(string p, string d)
        {
            return File(Path.Combine(Server.MapPath("~/App_Data/Upload/"), p), System.Net.Mime.MediaTypeNames.Application.Octet, d);
        }

        // POST: Tests/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TestId,TaskTitle,TaskDescription,TaskAvailable,AvailableDate,DueDate")] Test test, string[] selectedClassrooms)
        {
            if (selectedClassrooms != null)
            {
                test.VirtualClassrooms = new List<VirtualClassroom>();
                foreach (var classroom in selectedClassrooms)
                {
                    var classroomToAdd = db.VirtualClassrooms.Find(int.Parse(classroom));
                    test.VirtualClassrooms.Add(classroomToAdd);
                }
            }
            if (ModelState.IsValid)
            {
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];

                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        TestFileDetail fileDetail = new TestFileDetail()
                        {
                            FileName = fileName,
                            Extension = Path.GetExtension(fileName),
                            FileId = Guid.NewGuid(),
                            TestId = test.TestId
                        };
                        var path = Path.Combine(Server.MapPath("~/App_Data/Upload/"), fileDetail.FileId + fileDetail.Extension);
                        file.SaveAs(path);

                        db.Entry(fileDetail).State = EntityState.Added;
                    }
                }
                db.Entry(test).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(test);
        }

        private void PopulateAssignedClassroomData(Test test)
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
            var adminVirtualClassrooms = db.VirtualClassrooms;
            var instructorVirtualClassrooms =
                db.VirtualClassrooms
                    .Where(x => x.Instructors.All(i => i.InstructorId == user.Instructor.InstructorId));
            var testsClassroom = new HashSet<int>(test.VirtualClassrooms.Select(c => c.VirtualClassroomId));
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
                            Assigned = testsClassroom.Contains(classroom.VirtualClassroomId)
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
                            Assigned = testsClassroom.Contains(classroom.VirtualClassroomId)
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
                TestFileDetail fileDetail = db.TestFileDetails.Find(guid);
                if (fileDetail == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return Json(new { Result = "Error" });
                }

                //Remove from database
                db.TestFileDetails.Remove(fileDetail);
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

        // GET: Tests/Delete/5
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                Test test = db.Tests.Find(id);
                if (test == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return Json(new { Result = "Error" });
                }

                //delete files from the file system

                foreach (var item in test.FileDetails)
                {
                    String path = Path.Combine(Server.MapPath("~/App_Data/Upload/"), item.FileId + item.Extension);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }

                db.Tests.Remove(test);
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
