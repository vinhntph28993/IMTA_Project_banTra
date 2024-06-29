using System.Web.Mvc;
using WebsiteBanTraiCay.Models;

namespace WebsiteBanTraiCay.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration 
    {
        private ConnectDbContext db = new ConnectDbContext();
        public override string AreaName 
        {
            get 
            {
                return "Admin";
            }
        }
        

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
               "Profiles",
               "Admin/Profiles",
               new { Controller = "Auth", action = "Profiles", id = UrlParameter.Optional }
           );
            context.MapRoute(
               "Login",
               "Admin/Login",
               new { Controller = "Auth", action = "Login", id = UrlParameter.Optional }
           );
            context.MapRoute(
               "Logout",
               "Admin/Logout",
               new { Controller = "Auth", action = "Logout", id = UrlParameter.Optional }
           );
            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { Controller = "Dashboard", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}