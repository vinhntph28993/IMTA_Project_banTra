using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanTraiCay.Models;

namespace WebsiteBanTraiCay.Areas.Admin.Controllers
{
    public class ModulesController : Controller
    {
        private ConnectDbContext db = new ConnectDbContext();
        // GET: Admin/Modules
        public ActionResult LeftLayout()
        {
            return View();
        }
        public ActionResult LeftLayoutNoSession()
        {
            return View();
        }
        public ActionResult HeaderLayout()
        {
            ViewBag.Admins = db.Users.Where(m => m.Status != 0).First();
            return View();
        }
        public ActionResult LeftLayouts()
        {
            return View();
        }
    }
}