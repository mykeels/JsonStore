using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Extensions;
using System.Web.Mvc;
using JsonStore.Mvc.Models;

namespace JsonStore.Controllers
{
    public class EditController : Controller
    {
        private string _validateLogin()
        {
            if (Session["json-edit-validated"] == null || Session["json-site-user"] == null)
            {
                return "~/invite";
            }
            User currentUser = (User)Session["json-site-user"];
            if (currentUser.IsReset && !Request.Url.ToString().Contains("changepassword")) return "~/edit/changepassword";
            return null;
        }

        [Route("~/edit/")]
        public ActionResult Index()
        {
            string _redirectUrl = _validateLogin();
            if (!String.IsNullOrEmpty(_redirectUrl)) return Redirect(_redirectUrl);
            if (!String.IsNullOrEmpty(Request.QueryString["id"]))
            {
                User currentUser = (User)Session["json-site-user"];
                if (currentUser != null && currentUser.IsAdmin())
                {
                    return View();
                }
                else return Redirect("~/edit");
            }
            return View();
        }

        [Route("~/edit/newfile")]
        public ActionResult NewFile()
        {
            string _redirectUrl = _validateLogin();
            if (!String.IsNullOrEmpty(_redirectUrl)) return Redirect(_redirectUrl);
            User currentUser = (User)Session["json-site-user"];
            if (currentUser != null && currentUser.IsAdmin())
            {
                return View();
            }
            else return Redirect("~/edit");
        }

        [Route("~/edit/report")]
        public ActionResult Report()
        {
            //Session.AddSafe("json-site-user", User.GetUser("michaelik@wakanow.com", 0));
            //Session.AddSafe("json-edit-validated", true);
            string _redirectUrl = _validateLogin();
            if (!String.IsNullOrEmpty(_redirectUrl)) return Redirect(_redirectUrl);
            return View();
        }

        [Route("~/edit/signout")]
        public ActionResult Signout()
        {
            Session.Remove("json-edit-validated");
            User item = (User)Session["json-site-user"];
            if (item != null)
            {
                JsonStore.Mvc.Models.User.LoginRequests.Remove(item);
            }
            Session.Remove("json-site-user");
            return RedirectToAction("Index");
        }

        [Route("~/edit/changepassword")]
        public ActionResult ChangePassword(string oldPassword = null, string newPassword = null)
        {
            string _redirectUrl = _validateLogin();
            if (!String.IsNullOrEmpty(_redirectUrl)) return Redirect(_redirectUrl);
            User currentUser = (User)Session["json-site-user"];
            ViewData.AddSafe("current-user", currentUser);
            if (!String.IsNullOrEmpty(newPassword))
            {
                if (currentUser.IsReset)
                {
                    JsonStore.Mvc.Models.User.changePassword(currentUser.Email, newPassword);
                    ViewData.AddSafe("success-message", "Password Change Successful");
                    currentUser.IsReset = false;
                    Session.AddSafe("json-site-user", currentUser);
                }
                else if (!String.IsNullOrEmpty(oldPassword))
                {
                    if (JsonStore.Mvc.Models.User.Validate(currentUser.Email, oldPassword))
                    {
                        JsonStore.Mvc.Models.User.changePassword(currentUser.Email, newPassword);
                        ViewData.AddSafe("success-message", "Password Change Successful");
                        currentUser.IsReset = false;
                        Session.AddSafe("json-site-user", currentUser);
                    }
                    else ViewData.AddSafe("error-message", "Invalid Old Password");
                }
                else ViewData.AddSafe("error-message", "Old Password not specified");
            }
            return View();
        }

        [Route("~/edit/validate")]
        public ActionResult Validate(string passKey)
        {
            if (passKey == Extensions.Models.Site.AppSettings("Site_PassKey"))
            {
                Session.AddSafe("json-edit-validated", true);
            }
            return RedirectToAction("Index");
        }

        [Route("~/edit/getroles")]
        public JsonResult GetRoles()
        {
            return Json(JsonStore.Mvc.Models.User.Role.GetRoles(), JsonRequestBehavior.AllowGet);
        }

    }
}
