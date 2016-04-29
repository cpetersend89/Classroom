using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Classroom.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Classroom.Controllers
{
    public class AdminsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AdminsController()
        {
        }

        public AdminsController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        // GET: Admins
        public ActionResult Index()
        {
            return View(db.Admins.ToList());
        }

        // GET: Admins/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Admin admin = db.Admins.Find(id);
            if (admin == null)
            {
                return HttpNotFound();
            }
            return View(admin);
        }

        // GET: Admins/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admins/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Admin model, IdentityUserRole role)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true,
                    Admin = new Admin()
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        Active = model.Active
                    }
                };
                var adminRole = db.Roles.FirstOrDefault(x => x.Name == "Admin");
                var userRole = new IdentityUserRole()
                {
                    RoleId = adminRole.Id,
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
            return View(model);
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

        // GET: Admins/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Admin admin = db.Admins.Find(id);
            if (admin == null)
            {
                return HttpNotFound();
            }
            return View(admin);
        }

        // POST: Admins/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AdminId,FirstName,LastName,Active")] Admin admin)
        {
            if (ModelState.IsValid)
            {
                db.Entry(admin).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(admin);
        }

        // GET: Admins/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Admin admin = db.Admins.Find(id);
            if (admin == null)
            {
                return HttpNotFound();
            }
            return View(admin);
        }

        // POST: Admins/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Admin admin = db.Admins.Find(id);
            var user = db.Users.FirstOrDefault(x => x.Admin.AdminId == id);
            db.Users.Remove(user);
            db.SaveChanges();
            db.Admins.Remove(admin);
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
