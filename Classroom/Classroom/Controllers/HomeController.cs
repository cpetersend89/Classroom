using Classroom.Models;
using Classroom.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Classroom.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public HomeController()
        {
            _context = new ApplicationDbContext();
        }

        public HomeController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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


        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Student"))
                {
                    return RedirectToAction("StudentDashboard");
                }
                if (User.IsInRole("Instructor"))
                {
                    return RedirectToAction("InstructorDashboard");
                }
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("AdminDashboard");
                }
            }
            return View();
        }

        public async Task<ActionResult> StudentDashboard()
        {
            var today = DateTime.Now;
            var future = today.AddDays(7);
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var viewModel = new VirtualClassroomViewModel
            {
                VirtualClassrooms = user.Student.VirtualClassrooms,
                CompletedAssignments = _context.CompletedAssignments
                .Include(v => v.VirtualClassroom)
                .Include(a => a.Assignment)
                .Where(x => x.StudentId == user.Student.StudentId 
                && x.Submitted == false
                && x.Assignment.DueDate >= today 
                && x.Assignment.DueDate <= future)
                .OrderBy(a => a.Assignment.DueDate).ToList(),
                CompletedTests = _context.CompletedTests
                .Include(v => v.VirtualClassroom)
                .Include(t => t.Test)
                .Where(x => x.Student.StudentId == user.Student.StudentId
                && x.Submitted == false
                && x.Test.DueDate >= today
                && x.Test.DueDate <= future)
                .OrderBy(d => d.Test.DueDate).ToList()
            };
            return View(viewModel);
        }

        public async Task<ActionResult> InstructorDashboard()
        {
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var classrooms = user.Instructor.VirtualClassrooms;
            var completedAssignments =
                _context.CompletedAssignments
                .Include(s => s.Student)
                .Where(x => x.Submitted && x.Graded == false);
            var completedTests = _context.CompletedTests
                .Include(s => s.Student)
                .Where(x => x.Submitted && x.Graded == false);
            var viewModel = new VirtualClassroomViewModel
            {
                VirtualClassrooms = classrooms,
                CompletedAssignments = new List<CompletedAssignment>(),
                CompletedTests = new List<CompletedTest>(),
                Assignments = new List<Assignment>()
            };

            foreach (var classroom in classrooms)
            {
                foreach (var assignment in classroom.Assignments)
                {
                    viewModel.Assignments.Add(assignment);

                }
                foreach (var completedAssignment in completedAssignments.Where(x => x.VirtualClassroomId == classroom.Id))
                {
                    viewModel.CompletedAssignments.Add(completedAssignment);
                }
                foreach (var completedTest in completedTests.Where(x => x.VirtualClassroomId == classroom.Id))
                {
                    viewModel.CompletedTests.Add(completedTest);
                }

            }

            return View(viewModel);
        }

        public ActionResult AdminDashboard()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
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