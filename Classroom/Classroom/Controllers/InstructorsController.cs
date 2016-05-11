using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Classroom.Models;
using Classroom.ViewModels;
using Microsoft.AspNet.Identity.EntityFramework;
using PagedList;

namespace Classroom.Controllers
{
    public class InstructorsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public InstructorsController()
        {
        }

        public InstructorsController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        // GET: Instructors
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.IdSortParm = sortOrder == "Id" ? "id_desc" : "Id";
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;
            var instructors = from s in db.Instructors
                              select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                instructors = instructors.Where(s => s.LastName.Contains(searchString)
                                       || s.FirstName.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    instructors = instructors.OrderByDescending(s => s.LastName);
                    break;
                case "Id":
                    instructors = instructors.OrderBy(s => s.InstructorId);
                    break;
                case "id_desc":
                    instructors = instructors.OrderByDescending(s => s.InstructorId);
                    break;
                default:
                    instructors = instructors.OrderBy(s => s.LastName);
                    break;
            }

            int pageSize = 3;
            int pageNumber = (page ?? 1);
            return View(instructors.ToPagedList(pageNumber, pageSize));
        }

        // GET: Instructors/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Instructor instructor = db.Instructors.Find(id);
            if (instructor == null)
            {
                return HttpNotFound();
            }
            return View(instructor);
        }

        // GET: Instructors/Create
        public ActionResult Create()
        {
            //ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentName");
            return View();
        }

        // POST: Instructors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Instructor instructor, IdentityUserRole role)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = instructor.Email,
                    Email = instructor.Email,
                    EmailConfirmed = true,
                    Instructor = new Instructor()
                    {
                        HireDate = instructor.HireDate,
                        FirstName = instructor.FirstName,
                        LastName = instructor.LastName,
                        Email = instructor.Email,
                        Active = instructor.Active
                    }
                };
                var instructorRole = db.Roles.FirstOrDefault(x => x.Name == "Instructor");
                var userRole = new IdentityUserRole()
                {
                    RoleId = instructorRole.Id,
                    UserId = user.Id
                };

                var result = await UserManager.CreateAsync(user); // Create without password.
                if (result.Succeeded)
                {
                    db.Set<IdentityUserRole>().AddOrUpdate(userRole);
                    db.SaveChanges();
                    await SendActivationMail(user);
                    return RedirectToAction("Index");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
            }
            //ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentName", instructor.Department.DepartmentId);
            return View(instructor);
        }


        private async Task SendActivationMail(ApplicationUser user)
        {
            string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);

            // Using protocol param will force creation of an absolut url. We
            // don't want to send a relative URL by e-mail.
            var callbackUrl = Url.Action(
              "CreatePasswordFromEmail",
              "Account",
              new { userId = user.Id, code = code },
              protocol: Request.Url.Scheme);

            string body = $"<h4>Hello {user.Admin.FirstName}, welcome to Classroom!</h4>" +
                          $"<p>To get started, please <a href='{callbackUrl}'>activate</a> your account.</p>" +
                          $"<p>The account must be activated within 24 hours from receiving this mail.</p>";

            await UserManager.SendEmailAsync(user.Id, "Welcome to Classroom!", body);
    }

    // GET: Instructors/Edit/5
    public ActionResult Edit(int? id)
    {
        if (id == null)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        Instructor instructor = db.Instructors.Find(id);
        if (instructor == null)
        {
            return HttpNotFound();
        }
        //ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentName", instructor.Department.DepartmentId);
        return View(instructor);
    }

    // POST: Instructors/Edit/5
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit([Bind(Include = "InstructorId,HireDate,FirstName,LastName,DepartmentId,Active")] Instructor instructor)
    {
        if (ModelState.IsValid)
        {
            db.Entry(instructor).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        //ViewBag.DepartmentId = new SelectList(db.Departments, "DepartmentId", "DepartmentName", instructor.Department.DepartmentId);
        return View(instructor);
    }

    // GET: Instructors/Delete/5
    public ActionResult Delete(int? id)
    {
        if (id == null)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        Instructor instructor = db.Instructors.Find(id);
        if (instructor == null)
        {
            return HttpNotFound();
        }
        return View(instructor);
    }

    // POST: Instructors/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
        Instructor instructor = db.Instructors.Find(id);
        var user = db.Users.FirstOrDefault(x => x.Instructor.InstructorId == id);
        db.Users.Remove(user);
        db.SaveChanges();
        db.Instructors.Remove(instructor);
        db.SaveChanges();
        return RedirectToAction("Index");
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
