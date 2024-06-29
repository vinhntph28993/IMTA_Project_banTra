using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebsiteBanTraiCay.Models;

namespace WebsiteBanTraiCay.Areas.Admin.Controllers
{
    public class EventController : Controller
    {
        private ConnectDbContext db = new ConnectDbContext();

        // GET: Admin/Event
        public ActionResult Index()
        {
            var list = db.Event.Where(m => m.Status != 0 && m.Type == "event").ToList();
            foreach (var row in list)
            {
                var temp_link = db.Links.Where(m => m.Type == "event" && m.TableId == row.ID);
                if (temp_link.Count() > 0)
                {
                    var row_link = temp_link.First();
                    row_link.Name = row.Name;
                    row_link.Slug = row.URL;
                    db.Entry(row_link).State = EntityState.Modified;
                }
                else
                {
                    var row_link = new MLink();
                    row_link.Name = row.Name;
                    row_link.Slug = row.URL;
                    row_link.Type = "event";
                    row_link.TableId = row.ID;
                    db.Links.Add(row_link);
                }
            }
            db.SaveChanges();
            return View(list);
        }

        // GET: Admin/Event/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MEvent @event = db.Event.Find(id);
            if (@event == null)
            {
                return HttpNotFound();
            }
            return View(@event);
        }

        // GET: Admin/Event/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Event/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MEvent Event)
        {

            if (ModelState.IsValid)
            {

                string URL = XString.ToAscii(Event.Name);
                Event.URL = URL;
                Event.Type = "event";
                Event.Created_At = DateTime.Now;
                CheckSlug check = new CheckSlug();
                if (!check.KiemTraSlug("Event", URL, null))
                {
                    Notification.set_flash("Tên Event đã tồn tại, vui lòng thử lại!", "warning");
                    return RedirectToAction("Create", "Event");
                }

                db.Event.Add(Event);
                db.SaveChanges();
                Notification.set_flash("Event đã được thêm!", "success");
                return RedirectToAction("Index");
            }
            Notification.set_flash("Có lỗi xảy ra khi thêm Event!", "warning");
            return View(Event);
        }

        // GET: Admin/Event/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MEvent @event = db.Event.Find(id);
            if (@event == null)
            {
                return HttpNotFound();
            }
            return View(@event);
        }

        // POST: Admin/Event/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,URL,Detail,Status")] MEvent @event)
        {
            if (ModelState.IsValid)
            {
                db.Entry(@event).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(@event);
        }

        // GET: Admin/Event/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MEvent @event = db.Event.Find(id);
            if (@event == null)
            {
                return HttpNotFound();
            }
            return View(@event);
        }

        // POST: Admin/Event/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MEvent @event = db.Event.Find(id);
            db.Event.Remove(@event);
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
