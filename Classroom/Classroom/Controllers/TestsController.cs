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
    public class TestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public TestsController()
        {
            _context = new ApplicationDbContext();
        }

        public TestsController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        // GET: Tests
        public async Task<ActionResult> Index()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var classrooms = user.Instructor.VirtualClassrooms;
            var viewModel = new VirtualClassroomViewModel()
            {
                Tests = new List<Test>(),
                CompletedTests = new List<CompletedTest>(),
                VirtualClassrooms = classrooms
            };
            foreach (var classroom in classrooms)
            {
                foreach (var test in classroom.Tests)
                {
                    viewModel.Tests.Add(test);
                    foreach (var completedTest in test.CompletedTests.Where(s => s.Submitted))
                    {
                        viewModel.CompletedTests.Add(completedTest);
                    }
                }
            }
            return View(viewModel);
        }

        public async Task<ActionResult> ClassroomTests(int? id)
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var classrooms = user.Instructor.VirtualClassrooms;
            var classroom = _context.VirtualClassrooms.Find(id);
            var viewModel = new VirtualClassroomViewModel()
            {
                VirtualClassroom = classroom,
                VirtualClassrooms = classrooms,
                Tests = classroom.Tests.ToList(),
                CompletedTests = _context.CompletedTests
                .Where(x => x.VirtualClassroom.Id == id
                && x.Submitted
                && x.Graded == false).ToList(),
            };
            return View(viewModel);
        }

        // GET: Tests/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Test test = _context.Tests.Find(id);
            if (test == null)
            {
                return HttpNotFound();
            }
            return View(test);
        }

        //// GET: Tests/Create
        //public async Task<ActionResult> Create()
        //{
        //    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
        //    var viewModel = new TestsFormViewModel
        //    {
        //        VirtualClassrooms = user.Instructor.VirtualClassrooms,
        //        AvailableDate = null,
        //        DueDate = null,
        //    };
        //    return View(viewModel);
        //}

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
            var test = new Test
            {
                TaskTitle = viewModel.Test.TaskTitle,
                TaskDescription = viewModel.Test.TaskDescription,
                TaskAvailable = viewModel.Test.TaskAvailable,
                AvailableDate = (DateTime)viewModel.Test.AvailableDate,
                DueDate = (DateTime)viewModel.Test.DueDate,
                PointsWorth = viewModel.Test.PointsWorth,
                Classrooms = new List<VirtualClassroom>()
            };
            foreach (var classroomId in selectedClassrooms)
            {
                var classroom = _context.VirtualClassrooms.Find(int.Parse(classroomId));
                test.Classrooms.Add(classroom);
            }
            foreach (var classroom in test.Classrooms)
            {
                foreach (var student in classroom.Students)
                {
                    Grade grade = new Grade()
                    {
                        Id = Guid.NewGuid(),
                        PointsReceived = 0
                    };
                    _context.Grades.Add(grade);

                    var completedTest = new CompletedTest
                    {
                        TestId = test.Id,
                        StudentId = student.StudentId,
                        CompletedDateTime = null,
                        VirtualClassroomId = classroom.Id,
                        GradeId = grade.Id
                    };
                    _context.CompletedTests.Add(completedTest);
                }
            }
            test.FileDetails = AddFilesDetailsToTest();
            _context.Tests.Add(test);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        private List<TestFileDetail> AddFilesDetailsToTest()
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
            Test test = _context.Tests
                .Include(f => f.FileDetails)
                .SingleOrDefault(a => a.Id == id);
            if (test == null)
            {
                return HttpNotFound();
            }
            return View(test);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "Id,TaskTitle,TaskDescription,TaskAvailable,AvailableDate,DueDate,PointsWorth")] Test
                test)
        {
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
                            TestId = test.Id
                        };
                        var path = Path.Combine(Server.MapPath("~/App_Data/Upload/"),
                            fileDetail.FileId + fileDetail.Extension);
                        file.SaveAs(path);

                        _context.Entry(fileDetail).State = EntityState.Added;
                    }
                }
                _context.Entry(test).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(test);
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
                TestFileDetail fileDetail = _context.TestFileDetails.Find(guid);
                if (fileDetail == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return Json(new { Result = "Error" });
                }

                //Remove from database
                _context.TestFileDetails.Remove(fileDetail);
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

        // GET: Tests/Delete/5
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                Test test = _context.Tests.Find(id);
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

                _context.Tests.Remove(test);
                _context.SaveChanges();
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