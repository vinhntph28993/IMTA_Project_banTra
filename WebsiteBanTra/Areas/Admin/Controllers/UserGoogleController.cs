using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using WebsiteBanTraiCay.Models;

namespace WebsiteBanTraiCay.Areas.Admin.Controllers
{
    public class UserGoogleController : Controller
    {
        private ConnectDbContext db = new ConnectDbContext();

        // GET: Admin/UserGoogle
        public ActionResult Index()
        {
            ViewBag.countTrash = db.UserGoogle.Where(m => m.Status == 0).Count();
            return View(db.UserGoogle.Where(m => m.Status != 0).ToList());
        }
        /*[CustomAuthorizeAttribute(RoleID = "ADMIN")]*/
        public ActionResult Trash()
        {
            return View(db.UserGoogle.Where(m => m.Status == 0).ToList());
        }
        /*[CustomAuthorizeAttribute(RoleID = "ADMIN")]*/
        public ActionResult Create()
        {
            return View();
        }
        /*[CustomAuthorizeAttribute(RoleID = "ADMIN")]*/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Exclude = "IsEmailVerified,ActivationCode")] MUserGoogle mUser)
        {
            bool Status = false;
            string message = "";

            if (ModelState.IsValid)
            {
                #region //Email is already Exist 
                var isExist = IsEmailExist(mUser.EmailID);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "Email already exist");
                    return View(mUser);
                }
                #endregion
                #region Generate Activation Code 
                mUser.ActivationCode = Guid.NewGuid();
                #endregion
                #region  Password Hashing 
                /* user.Password = Crypto.Hash(user.Password);*/
                mUser.Password = XString.ToMD5(mUser.Password);
                #endregion
                mUser.IsEmailVerified = false;
                #region Save to Database
                using (ConnectDbContext dc = new ConnectDbContext())
                {
                    /* mUser.Created_at = DateTime.Now;
                     mUser.Created_by = 1;
                     mUser.Updated_at = DateTime.Now;
                     mUser.Updated_by = 1;
                     dc.User.Add(mUser);
                     dc.SaveChanges();*/
                    //Send Email to User
                    SendVerificationLinkEmail(mUser.EmailID, mUser.ActivationCode.ToString());
                    Notification.set_flash("Đăng ký thành công. Liên kết kích hoạt tài khoản " +
                        " đã được gửi đến email của bạn:" + mUser.EmailID, "success");
                    /*message = "Đăng ký thành công. Liên kết kích hoạt tài khoản " +
                        " đã được gửi đến email của bạn:" + mUser.EmailID;*/
                    /*dc.Status = true;*/
                }
                #endregion

                String avatar = XString.ToAscii(mUser.Fullname);
                /*mUser.Password = XString.ToMD5(mUser.Password);*/
                mUser.Created_at = DateTime.Now;
                mUser.Created_by = 1;
                mUser.Updated_at = DateTime.Now;
                mUser.Updated_by = 1;


                var file = Request.Files["Image"];
                if (file != null && file.ContentLength > 0)
                {
                    String filename = avatar + file.FileName.Substring(file.FileName.LastIndexOf("."));
                    mUser.Image = filename;
                    String Strpath = Path.Combine(Server.MapPath("~/Public/images/user/"), filename);
                    file.SaveAs(Strpath);
                }

                db.UserGoogle.Add(mUser);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                Notification.set_flash("Yêu cầu không hợp lệ", "success");
                message = "Yêu cầu không hợp lệ";
            }
            return View(mUser);
        }

        /*[CustomAuthorizeAttribute(RoleID = "ADMIN")]*/
        public ActionResult DelTrash(int id)
        {
            MUserGoogle mUser = db.UserGoogle.Find(id);
            if (mUser == null)
            {
                Notification.set_flash("Không tồn tại User trên!", "warning");
                return RedirectToAction("Index");
            }

            mUser.Status = 0;

            mUser.Created_at = DateTime.Now;
            mUser.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            mUser.Updated_at = DateTime.Now;
            mUser.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(mUser).State = EntityState.Modified;
            db.SaveChanges();
            Notification.set_flash("Đã xóa vào thùng rác!" + " ID = " + id, "success");
            return RedirectToAction("Index");
        }
        /*[CustomAuthorizeAttribute(RoleID = "ADMIN")]*/
        // Delete to trash
        public ActionResult ReTrash(int? id)
        {
            MUserGoogle mUser = db.UserGoogle.Find(id);
            if (mUser == null)
            {
                Notification.set_flash("Không tồn tại User!", "warning");
                return RedirectToAction("Trash", "UserGoogle");
            }
            mUser.Status = 2;

            mUser.Updated_at = DateTime.Now;
            mUser.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(mUser).State = EntityState.Modified;
            db.SaveChanges();
            Notification.set_flash("Khôi phục thành công!" + " ID = " + id, "success");
            return RedirectToAction("Trash", "UserGoogle");
        }
        /*[CustomAuthorizeAttribute(RoleID = "ADMIN")]*/
        // GET: Admin/User/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MUserGoogle mUser = db.UserGoogle.Find(id);
            if (mUser == null)
            {
                return HttpNotFound();
            }
            return View(mUser);
        }

        /*[CustomAuthorizeAttribute(RoleID = "ADMIN")]*/
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MUserGoogle mUser = db.UserGoogle.Find(id);
            if (mUser == null)
            {
                return HttpNotFound();
            }
            return View(mUser);
        }
        /*[CustomAuthorizeAttribute(RoleID = "ADMIN")]*/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MUserGoogle mUser)
        {
            if (ModelState.IsValid)
            {
                #region //Email is already Exist 
                var isExist = IsEmailExist(mUser.EmailID);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "Email already exist");
                    return View(mUser);
                }
                #endregion
                #region Generate Activation Code 
                mUser.ActivationCode = Guid.NewGuid();
                #endregion
                #region  Password Hashing 
                /* user.Password = Crypto.Hash(user.Password);*/
                mUser.Password = XString.ToMD5(mUser.Password);
                #endregion
                mUser.IsEmailVerified = false;
                #region Save to Database
                using (ConnectDbContext dc = new ConnectDbContext())
                {
                    /*mUser.Created_at = DateTime.Now;
                    mUser.Created_by = 1;
                    mUser.Updated_at = DateTime.Now;
                    mUser.Updated_by = 1;*//*
                    dc.User.Add(mUser);
                    dc.SaveChanges();*/

                    //Send Email to User
                    SendVerificationLinkEmail(mUser.EmailID, mUser.ActivationCode.ToString());
                    Notification.set_flash("Đăng ký thành công. Liên kết kích hoạt tài khoản " +
                        " đã được gửi đến email của bạn:" + mUser.EmailID, "success");
                    /*message = "Đăng ký thành công. Liên kết kích hoạt tài khoản " +
                        " đã được gửi đến email của bạn:" + mUser.EmailID;*/
                    /*dc.Status = true;*/
                }
                #endregion
                String avatar = XString.ToAscii(mUser.Fullname);
                mUser.Password = XString.ToMD5(mUser.Password);
                mUser.Created_at = DateTime.Now;
                mUser.Created_by = int.Parse(Session["Admin_ID"].ToString());
                mUser.Updated_at = DateTime.Now;
                mUser.Updated_by = int.Parse(Session["Admin_ID"].ToString());

                var file = Request.Files["Image"];
                if (file != null && file.ContentLength > 0)
                {
                    String filename = avatar + file.FileName.Substring(file.FileName.LastIndexOf("."));
                    mUser.Image = filename;
                    String Strpath = Path.Combine(Server.MapPath("~/Public/images/user/"), filename);
                    file.SaveAs(Strpath);
                }
                db.Entry(mUser).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(mUser);
        }
        /*[CustomAuthorizeAttribute(RoleID = "ADMIN")]*/
        // GET: Admin/User/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MUserGoogle mUser = db.UserGoogle.Find(id);
            if (mUser == null)
            {
                return HttpNotFound();
            }
            return View(mUser);
        }
        /*[CustomAuthorizeAttribute(RoleID = "ADMIN")]*/
        // POST: Admin/User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MUserGoogle mUser = db.UserGoogle.Find(id);
            db.UserGoogle.Remove(mUser);
            db.SaveChanges();
            Notification.set_flash("Đã xóa hoàn User!", "success");
            return RedirectToAction("Trash");
        }
        /*[CustomAuthorizeAttribute(RoleID = "ADMIN")]*/
        [HttpPost]
        public JsonResult changeStatus(int id)
        {
            MUserGoogle mUser = db.UserGoogle.Find(id);
            mUser.Status = (mUser.Status == 1) ? 2 : 1;

            mUser.Updated_at = DateTime.Now;
            mUser.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(mUser).State = EntityState.Modified;
            db.SaveChanges();
            return Json(new
            {
                Status = mUser.Status
            });
        }

        //update

        [NonAction]
        public void SendVerificationLinkEmail(string emailID, string activationCode, string emailFor = "VerifyAccount")
        {
            var verifyUrl = "/Account/" + emailFor + "/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("testmailaspdotnet@gmail.com", "OnlineShop");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "elonvirhgtszchvr"; // Replace with actual password

            string subject = "";
            string body = "";
            if (emailFor == "VerifyAccount")
            {
                subject = "Tài khoản của bạn đã được tạo thành công!";
                body = "<br/><br/>Chúng tôi rất vui khi được thông báo với bạn rằng tài khoản OnlineShop của bạn là" +
                    " thành công trong việc tạo ra. Vui lòng nhấp vào liên kết dưới đây để xác minh tài khoản của bạn" +
                    " <br/><br/><a href='" + link + "'>" + link + "</a> ";

            }
            else if (emailFor == "ResetPassword")
            {
                subject = "Đặt lại mật khẩu";
                body = "Xin chào,<br/><br/>Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu tài khoản của bạn. Vui lòng nhấp vào liên kết dưới đây để đặt lại mật khẩu của bạn" +
                    "<br/><br/><a href=" + link + ">Đặt lại liên kết mật khẩu</a>";
            }


            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                Timeout = 10000,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string EmailID)
        {
            //Verify Email ID
            //Generate Reset password link 
            //Send Email 
            string message = "";
            bool status = false;

            using (ConnectDbContext dc = new ConnectDbContext())
            {
                var account = dc.UserGoogle.Where(a => a.EmailID == EmailID).FirstOrDefault();
                if (account != null)
                {
                    //Send email for reset password
                    string resetCode = Guid.NewGuid().ToString();
                    SendVerificationLinkEmail(account.EmailID, resetCode, "ResetPassword");
                    account.ResetPasswordCode = resetCode;
                    //This line I have added here to avoid confirm password not match issue , as we had added a confirm password property 
                    //in our model class in part 1
                    dc.Configuration.ValidateOnSaveEnabled = false;
                    dc.SaveChanges();
                    Notification.set_flash("Liên kết đặt lại mật khẩu đã được gửi đến email của bạn!", "success");
                }
                else
                {
                    message = "Tài khoản không được tìm thấy";
                }
            }
            ViewBag.Message = message;
            return View();
        }

        public ActionResult ResetPassword(string id)
        {
            //Verify the reset password link
            //Find account associated with this link
            //redirect to reset password page
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }

            using (ConnectDbContext dc = new ConnectDbContext())
            {
                var user = dc.UserGoogle.Where(a => a.ResetPasswordCode == id).FirstOrDefault();
                if (user != null)
                {
                    ResetPasswordModel model = new ResetPasswordModel();
                    model.ResetCode = id;
                    return View(model);
                }
                else
                {
                    return HttpNotFound();
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            var message = "";
            if (ModelState.IsValid)
            {
                using (ConnectDbContext dc = new ConnectDbContext())
                {
                    var user = dc.UserGoogle.Where(a => a.ResetPasswordCode == model.ResetCode).FirstOrDefault();
                    if (user != null)
                    {
                        /*user.Password = Crypto.Hash(model.NewPassword);*/
                        user.Password = XString.ToMD5(model.NewPassword);
                        user.ResetPasswordCode = "";
                        dc.Configuration.ValidateOnSaveEnabled = false;
                        dc.SaveChanges();
                        Notification.set_flash("Mật khẩu mới đã được cập nhật!", "success");
                    }
                }
            }
            else
            {
                /*message = "Bạn đã nhập không hợp lệ";*/
                Notification.set_flash("Bạn đã nhập không hợp lệ!", "danger");
            }
            ViewBag.Message = message;
            return View(model);
        }

        //Login 
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        //Login POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin login, string ReturnUrl = "")
        {
            string message = "";
            using (ConnectDbContext dc = new ConnectDbContext())
            {
                var v = dc.UserGoogle.Where(a => a.EmailID == login.EmailID).FirstOrDefault();
                if (v != null)
                {
                    if (!v.IsEmailVerified)
                    {
                        ViewBag.Message = "Please verify your email first";
                        return View();
                    }

                    if (string.Compare(Crypto.Hash(login.Password), v.Password) == 0)
                    {
                        int timeout = login.RememberMe ? 525600 : 20; // 525600 min = 1 year
                        var ticket = new FormsAuthenticationTicket(login.EmailID, login.RememberMe, timeout);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);


                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        message = "Invalid credential provided";
                    }
                }
                else
                {
                    message = "Invalid credential provided";
                }
            }
            ViewBag.Message = message;
            return View();
        }

        [NonAction]
        public bool IsEmailExist(string emailID)
        {
            using (ConnectDbContext dc = new ConnectDbContext())
            {
                var v = dc.UserGoogle.Where(a => a.EmailID == emailID).FirstOrDefault();
                return v != null;
            }
        }

        //Registration Action
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        //Registration POST action 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "IsEmailVerified,ActivationCode")] MUserGoogle user)
        {
            bool Status = false;
            string message = "";
            //
            // Model Validation 
            if (ModelState.IsValid)
            {

                #region //Email is already Exist 
                var isExist = IsEmailExist(user.EmailID);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "Email already exist");
                    return View(user);
                }
                #endregion

                #region Generate Activation Code 
                user.ActivationCode = Guid.NewGuid();
                #endregion

                #region  Password Hashing 
                /* user.Password = Crypto.Hash(user.Password);*/
                user.Password = XString.ToMD5(user.Password);
                #endregion
                user.IsEmailVerified = false;

                #region Save to Database
                using (ConnectDbContext dc = new ConnectDbContext())
                {
                    user.Created_at = DateTime.Now;
                    user.Created_by = 1;
                    user.Updated_at = DateTime.Now;
                    user.Updated_by = 1;
                    dc.UserGoogle.Add(user);
                    dc.SaveChanges();

                    //Send Email to User
                    SendVerificationLinkEmail(user.EmailID, user.ActivationCode.ToString());
                    message = "Registration successfully done. Account activation link " +
                        " has been sent to your email id:" + user.EmailID;
                    Status = true;
                }
                #endregion
            }
            else
            {
                message = "Invalid Request";
            }

            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(user);
        }
        //Verify Account  

        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool Status = false;
            using (ConnectDbContext dc = new ConnectDbContext())
            {
                dc.Configuration.ValidateOnSaveEnabled = false; // This line I have added here to avoid 
                                                                // Confirm password does not match issue on save changes
                var v = dc.UserGoogle.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
                if (v != null)
                {
                    v.IsEmailVerified = true;
                    dc.SaveChanges();
                    Status = true;
                }
                else
                {
                    Notification.set_flash("yêu cầu không hợp lệ!", "danger");
                    ViewBag.Message = "yêu cầu không hợp lệ";
                }
            }
            ViewBag.Status = Status;
            return View();
        }
    }
}
