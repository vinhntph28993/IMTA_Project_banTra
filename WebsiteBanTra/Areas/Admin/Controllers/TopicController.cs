﻿using System;
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
    public class TopicController : BaseController
    {
        private ConnectDbContext db = new ConnectDbContext();

        // GET: Admin/Topic
        public ActionResult Index()
        {
            ViewBag.countTrash = db.Topics.Where(m => m.Status == 0).Count();
            var list = db.Topics.Where(m => m.Status != 0).ToList();

            foreach (var row in list)
            {
                var temp_link = db.Links.Where(m => m.Type == "topic" && m.TableId == row.ID);
                if (temp_link.Count() > 0)
                {
                    var row_link = temp_link.First();
                    row_link.Name = row.Name;
                    row_link.Slug = row.Slug;
                    db.Entry(row_link).State = EntityState.Modified;
                }
                else
                {
                    var row_link = new MLink();
                    row_link.Name = row.Name;
                    row_link.Slug = row.Slug;
                    row_link.Type = "topic";
                    row_link.TableId = row.ID;
                    db.Links.Add(row_link);
                }
            }
            db.SaveChanges();
            return View(list);
        }

        // GET: Admin/Topic/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MTopic mTopic = db.Topics.Find(id);
            if (mTopic == null)
            {
                return HttpNotFound();
            }
            return View(mTopic);
        }
        public ActionResult Status(int? id)
        {
            MTopic mTopic = db.Topics.Find(id);
            if (mTopic == null)
            {
                Notification.set_flash("Không tồn tại danh mục!", "warning");
                return RedirectToAction("Index");
            }
            mTopic.Status = (mTopic.Status == 1) ? 2 : 1;

            mTopic.Updated_at = DateTime.Now;
            mTopic.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(mTopic).State = EntityState.Modified;
            db.SaveChanges();
            Notification.set_flash("Thay đổi trạng thái thành công!" + " id = " + id, "success");
            return RedirectToAction("Index");
        }
        public ActionResult Create()
        {
            ViewBag.listTopic = new SelectList(db.Topics.Where(m => m.Status == 1), "ID", "Name", 0);
            ViewBag.listOrder = new SelectList(db.Topics.Where(m => m.Status == 1), "Order", "Name", 0);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MTopic mTopic)
        {
            ViewBag.listTopic = new SelectList(db.Topics.Where(m => m.Status == 1), "ID", "Name", 0);
            ViewBag.listOrder = new SelectList(db.Topics.Where(m => m.Status == 1), "Order", "Name", 0);
            if (ModelState.IsValid)
            {
                if (mTopic.ParentID == null)
                {
                    mTopic.ParentID = 0;
                }
                String Slug = XString.ToAscii(mTopic.Name);
                if (db.Categorys.Where(m => m.Slug == Slug).Count() > 0)
                {
                    Notification.set_flash("Tên danh mục đã tồn tại, vui lòng thử lại!", "warning");
                    return RedirectToAction("Create", "Topic");
                }
                if (db.Topics.Where(m => m.Slug == Slug).Count() > 0)
                {
                    Notification.set_flash("Tên danh mục đã tồn tại trong TOPIC, vui lòng thử lại!", "warning");
                    return RedirectToAction("Create", "Topic");
                }
                if (db.Posts.Where(m => m.Slug == Slug).Count() > 0)
                {
                    Notification.set_flash("Tên danh mục đã tồn tại trong POST, vui lòng thử lại!", "warning");
                    return RedirectToAction("Create", "Topic");
                }
                if (db.Products.Where(m => m.Slug == Slug).Count() > 0)
                {
                    Notification.set_flash("Tên danh mục đã tồn tại trong PRODUCT, vui lòng thử lại!", "warning");
                    return RedirectToAction("Create", "Topic");
                }


                mTopic.Slug = Slug;
                mTopic.Created_at = DateTime.Now;
                mTopic.Created_by = int.Parse(Session["Admin_ID"].ToString());
                mTopic.Updated_at = DateTime.Now;
                mTopic.Updated_by = int.Parse(Session["Admin_ID"].ToString());

                db.Topics.Add(mTopic);
                db.SaveChanges();
                Notification.set_flash("Danh mục đã được thêm!", "success");
                return RedirectToAction("Index", "Topic");
            }
            ViewBag.list = db.Categorys.Where(m => m.Status == 1).ToList();
            Notification.set_flash("Có lỗi xảy ra khi thêm danh mục!", "warning");
            return View(mTopic);
        }
        public ActionResult DelTrash(int id)
        {
            MTopic mTopic = db.Topics.Find(id);
            if (mTopic == null)
            {
                Notification.set_flash("Không tồn tại danh mục cần xóa vĩnh viễn!", "warning");
                return RedirectToAction("Index");
            }
            int count_child = db.Topics.Where(m => m.ParentID == id).Count();
            if (count_child != 0)
            {
                Notification.set_flash("Không thể xóa, danh mục có chủ đề con!", "warning");
                return RedirectToAction("Index");
            }
            mTopic.Status = 0;

            mTopic.Updated_at = DateTime.Now;
            mTopic.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(mTopic).State = EntityState.Modified;
            db.SaveChanges();
            Notification.set_flash("Ném thành công vào thùng rác!" + " ID = " + id, "success");
            return RedirectToAction("Index");
        }
        public ActionResult ReTrash(int? id)
        {
            MTopic cate = db.Topics.Find(id);
            if (cate == null)
            {
                Notification.set_flash("Không tồn tại chủ đề!", "danger");
                return RedirectToAction("Trash", "Topic");
            }
            cate.Status = 2;

            cate.Updated_at = DateTime.Now;
            cate.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(cate).State = EntityState.Modified;
            db.SaveChanges();
            Notification.set_flash("Khôi phục thành công!" + " ID = " + id, "success");
            return RedirectToAction("Trash", "Topic");
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                Notification.set_flash("Không tồn tại !", "warning");
                return RedirectToAction("Trash", "Topic");
            }
            MTopic mTopic = db.Topics.Find(id);
            if (mTopic == null)
            {
                Notification.set_flash("Không tồn tại!", "warning");
                return RedirectToAction("Trash", "Topic");
            }
            return View(mTopic);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MTopic mTopic = db.Topics.Find(id);
            db.Topics.Remove(mTopic);
            db.SaveChanges();
            Notification.set_flash("Đã xóa hoàn toàn chủ đề!", "success");
            return RedirectToAction("Trash", "Topic");
        }

        public ActionResult Trash()
        {
            return View(db.Topics.Where(m => m.Status == 0).ToList());
        }

        public ActionResult Edit(int? id)
        {
            ViewBag.listTopic = new SelectList(db.Topics.Where(m => m.Status == 1), "ID", "Name", 0);
            ViewBag.listOrder = new SelectList(db.Topics.Where(m => m.Status == 1), "Orders", "Name", 0);
            MTopic mTopic = db.Topics.Find(id);
            if (mTopic == null)
            {
                Notification.set_flash("404!", "warning");
                return RedirectToAction("Index", "Topic");
            }
            return View(mTopic);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MTopic mTopic)
        {
            ViewBag.listTopic = new SelectList(db.Topics.Where(m => m.Status == 1), "ID", "Name", 0);
            ViewBag.listOrder = new SelectList(db.Topics.Where(m => m.Status == 1), "Orders", "Name", 0);
            if (ModelState.IsValid)
            {
                if (mTopic.ParentID == null)
                {
                    mTopic.ParentID = 0;
                }
                String Slug = XString.ToAscii(mTopic.Name);
                int ID = mTopic.ID;
                if (db.Categorys.Where(m => m.Slug == Slug && m.ID != ID).Count() > 0)
                {
                    Notification.set_flash("Tên danh mục đã tồn tại, vui lòng thử lại!", "warning");
                    return RedirectToAction("Edit", "Topic");
                }
                if (db.Topics.Where(m => m.Slug == Slug && m.ID != ID).Count() > 0)
                {
                    Notification.set_flash("Tên danh mục đã tồn tại trong TOPIC, vui lòng thử lại!", "warning");
                    return RedirectToAction("Edit", "Topic");
                }
                if (db.Posts.Where(m => m.Slug == Slug && m.ID != ID).Count() > 0)
                {
                    Notification.set_flash("Tên danh mục đã tồn tại trong POST, vui lòng thử lại!", "warning");
                    return RedirectToAction("Edit", "Topic");
                }
                if (db.Products.Where(m => m.Slug == Slug && m.ID != ID).Count() > 0)
                {
                    Notification.set_flash("Tên danh mục đã tồn tại trong PRODUCT, vui lòng thử lại!", "warning");
                    return RedirectToAction("Edit", "Topic");
                }

                mTopic.Slug = Slug;

                // Lỗi datatime2
                mTopic.Created_at = DateTime.Now;
                mTopic.Created_by = int.Parse(Session["Admin_ID"].ToString());

                mTopic.Updated_at = DateTime.Now;
                mTopic.Updated_by = int.Parse(Session["Admin_ID"].ToString());

                db.Entry(mTopic).State = EntityState.Modified;
                db.SaveChanges();
                Notification.set_flash("Câp nhật thành công chủ đề!", "success");
                return RedirectToAction("Index");
            }
            ViewBag.list = db.Categorys.Where(m => m.Status == 1).ToList();
            return View(mTopic);
        }

        [HttpPost]
        public JsonResult changeStatus(int id)
        {
            MTopic mTopic = db.Topics.Find(id);
            mTopic.Status = (mTopic.Status == 1) ? 2 : 1;

            mTopic.Updated_at = DateTime.Now;
            mTopic.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(mTopic).State = EntityState.Modified;
            db.SaveChanges();
            return Json(new { Status = mTopic.Status });
        }
    }
}
