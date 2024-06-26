using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebsiteBanTraiCay.Models;

namespace WebsiteBanTraiCay.Areas.Admin.Controllers
{
    public class ConfigController : BaseController
    {
        private ConnectDbContext db = new ConnectDbContext();

        // GET: Admin/Config
        public ActionResult Index()
        {
            return View(db.Configs.ToList());
        }
        public ActionResult Favicon()
        {
            ViewBag.Configs = db.Configs.First();
            return View();
        }
        
        public ActionResult Dashboard()
        {
            ViewBag.Configs = db.Configs.First();
            return View(db.Configs.ToList());
        }
        // GET: Admin/Config/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MConfig mConfig = db.Configs.Find(id);
            if (mConfig == null)
            {
                return HttpNotFound();
            }
            return View(mConfig);
        }

        // GET: Admin/Config/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Config/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MConfig config)
        {

            String urllogo = XString.ToAscii(config.Name);
            config.Url = urllogo;
            if (ModelState.IsValid)
            {
                var file = Request.Files["LogoIconWeb"];
                if (file != null && file.ContentLength > 0)
                {
                    String filename = urllogo + file.FileName.Substring(file.FileName.LastIndexOf("."));
                    config.LogoIconWeb = filename;
                    String Strpath = Path.Combine(Server.MapPath("~/Public/images/iconweb/"), filename);
                    file.SaveAs(Strpath);
                }
                db.Configs.Add(config);
                db.SaveChanges();
                Notification.set_flash("Cấu hình Website thành công!", "success");
                return RedirectToAction("Index", "Dashboard");
            }
            Notification.set_flash("Có lỗi xảy ra khi thêm !", "warning");
            return View(config);
        }

        // GET: Admin/Config/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MConfig mConfig = db.Configs.Find(id);
            if (mConfig == null)
            {
                return HttpNotFound();
            }
            return View(mConfig);
        }

        // POST: Admin/Config/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MConfig config)
        {
            String strSlug = XString.ToAscii(config.Name);
            config.Url = strSlug;
            String strSlug1 = XString.ToAscii(config.Name);
            config.Url = strSlug1;
            if (ModelState.IsValid)
            {
                var file = Request.Files["LogoIconWeb"];
                if (file != null && file.ContentLength > 0)
                {
                    String filename = strSlug + file.FileName.Substring(file.FileName.LastIndexOf("."));
                    config.LogoIconWeb = filename;
                    String Strpath = Path.Combine(Server.MapPath("~/Public/images/iconweb/"), filename);
                    file.SaveAs(Strpath);
                }
                var file1 = Request.Files["LogoWeb"];
                if (file1 != null && file1.ContentLength > 0)
                {
                    String filename1 = strSlug1 +"-1"+ file1.FileName.Substring(file1.FileName.LastIndexOf("."));
                    config.LogoWeb = filename1;
                    String Strpath1 = Path.Combine(Server.MapPath("~/Public/images/iconweb/"), filename1);
                    file1.SaveAs(Strpath1);
                }
                db.Configs.Add(config);
                // db.SaveChanges();

                db.Entry(config).State = EntityState.Modified;
                db.SaveChanges();
                Notification.set_flash("Cập nhật cấu hình website thành công!", "success");
                return RedirectToAction("Index", "Dashboard");
            }
            Notification.set_flash("Có lỗi xảy ra khi sửa !", "warning");
            return View(config);
        }

        // GET: Admin/Config/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MConfig mConfig = db.Configs.Find(id);
            if (mConfig == null)
            {
                return HttpNotFound();
            }
            return View(mConfig);
        }

        // POST: Admin/Config/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MConfig mConfig = db.Configs.Find(id);
            db.Configs.Remove(mConfig);
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
