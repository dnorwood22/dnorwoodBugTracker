using dnorwoodBugTracker.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace dnorwoodBugTracker.Controllers
{
    [Authorize]
    public class HomeController : Universal
    {
        public ActionResult Index()
        {
            return View(db.Projects.ToList());
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        [AllowAnonymous]
        public ActionResult Landing()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        //public async Task<ActionResult> Contact(EmailModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var body = "<p>Email From: <bold>{0}</bold>({1})</p><p>Message:</p><p>{2}</p>";
        //            var from = "MyBugTracker<dnorwood22@gmail.com>";
        //            var subject = "BugTracker Contact Email: (no subject)";

        //            var assignedUser = db.Users.Find(User.Identity.GetUserId());
        //            var ticket = db.Tickets.Find();
        //            var emailTo = assignedUser.Email;

        //            if (model.Subject != null)
        //            {
        //                subject = "Portfolio Contact Email: " + model.Subject;
        //            }


        //            var email = new MailMessage(from,
        //                        ConfigurationManager.AppSettings["emailto"])
        //            {
        //                Subject = subject,
        //                Body = string.Format(body, model.FromName, model.FromEmail, model.Body),
        //                IsBodyHtml = true
        //            };

        //            var svc = new PersonalEmail();
        //            await svc.SendAsync(email);

        //            return RedirectToAction("Sent");
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //            await Task.FromResult(0);
        //        }
        //    }
        //    return View(model);
        //}
    }
}