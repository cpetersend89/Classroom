using Classroom.Models;
using Classroom.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Classroom.Controllers
{
    public class CompletedTestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public CompletedTestsController()
        {
            _context = new ApplicationDbContext();
        }

        public CompletedTestsController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        // GET: CompletedTests
        public async Task<ActionResult> Index(int? id, int? classroomId)
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var viewModel = new VirtualClassroomViewModel()
            {
                Test = _context.Tests.Find(id),
                VirtualClassroom = _context.VirtualClassrooms.Find(classroomId),
                VirtualClassrooms = user.Instructor.VirtualClassrooms,
                CompletedTests = _context.CompletedTests
                .Include(s => s.Student)
                .Where(s => s.Submitted
                && s.TestId == id
                && s.Graded == false).ToList()
            };
            return View(viewModel);
        }

        public async Task<ActionResult> Edit(int? id, int? classroomId)
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var viewModel = new VirtualClassroomViewModel()
            {
                VirtualClassroom = _context.VirtualClassrooms.Find(classroomId),
                VirtualClassrooms = user.Instructor.VirtualClassrooms,
                CompletedTest = _context.CompletedTests.Find(id)
            };
            if (viewModel == null)
            {
                return HttpNotFound();
            }
            //ViewBag.TestId = new SelectList(db.Tests, "TestId", "TaskTitle", completedTest.CompletedTest.TestId);
            //ViewBag.StudentId = new SelectList(db.Students, "StudentId", "FirstName", completedTest.CompletedTest.StudentId);
            return View(viewModel);
        }

        // POST: CompletedTests/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(VirtualClassroomViewModel viewModel)
        {
            var test = _context.Tests.Find(viewModel.CompletedTest.TestId);
            var editedTest = _context.CompletedTests.Find(viewModel.CompletedTest.Id);
            editedTest.Grade = _context.Grades.Find(viewModel.CompletedTest.GradeId);
            editedTest.Grade.PointsReceived = viewModel.CompletedTest.Grade.PointsReceived;
            editedTest.Grade.GradePercentage = Grade.GetGradePercentage(test.PointsWorth, viewModel.CompletedTest.Grade.PointsReceived);
            editedTest.Graded = true;
            if (ModelState.IsValid)
            {
                _context.CompletedTests.AddOrUpdate(editedTest);
                //db.Entry(editedTest).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Index", new { id = viewModel.CompletedTest.TestId, classroomId = viewModel.CompletedTest.VirtualClassroomId });
            }
            ViewBag.TestId = new SelectList(_context.Tests, "Id", "TaskTitle", viewModel.CompletedTest.TestId);
            ViewBag.StudentId = new SelectList(_context.Students, "StudentId", "FirstName", viewModel.StudentId);
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
