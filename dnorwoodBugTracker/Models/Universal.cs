using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace dnorwoodBugTracker.Models
{
    public class Universal : Controller

    {
        public ApplicationDbContext db = new ApplicationDbContext();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = db.Users.Find(User.Identity.GetUserId());

                ViewBag.FirstName = user.FirstName;
                ViewBag.LastName = user.LastName;
                ViewBag.FullName = user.FullName;
                ViewBag.UserTimeZone = user.TimeZone;

                ViewBag.Notifications = user.Notifications.OrderByDescending(n => n.Id).ToList();

                base.OnActionExecuting(filterContext);
            }
        }
    }
}