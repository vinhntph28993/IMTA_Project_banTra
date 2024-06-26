using CKFinder.Connector;
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
    public class ProductController : BaseController
    {
        private ConnectDbContext db = new ConnectDbContext();

        // GET: Admin/Product
        public ActionResult Index()
        {
            ViewBag.countTrash = db.Products.Where(m => m.Status == 0).Count();
            var list = from p in db.Products
                       join c in db.Categorys
                       on p.CateID equals c.ID
                       where p.Status != 0
                       where p.CateID == c.ID
                       orderby p.Created_at descending
                       select new ProductCategory()
                       {
                           ProductId = p.ID,
                           ProductImg = p.Image,
                           ProductName = p.Name,
                           ProductStatus = p.Status,
                           ProductDiscount = p.Discount,
                           CategoryName = c.Name
                       };
            return View(list.ToList());
        }
        public ActionResult Trash()
        {
            var list = from p in db.Products
                       join c in db.Categorys
                       on p.CateID equals c.ID
                       where p.Status == 0
                       where p.CateID == c.ID
                       orderby p.Created_at descending
                       select new ProductCategory()
                       {
                           ProductId = p.ID,
                           ProductImg = p.Image,
                           ProductName = p.Name,
                           ProductStatus = p.Status,
                           ProductDiscount = p.Discount,
                           CategoryName = c.Name
                       };
            return View(list.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                Notification.set_flash("Không tồn tại!", "warning");
                return RedirectToAction("Index");
            }
            MProduct mProduct = db.Products.Find(id);
            if (mProduct == null)
            {
                Notification.set_flash("Không tồn tại!", "warning");
                return RedirectToAction("Index");
            }
            return View(mProduct);
        }

        public ActionResult Create()
        {
            MCategory mCategory = new MCategory();
            ViewBag.ListCat = new SelectList(db.Categorys.Where(m => m.Status != 0), "ID", "Name", 0);
            ViewBag.ListOwner = new SelectList(db.ProductOwners.Where(x=>x.Status!=0).ToList(), "ID", "Name", 0);
            return View();
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MProduct mProduct)
        {
            ViewBag.ListCat = new SelectList(db.Categorys.Where(m => m.Status != 0), "ID", "Name", 0);
            
                mProduct.Price = mProduct.Price;
                mProduct.ProPrice = mProduct.ProPrice;

                String strSlug = XString.ToAscii(mProduct.Name);
                mProduct.Slug = strSlug;
                String strSlug1 = XString.ToAscii(mProduct.Name);
                mProduct.Slug = strSlug1;
                mProduct.Discount = 2;
                mProduct.Installment = 2;
                mProduct.Created_at = DateTime.Now;
                mProduct.Created_by = 1;
                mProduct.Updated_at = DateTime.Now;
                mProduct.Updated_by = 1;
                mProduct.NewPromotion = "None";
                mProduct.Specification = "None";
                mProduct.MetaKey = mProduct.Name;
                mProduct.MetaDesc = mProduct.Name;
            // Upload file
            var file = Request.Files["Image"];
                if (file != null && file.ContentLength > 0)
                {
                    String filename = strSlug + file.FileName.Substring(file.FileName.LastIndexOf("."));
                    mProduct.Image = filename;
                    String Strpath = Path.Combine(Server.MapPath("~/Public/images/products/"), filename);
                    file.SaveAs(Strpath);
                }
                var file1 = Request.Files["ImageList"];
                if (file1 != null && file1.ContentLength > 0)
                {
                    String filename1 = strSlug1 + "-1" + file1.FileName.Substring(file1.FileName.LastIndexOf("."));
                    mProduct.ImageList = filename1;
                    String Strpath1 = Path.Combine(Server.MapPath("~/Public/images/products/"), filename1);
                    file1.SaveAs(Strpath1);
                }
                db.Products.Add(mProduct);
                db.SaveChanges();
                Notification.set_flash("Thêm mới sản phẩm thành công!", "success");
                return RedirectToAction("Index");
        }

        public ActionResult Edit(int? id)
        {
            ViewBag.ListCat = new SelectList(db.Categorys.Where(x=>x.Status!= 0).ToList(), "ID", "Name", 0);
            ViewBag.ListOwner = new SelectList(db.ProductOwners.Where(x => x.Status != 0).ToList(), "ID", "Name", 0);
            MProduct mProduct = db.Products.Find(id);
            if (mProduct == null)
            {
                Notification.set_flash("404!", "warning");
                return RedirectToAction("Index", "Product");
            }
            return View(mProduct);
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MProduct mProduct)
        {
            ViewBag.ListCat = new SelectList(db.Categorys.Where(x => x.Status != 0).ToList(), "ID", "Name", 0);
            ViewBag.ListOwner = new SelectList(db.ProductOwners.Where(x => x.Status != 0).ToList(), "ID", "Name", 0);
                String strSlug = XString.ToAscii(mProduct.Name);
                mProduct.Slug = strSlug;
                mProduct.Slug = strSlug;
                String strSlug1 = XString.ToAscii(mProduct.Name);
                mProduct.Slug = strSlug1;
                mProduct.Discount = 2;
                mProduct.Installment = 2;
                mProduct.Updated_at = DateTime.Now;
                mProduct.Updated_by = 1;
                mProduct.NewPromotion = "None";
                mProduct.Specification = "None";
                mProduct.MetaKey = mProduct.Name;
                mProduct.MetaDesc = mProduct.Name;
            // Upload file
            var file = Request.Files["Image"];
                if (file != null && file.ContentLength > 0)
                {
                    String filename = strSlug + file.FileName.Substring(file.FileName.LastIndexOf("."));
                    mProduct.Image = filename;
                    String Strpath = Path.Combine(Server.MapPath("~/Public/images/products/"), filename);
                    file.SaveAs(Strpath);
                }
                var file1 = Request.Files["ImageList"];
                if (file1 != null && file1.ContentLength > 0)
                {
                    String filename1 = strSlug1 + "-1" + file1.FileName.Substring(file1.FileName.LastIndexOf("."));
                    mProduct.ImageList = filename1;
                    String Strpath1 = Path.Combine(Server.MapPath("~/Public/images/products/"), filename1);
                    file1.SaveAs(Strpath1);
                }
            db.Entry(mProduct).State = EntityState.Modified;
                db.SaveChanges();
                Notification.set_flash("Đã cập nhật lại thông tin sản phẩm!", "success");
                return RedirectToAction("Index");
        }

        public ActionResult DelTrash(int? id)
        {
            MProduct mProduct = db.Products.Find(id);
            mProduct.Status = 0;

            mProduct.Updated_at = DateTime.Now;
            mProduct.Updated_by = 1;
            db.Entry(mProduct).State = EntityState.Modified;
            db.SaveChanges();
            Notification.set_flash("Ném thành công vào thùng rác!" + " ID = " + id, "success");
            return RedirectToAction("Index");
        }
        public ActionResult Undo(int? id)
        {
            MProduct mProduct = db.Products.Find(id);
            mProduct.Status = 2;

            mProduct.Updated_at = DateTime.Now;
            mProduct.Updated_by = 1;
            db.Entry(mProduct).State = EntityState.Modified;
            db.SaveChanges();
            Notification.set_flash("Khôi phục thành công!" + " ID = " + id, "success");
            return RedirectToAction("Trash");
        }
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                Notification.set_flash("Không tồn tại !", "warning");
                return RedirectToAction("Trash");
            }
            MProduct mProduct = db.Products.Find(id);
            if (mProduct == null)
            {
                Notification.set_flash("Không tồn tại !", "warning");
                return RedirectToAction("Trash");
            }
            return View(mProduct);
        }

        // POST: Admin/Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MProduct mProduct = db.Products.Find(id);
            db.Products.Remove(mProduct);
            db.SaveChanges();
            Notification.set_flash("Đã xóa vĩnh viễn sản phẩm!", "danger");
            return RedirectToAction("Trash");
        }

        [HttpPost]
        public JsonResult changeStatus(int id)
        {
            MProductOwner mProductOwner = db.ProductOwners.Find(id);
            mProductOwner.Status = (mProductOwner.Status == 1) ? 2 : 1;

            mProductOwner.Updated_at = DateTime.Now;
            mProductOwner.Updated_by = 1;
            db.Entry(mProductOwner).State = EntityState.Modified;
            db.SaveChanges();
            return Json(new { Status = mProductOwner.Status });
        }
        [HttpPost]
        public JsonResult changeDiscount(int id)
        {
            MProduct mProduct = db.Products.Find(id);
            mProduct.Discount = (mProduct.Discount == 1) ? 2 : 1;

            mProduct.Updated_at = DateTime.Now;
            mProduct.Updated_by = 1;
            db.Entry(mProduct).State = EntityState.Modified;
            db.SaveChanges();
            
            return Json(new { Discount = mProduct.Discount });
        }

        public ActionResult Statistical()
        {
            var list = from o in db.Orders
                       join od in db.Orderdetails on o.ID equals od.OrderID
                       where o.Trash != 1
                       join p in db.Products on od.ProductID equals p.ID
                       group p by new { p.ID, p.Name, p.Image } into groupb
                       orderby groupb.Key.ID descending
                       select new ProductTop
                       {
                           Id = groupb.Key.ID,
                           Img = groupb.Key.Image,
                           Name = groupb.Key.Name,
                           Count = groupb.Sum(m=>m.Quantity)
                       };

            return View(list.ToList());
        }
    }
}
