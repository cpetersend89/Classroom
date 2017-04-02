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
    public class CompletedAssignmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public CompletedAssignmentsController()
        {
            _context = new ApplicationDbContext();
        }

        public CompletedAssignmentsController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        // GET: CompletedAssignments
        public async Task<ActionResult> Index(int? id, int? classroomId)
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var viewModel = new VirtualClassroomViewModel()
            {
                Assignment = _context.Assignments.Find(id),
                VirtualClassroom = _context.VirtualClassrooms.Find(classroomId),
                VirtualClassrooms = user.Instructor.VirtualClassrooms,
                CompletedAssignments = _context.CompletedAssignments
                .Include(s => s.Student)
                .Where(s => s.Submitted 
                && s.AssignmentId == id
                && s.Graded == false).ToList()
            };
            return View(viewModel);
        }

        //// GET: CompletedAssignments/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    CompletedAssignment completedAssignment = db.CompletedAssignments.Find(id);
        //    if (completedAssignment == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(completedAssignment);
        //}

        //// GET: CompletedAssignments/Create
        //public ActionResult Create()
        //{
        //    ViewBag.AssignmentId = new SelectList(db.Assignments, "AssignmentId", "TaskTitle");
        //    ViewBag.StudentId = new SelectList(db.Students, "StudentId", "FirstName");
        //    return View();
        //}

        //// POST: CompletedAssignments/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "CompletedAssignmentId,CompletedDateTime,AssignmentId,StudentId")] CompletedAssignment completedAssignment)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.CompletedAssignments.Add(completedAssignment);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.AssignmentId = new SelectList(db.Assignments, "AssignmentId", "TaskTitle", completedAssignment.AssignmentId);
        //    ViewBag.StudentId = new SelectList(db.Students, "StudentId", "FirstName", completedAssignment.StudentId);
        //    return View(completedAssignment);
        //}

        // GET: CompletedAssignments/Edit/5
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
                CompletedAssignment = _context.CompletedAssignments.Find(id)
            };
            if (viewModel == null)
            {
                return HttpNotFound();
            }
            //ViewBag.AssignmentId = new SelectList(db.Assignments, "AssignmentId", "TaskTitle", completedAssignment.CompletedAssignment.AssignmentId);
            //ViewBag.StudentId = new SelectList(db.Students, "StudentId", "FirstName", completedAssignment.CompletedAssignment.StudentId);
            return View(viewModel);
        }

        // POST: CompletedAssignments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(VirtualClassroomViewModel viewModel)
        {
            var assignment = _context.Assignments.Find(viewModel.CompletedAssignment.AssignmentId);
            var editedAssignment = _context.CompletedAssignments.Find(viewModel.CompletedAssignment.Id);
            editedAssignment.Grade = _context.Grades.Find(viewModel.CompletedAssignment.GradeId);
            editedAssignment.Grade.PointsReceived = viewModel.CompletedAssignment.Grade.PointsReceived;
            editedAssignment.Grade.GradePercentage = Grade.GetGradePercentage(assignment.PointsWorth, viewModel.CompletedAssignment.Grade.PointsReceived);
            editedAssignment.Graded = true;
            if (ModelState.IsValid)
            {
                _context.CompletedAssignments.AddOrUpdate(editedAssignment);
                //db.Entry(editedAssignment).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Index", new { id = viewModel.CompletedAssignment.AssignmentId, classroomId = viewModel.CompletedAssignment.VirtualClassroomId });
            }
            ViewBag.AssignmentId = new SelectList(_context.Assignments, "Id", "TaskTitle", viewModel.CompletedAssignment.AssignmentId);
            ViewBag.StudentId = new SelectList(_context.Students, "StudentId", "FirstName", viewModel.StudentId);
            return View(viewModel);
        }

        //// GET: CompletedAssignments/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    CompletedAssignment completedAssignment = db.CompletedAssignments.Find(id);
        //    if (completedAssignment == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(completedAssignment);
        //}

        //// POST: CompletedAssignments/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    CompletedAssignment completedAssignment = db.CompletedAssignments.Find(id);
        //    db.CompletedAssignments.Remove(completedAssignment);
        //    db.SaveChanges();
        //    return RedirectToAction("Index", new { id = completedAssignment.AssignmentId });
        //}

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
