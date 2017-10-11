using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using dnorwoodBugTracker.Models;
using dnorwoodBugTracker.Models.CodeFirst;
using Microsoft.AspNet.Identity;
using dnorwoodBugTracker.Models.Helper;

namespace dnorwoodBugTracker.Controllers
{
    [Authorize]
    public class ProjectsController : Universal
    {
        // GET: Projects
        public ActionResult Index()
        {
            List<Project> projects = new List<Project>();
            if(User.IsInRole("Admin") || User.IsInRole("Project Manager"))
                projects = db.Projects.ToList();
            else if (User.IsInRole("Developer") || User.IsInRole("Submitter"))
            {
                ApplicationUser user = db.Users.Find(User.Identity.GetUserId());
                projects = user.Projects.ToList();
            }

            return View(projects);
        }
    

        // GET: Projects/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }


        // GET: Projects/Create
        [Authorize(Roles = "Admin,Project Manger")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Created,Updated,Title,Description,AuthorId")] Project project)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                project.AuthorId = user.Id;
                db.Projects.Add(project);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin,Project Manager")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        //GET: Projects/Assgin
        [Authorize (Roles = "Admin, Project Manager")]
        public ActionResult ProjectAssign(int id, string ticket )
        {
            var project = db.Projects.Find(id);
            var helper = new ProjectAssignHelper();
            var model = new ProjectsViewModel();
            model.AssignProject = project;
            model.AssignProjectId = id;
            model.SelectedUsers = helper.ListUsersOnProject(id).Select(p => p.Id).ToArray();
            model.Users = new MultiSelectList(db.Users.ToList(), "Id", "FullName", model.SelectedUsers);

            return View(model);
        }

        //POST: Projects/Assign
        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult ProjectAssign(ProjectsViewModel model)
        {
            ProjectAssignHelper helper = new ProjectAssignHelper();
            foreach (var userId in db.Users.Select(r => r.Id).ToList())
            {
                helper.RemoveUserFromProject(userId, model.AssignProjectId);
            }
            foreach (var userId in model.SelectedUsers)
            {
                helper.AddUserToProject(userId, model.AssignProjectId);
            }

            return RedirectToAction("Index");
        }

        //[Authorize(Roles = "Admin, ProjectManager, Developer, Submitter")]
        //public ActionResult ProjectAssignedTo(int id)
        //{
        //    var project = db.Users.Find(id);
        //    return db.Projects.ToList();
        //}

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Created,Updated,Title,Description,AuthorId")] Project project)
        {
                if (ModelState.IsValid)
            {
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(project);

        }


        // GET: Projects/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Project project = db.Projects.Find(id);
            db.Projects.Remove(project);
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
