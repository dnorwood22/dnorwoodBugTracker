using dnorwoodBugTracker.Models.CodeFirst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using System.Web.Mvc;
using System.Data.Entity;
using dnorwoodBugTracker.Models;

namespace BugTracker.Models.Helpers
{
    public class HistoryHelper : Universal
    {

        // User.Identity.GetUserId() will not work in this helper class for some unknown reason.
        // To compensate for this, the userId is being passed during instantiation.

        private string _userId;
        public HistoryHelper(string userId)
        {
            _userId = userId;
        }

        public void AddHistoryEvent(int ticketId, string changedProperty, string oldValue, string newValue)
        {
            TicketHistory newHistory = new TicketHistory();
            string userName = db.Users.AsNoTracking().First(t => t.Id == _userId).FullName;
            newHistory.AuthorId = _userId;
            newHistory.TicketId = ticketId;
            newHistory.Property = changedProperty;
            newHistory.OldValue = oldValue;
            newHistory.NewValue = newValue;
            newHistory.Created = DateTimeOffset.UtcNow;
            db.TicketHistories.Add(newHistory);
            db.SaveChanges();
        }

        public void AddHistoryEvent(int ticketId, string changedProperty, int oldValue, int newValue)
        {
            TicketHistory newHistory = new TicketHistory();
            string userName = db.Users.AsNoTracking().First(t => t.Id == _userId).FullName;
            newHistory.AuthorId = _userId;
            newHistory.TicketId = ticketId;
            newHistory.Property = changedProperty;
            newHistory.OldValue = oldValue.ToString();
            newHistory.NewValue = newValue.ToString();
            newHistory.Created = DateTimeOffset.UtcNow;
            db.TicketHistories.Add(newHistory);
            db.SaveChanges();
        }

        public void AddHistoryEvent(int ticketId, string changedProperty, bool oldValue, bool newValue)
        {
            TicketHistory newHistory = new TicketHistory();
            string userName = db.Users.AsNoTracking().First(t => t.Id == _userId).FullName;
            newHistory.AuthorId = _userId;
            newHistory.TicketId = ticketId;
            newHistory.Property = changedProperty;
            newHistory.OldValue = oldValue.ToString();
            newHistory.NewValue = newValue.ToString();
            newHistory.Created = DateTimeOffset.UtcNow;
            db.TicketHistories.Add(newHistory);
            db.SaveChanges();
        }
    }
}