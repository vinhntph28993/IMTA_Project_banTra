using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanTraiCay.Models;

namespace WebsiteBanTraiCay.Areas.Admin.Controllers
{
    public class DashboardController : BaseController
    {
        private ConnectDbContext db = new ConnectDbContext();
        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            ViewBag.CountOrderSuccess = db.Orders.Where(m => m.Status == 3).Count();
            ViewBag.CountOrderCancel = db.Orders.Where(m => m.Status == 1).Count();
            ViewBag.CountContactDoneReply = db.Contacts.Where(m => m.Flag == 0).Count();
            ViewBag.Total = 0;
            ViewBag.CountUser = db.Users.Where(m => m.Status != 0).Count();
            var thisMonth = DateTime.Now.Month;
            var thisYear = DateTime.Now.Year;
            ViewBag.ThisMonth = thisMonth;
            ViewBag.ThisYear = thisYear;

            ViewBag.countOrderSuccessThisMouth =
                db.Orders.Where(m => m.Status == 3 && m.CreateDate.Month == thisMonth && m.CreateDate.Year == thisYear).Count();
            ViewBag.countOrderCancelThisMouth =
                db.Orders.Where(m => m.Trash == 1 && m.CreateDate.Month == thisMonth && m.CreateDate.Year == thisYear).Count();
            ViewBag.countOrderWaitingThisMouth =
                db.Orders.Where(m => m.Status == 1 && m.Trash != 1 && m.CreateDate.Month == thisMonth && m.CreateDate.Year == thisYear).Count();
            ViewBag.countOrderSendingThisMouth =
                db.Orders.Where(m => m.Status == 2 && m.Trash != 1 && m.CreateDate.Month == thisMonth && m.CreateDate.Year == thisYear).Count();
            
            
            ViewBag.TongDoanhThu = ThongKeDoanhThu();
            ViewBag.SoDonHang = ThongKeDonHang();


            int days = DateTime.DaysInMonth(thisYear, thisMonth);
            List<Double> list = new List<Double>();
            for (int i = 1; i <= days; i++)
            {
                var odersInDay = db.Orders.Where(m => m.Status == 3 && m.CreateDate.Month == thisMonth && m.CreateDate.Year == thisYear && m.CreateDate.Day == i).ToList();
                Double sum = 0;
                foreach (var order in odersInDay)
                {
                    sum += db.Orderdetails.Where(m => m.OrderID == order.ID).Sum(m => m.Price * m.Quantity);
                }
                list.Add(sum);
                ViewBag.sum = sum;
            }
            ViewBag.dataBarChar = list;
            return View();
        }
        public decimal ThongKeDoanhThu()
        {
            
            decimal TongDoanhThu = ((decimal)db.Orderdetails.Sum(m => m.Quantity * m.Price));
            return TongDoanhThu;
        }
        /*public decimal ThongKeDoanhThuThang(int Thang, int Nam)
        {
            var lstDDH = db.Orders.Where(n => n.CreateDate.Month == Thang && n.CreateDate.Year == Nam);  //lấy ds đơn hàng có date tương ứng
            decimal TongDoanhThu = 0;
            foreach (var item in lstDDH) //duyệt chi tiết từng đơn và tính tổng tiền
            {
                TongDoanhThu += decimal.Parse(item.CreateDate.Sum(n => n.SoLuong * n.Dongia).Value.ToString());
            }
            return TongDoanhThu;
        }*/
        public double ThongKeDonHang()
        {
            double slddh = db.Orders.Where(m=>m.Status==3).Count();    //đếm số đơn hàng
            return slddh;
        }
    }
}