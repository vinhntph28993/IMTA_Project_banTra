using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using WebsiteBanTraiCay.Library;
using WebsiteBanTraiCay.Models;

namespace WebsiteBanTraiCay.Areas.Admin.Controllers
{
    public class ContactController : BaseController
    {
        private ConnectDbContext db = new ConnectDbContext();

        public ActionResult Index()
        {
            ViewBag.countTrash = db.Contacts.Where(m => m.Status == 0).Count();
            return View(db.Contacts.Where(m => m.Status == 1).ToList());
        }
        public ActionResult Trash()
        {
            return View(db.Contacts.Where(m => m.Status == 0).ToList());
        }

        public ActionResult Reply(int? id)
        {
            if (id == null)
            {
                Notification.set_flash("Không tồn tại liên hệ từ khách hàng!", "danger");
                return RedirectToAction("Index", "Contact");
            }
            MContact mContact = db.Contacts.Find(id);
            if (mContact == null)
            {
                Notification.set_flash("Không tồn tại liên hệ từ khách hàng!", "danger");
                return RedirectToAction("Index", "Contact");
            }
            return View(mContact);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult Reply(mContact mContact)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        mContact.Status = 2;
        //        mContact.Updated_at = DateTime.Now;
        //        mContact.Updated_by = 1;

        //        String content = System.IO.File.ReadAllText(Server.MapPath("~/Areas/Admin/Views/Mail/D_Mail.html"));
        //        content = content.Replace("{{FullName}}", mContact.Fullname);
        //        content = content.Replace("{{Reply}}", mContact.Reply);
        //        content = content.Replace("{{RQ}}", mContact.Detail);
        //        content = content.Replace("{{AdminName}}", Session["User_Admin"].ToString());
        //        String subject = "Phản hồi liên hệ từ Giadunghiendai.com";
        //        //new MailHelper().SendMail(mContact.Email, subject, content);

        //        db.Entry(mContact).State = EntityState.Modified;
        //        db.SaveChanges();
        //        Notification.set_flash("Đã trả lời liên hệ!", "success");
        //        return RedirectToAction("Index");
        //    }
        //    return View(mContact);
        //}
        public ActionResult Reply(MContact mContact, String Email, String Phone)
        {
            if (ModelState.IsValid)
            {
                mContact.Flag = 1;
                mContact.Updated_at = DateTime.Now;
                mContact.Updated_by = int.Parse(Session["Admin_ID"].ToString());
                mContact.Email = Email;
                mContact.Phone = int.Parse(Phone.ToString());
                db.Entry(mContact).State = EntityState.Modified;
                db.SaveChanges();
                MailHelper helperV = new MailHelper();
                helperV.SendMail(Email, "Thông báo", "Admin Shop Kinh Doanh Trái Cây sẽ liên hệ qua SĐT: 0"+ Phone + "trong thời gian sớm nhất! ");
                Notification.set_flash("Đã trả lời liên hệ!", "success");
                return RedirectToAction("Index");
            }
            return View(mContact);
        }

        public ActionResult DelTrash(int id)
        {
            MContact mContact = db.Contacts.Find(id);
            if (mContact == null)
            {
                Notification.set_flash("Không tồn tại!", "warning");
                return RedirectToAction("Index");
            }
            mContact.Status = 0;
            mContact.Updated_at = DateTime.Now;
            mContact.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(mContact).State = EntityState.Modified;
            db.SaveChanges();
            Notification.set_flash("Ném thành công vào thùng rác!" + " ID = " + id, "success");
            return RedirectToAction("Index");
        }

        public ActionResult ReTrash(int? id)
        {
            MContact mContact = db.Contacts.Find(id);
            if (mContact == null)
            {
                Notification.set_flash("Không tồn tại danh mục!", "warning");
                return RedirectToAction("Trash", "Contact");
            }
            mContact.Status = 1;
            mContact.Updated_at = DateTime.Now;
            mContact.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(mContact).State = EntityState.Modified;
            db.SaveChanges();
            Notification.set_flash("Khôi phục thành công!" + " ID = " + id, "success");
            return RedirectToAction("Trash", "Contact");
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                Notification.set_flash("Không tồn tại!", "warning");
                return RedirectToAction("Index");
            }
            MContact mContact = db.Contacts.Find(id);
            if (mContact == null)
            {
                Notification.set_flash("Không tồn tại!", "warning");
                return RedirectToAction("Index");
            }
            return View(mContact);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MContact mContact = db.Contacts.Find(id);
            db.Contacts.Remove(mContact);
            db.SaveChanges();
            Notification.set_flash("Đã xóa vĩnh viễn liên hệ!", "danger");
            return RedirectToAction("Index");
        }
    }
}
