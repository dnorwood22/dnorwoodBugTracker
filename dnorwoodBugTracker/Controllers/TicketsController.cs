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
using System.IO;
using dnorwoodBugTracker.Models.Helper;

namespace dnorwoodBugTracker.Controllers
{
    [Authorize]
    public class TicketsController : Universal
    {

        // GET: Tickets
        public ActionResult Index()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var tickets = db.Tickets.Include(t => t.AssignToUser).Include(t => t.OwnerUser).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
            List<Ticket> Tickets = new List<Ticket>();

            if (User.IsInRole("Admin"))
            {
                return View(tickets.ToList());
            }
            else if (User.IsInRole("Project Manager"))
            {
                return View(tickets.Where(c => c.Project.Users.Any(u => u.Id == user.Id)));
            }
            else if (User.IsInRole("Developer"))
            {
                return View(tickets.Where(c => c.AssignToUserId == user.Id).ToList());
            }
            else if (User.IsInRole("Submitter"))
            {
                return View(tickets.Where(c => c.OwnerUserId == user.Id).ToList());
            }
            return View(tickets);
        }

        // GET: Tickets/Details/5
        public ActionResult Details(int? id)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }

            //Role Checking Security
            if (User.IsInRole("Admin"))
            {
                return View(ticket);
            }
            else if (User.IsInRole("Project Manager") && !ticket.Project.Users.Any(u => u.Id == user.Id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (User.IsInRole("Developer") && ticket.AssignToUserId != user.Id)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (User.IsInRole("Submitter") && ticket.OwnerUserId != user.Id)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (user.Roles.Count == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            return View(ticket);
        }

        // GET: Tickets/Create
        [Authorize(Roles = "Submitter")]
        public ActionResult Create()
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Title");
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name");
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Submitter")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Description,ProjectId,TicketTypeId,TicketPriorityId")] Ticket ticket)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            TicketHistory ticketHistory = new TicketHistory();

            if (ModelState.IsValid)
            {
                ticket.OwnerUserId = user.Id;
                ticket.Created = DateTimeOffset.UtcNow;
                ticket.Updated = DateTimeOffset.UtcNow;
                ticket.TicketStatusId = 1;
                db.Tickets.Add(ticket);

                ticketHistory.AuthorId = User.Identity.GetUserId();
                ticketHistory.Created = DateTimeOffset.UtcNow;
                ticketHistory.TicketId = ticket.Id;
                ticketHistory.Property = "NEW TICKET CREATED";
                db.TicketHistories.Add(ticketHistory);

                db.SaveChanges();

                return RedirectToAction("Index");
            }


            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Title", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            Ticket ticket = db.Tickets.Find(id);
            UserRoleHelper userRoleHelper = new UserRoleHelper();

            var developers = userRoleHelper.UserInRole("Developer");
            var devsOnProj = developers.Where(d => d.Projects.Any(p => p.Id == ticket.ProjectId));

            ViewBag.AssignToUserId = new SelectList(devsOnProj, "Id", "FullName", ticket.AssignToUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Title", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            ViewBag.UserTimeZone = db.Users.Find(User.Identity.GetUserId()).TimeZone;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
           
            if (ticket == null)
            {
                return HttpNotFound();
            }

            
            return View(ticket);
        }

        //POST: Tickets/Edit/5
        //To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //more details see https://go.microsoft.com/fwlink/?LinkId=317598.        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Description,Created,Updated,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,AssignToUserId")] Ticket ticket)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            TicketHistory ticketHistory = new TicketHistory();
            Ticket oldTicket = db.Tickets.AsNoTracking().First(t => t.Id == ticket.Id);

            if (ModelState.IsValid)
            {
                if(oldTicket.Title != ticket.Title)
                {
                    TicketHistory th = new TicketHistory();
                    th.Property = "TICKET EDITED: TITLE";
                    th.OldValue = oldTicket.Title;
                    th.NewValue = ticket.Title;
                    th.AuthorId = User.Identity.GetUserId();
                    th.TicketId = ticket.Id;
                    db.TicketHistories.Add(th);
                    db.SaveChanges();
                }
                if (oldTicket.Description != ticket.Description)
                {
                    TicketHistory th = new TicketHistory();
                    th.Property = "TICKET EDITED: DESCRIPTION";
                    th.OldValue = oldTicket.Description;
                    th.NewValue = ticket.Description;
                    th.AuthorId = User.Identity.GetUserId();
                    th.TicketId = ticket.Id;
                    db.TicketHistories.Add(th);
                    db.SaveChanges();
                }
                if (oldTicket.ProjectId != ticket.ProjectId)
                {
                    TicketHistory th = new TicketHistory();
                    th.Property = "TICKET EDITED: PROJECT";
                    th.OldValue = oldTicket.ProjectId.ToString();
                    th.NewValue = ticket.ProjectId.ToString();
                    th.AuthorId = User.Identity.GetUserId();
                    th.TicketId = ticket.Id;
                    db.TicketHistories.Add(th);
                    db.SaveChanges();
                }
                if (oldTicket.TicketTypeId != ticket.TicketTypeId)
                {
                    TicketHistory th = new TicketHistory();
                    th.Property = "TICKET EDITED: TYPE";
                    th.OldValue = oldTicket.TicketTypeId.ToString();
                    th.NewValue = ticket.TicketTypeId.ToString();
                    th.AuthorId = User.Identity.GetUserId();
                    th.TicketId = ticket.Id;
                    db.TicketHistories.Add(th);
                    db.SaveChanges();
                }
                if (oldTicket.TicketPriorityId != ticket.TicketPriorityId)
                {
                    TicketHistory th = new TicketHistory();
                    th.Property = "TICKET EDITED: PRIORITY";
                    th.OldValue = oldTicket.TicketPriorityId.ToString();
                    th.NewValue = ticket.TicketPriorityId.ToString();
                    th.AuthorId = User.Identity.GetUserId();
                    th.TicketId = ticket.Id;
                    db.TicketHistories.Add(th);
                    db.SaveChanges();
                }
                if (oldTicket.TicketStatusId != ticket.TicketStatusId)
                {
                    TicketHistory th = new TicketHistory();
                    th.Property = "TICKET EDITED: STATUS";
                    th.OldValue = oldTicket.TicketStatusId.ToString();
                    th.NewValue = ticket.TicketStatusId.ToString();
                    th.AuthorId = User.Identity.GetUserId();
                    th.TicketId = ticket.Id;
                    db.TicketHistories.Add(th);
                    db.SaveChanges();
                }
                if (oldTicket.AssignToUserId != ticket.AssignToUserId)
                {
                    TicketHistory th = new TicketHistory();
                    th.Property = "TICKET EDITED: ASSIGNED";
                    th.OldValue = oldTicket.AssignToUserId.ToString();
                    th.NewValue = ticket.AssignToUserId.ToString();
                    th.AuthorId = User.Identity.GetUserId();
                    th.TicketId = ticket.Id;
                    db.TicketHistories.Add(th);
                    db.SaveChanges();
                }

                db.Entry(ticket).State = EntityState.Modified;
                ticket.Updated = DateTimeOffset.Now;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            UserRoleHelper userRoleHelper = new UserRoleHelper();
            var developers = userRoleHelper.UserInRole("Developer");
            var devsOnProj = developers.Where(d => d.Projects.Any(p => p.Id == ticket.ProjectId));

            ViewBag.AssignToUserId = new SelectList(devsOnProj, "Id", "FullName", ticket.AssignToUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Title", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);

            return View(ticket);
        }

        //POST: TicketAttachments/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAttachments(IEnumerable<HttpPostedFileBase> files, int ticketId)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            Ticket ticket = db.Tickets.Find(ticketId);

            if (User.IsInRole("Admin") || (User.IsInRole("Project Manager") && ticket.Project.Users.Any(u => u.Id == user.Id)) || (User.IsInRole("Developer") && ticket.AssignToUserId == user.Id) || (User.IsInRole("Submitter") && ticket.OwnerUserId == user.Id))
            { 
                foreach (var file in files)
                {
                    TicketAttachment attachment = new TicketAttachment();
                    TicketHistory ticketHistory = new TicketHistory();

                    file.SaveAs(Path.Combine(Server.MapPath("~/TicketAttachments/"), Path.GetFileName(file.FileName)));
                    attachment.FileUrl = file.FileName;

                    attachment.AuthorId = User.Identity.GetUserId();
                    attachment.TicketId = ticketId;
                    attachment.Created = DateTimeOffset.UtcNow;
                    db.TicketAttachments.Add(attachment);

                    ticketHistory.AuthorId = User.Identity.GetUserId();
                    ticketHistory.Created = attachment.Created;
                    ticketHistory.TicketId = ticket.Id;
                    ticketHistory.NewValue = attachment.FileUrl;
                    ticketHistory.Property = "NEW TICKET ATTACHMENT";
                    db.TicketHistories.Add(ticketHistory);

                    db.SaveChanges();
                }
            }

            return RedirectToAction("Details", "Tickets", new { id = ticketId });
        }

        //POST: TicketComments/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddComments([Bind(Include = "Id,TicketId,Body")]TicketComment ticketComment)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var ticket = db.Tickets.Find(ticketComment.TicketId);
            TicketHistory ticketHistory = new TicketHistory();

            if (ModelState.IsValid && (User.IsInRole("Admin") || (User.IsInRole("Project Manager") && ticket.Project.Users.Any(u => u.Id == user.Id)) || (User.IsInRole("Developer") && ticket.AssignToUserId == user.Id) || (User.IsInRole("Submitter") && ticket.OwnerUserId == user.Id)))
            {

                ticketComment.AuthorId = User.Identity.GetUserId();
                ticketComment.TicketId = ticket.Id;
                ticketComment.Created = DateTimeOffset.UtcNow;
                db.TicketComments.Add(ticketComment);

                ticketHistory.AuthorId = User.Identity.GetUserId();
                ticketHistory.Created = ticket.Created;
                ticketHistory.TicketId = ticket.Id;
                ticketHistory.NewValue = ticketComment.Body;
                ticketHistory.Property = "NEW TICKET COMMENT";
                db.TicketHistories.Add(ticketHistory);
                db.SaveChanges();

                return RedirectToAction("Details", "Tickets", new { id = ticketComment.TicketId });
            }

            return RedirectToAction("Index", "Tickets");
        }


        // GET: Tickets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        //POST: Tickets/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ticket ticket = db.Tickets.Find(id);
            TicketHistory ticketHistory = new TicketHistory();

            db.Tickets.Remove(ticket);

            ticketHistory.AuthorId = User.Identity.GetUserId();
            ticketHistory.Created = ticket.Created;
            ticketHistory.TicketId = ticket.Id;
            ticketHistory.Property = "TICKET REMOVED";
            db.TicketHistories.Add(ticketHistory);  
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        //GET:TicketComments/Delete
        [Authorize(Roles = "Admin")]
        public ActionResult CommentDelete(int? id)
        {
            TicketComment comments = db.TicketComments.Find(id);
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (comments == null)
            {
                return HttpNotFound();
            }
            return View(comments);
        }

        // POST: Comments/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("CommentDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult CommentDeleteConfirmed(int id)
        {
            TicketComment comments = db.TicketComments.Find(id);
            TicketHistory ticketHistory = new TicketHistory();

         
            ticketHistory.AuthorId = User.Identity.GetUserId();
            ticketHistory.Created = comments.Created;
            ticketHistory.TicketId = comments.TicketId;
            ticketHistory.Property = "COMMENT REMOVED";
            db.TicketHistories.Add(ticketHistory);
            db.TicketComments.Remove(comments);
            db.SaveChanges();

            return RedirectToAction("Details", new { id = comments.TicketId });
        }

        //GET: TicketAttachments/Delete
        [Authorize(Roles = "Admin")]
        public ActionResult AttachmentDelete(int? id)
        {
            TicketAttachment attachments = db.TicketAttachments.Find(id);

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (attachments == null)
            {
                return HttpNotFound();
            }
            return View(attachments);
        }

        //POST: TicketAttachments/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("AttachmentDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult AttachmentDeleteConfirmed(int id)
        {
            TicketAttachment attachments = db.TicketAttachments.Find(id);
            TicketHistory ticketHistory = new TicketHistory();


            ticketHistory.AuthorId = User.Identity.GetUserId();
            ticketHistory.Created = attachments.Created;
            ticketHistory.TicketId = attachments.TicketId;
            ticketHistory.Property = "ATTACHMENT REMOVED";
            db.TicketHistories.Add(ticketHistory);
            db.TicketAttachments.Remove(attachments);
            db.SaveChanges();

            return RedirectToAction("Details", new { id = attachments.TicketId });
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
