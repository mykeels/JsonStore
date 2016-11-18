using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Extensions.Models;
using System.Web.Mvc;

namespace JsonStore.Mvc.Controllers
{
    public class InviteController : Controller
    {
        [Route("~/invite/create")]
        public JsonResult Create(string email)
        {
            if (Models.User.AddLoginRequest(email))
            {
                return base.Json(new Response<object>("An Email has been sent to " + email + ". Please check your Mail and sign in.", null, true));
            }
            return base.Json(new Response<object>("We cannot find your Email in our records. Please see Admin to verify.", null, false));
        }

        [Route("~/invite/")]
        public ActionResult Index() => base.View();

        [Route("~/invite/login")]
        public ActionResult Login(string email = null, string password = null)
        {
            if (Session["success-message"] != null)
            {
                ViewData.AddSafe("success-message", Session["success-message"]);
                Session.Remove("success-message");
            }
            if (Session["error-message"] != null)
            {
                ViewData.AddSafe("error-message", Session["error-message"]);
                Session.Remove("error-message");
            } //<--- this code sucks ... lol

            if (!String.IsNullOrEmpty(email) && !String.IsNullOrEmpty(password))
            {
                if (!email.ToLower().EndsWith("wakanow.com"))
                {
                    ViewData.AddSafe("error-message", "Email must be in the Wakanow Domain");
                    return View();
                }
                if (Models.User.Validate(email, password))
                {
                    Site.Context().Session.AddSafe("json-edit-validated", true);
                    Models.User currentUser = (Models.User)Session["json-site-user"];
                    if (currentUser.IsRole("customer-feedback"))
                    {
                        return this.Redirect("~/customerfeedback/admin");
                    }
                    return this.Redirect("~/edit");
                }
                ViewData.AddSafe("error-message", "Invalid Email or Password");
            }
            return View();
        }

        [Route("~/invite/forgotpassword")]
        public ActionResult ForgotPassword(string email)
        {
            Models.User user = Models.User.GetUser(email);
            if (user != null && user.Active)
            {
                string resetToken = Models.User.CreateForgotPasswordToken(email);
                if (!String.IsNullOrEmpty(resetToken))
                {
                    string resetLink = Site.ResolveURL("~/invite/resetpassword?x=" + email.Encrypt() + "&y=" + resetToken);
                    string innerHtml = Mail.GetMailBody("~/mail/users/forgotpassword.html",
                        (new List<KeyValue>()).Push(new KeyValue("[[url]]", resetLink)).ToArray());

                    string html = Mail.GetMailBody("~/mail/general/mail.html",
                        (new List<KeyValue>()).Push(new KeyValue("[[url]]", resetLink)).Push(new KeyValue("[[header]]", "Password Reset Confirmation"))
                        .Push(new KeyValue("[[subheader]]", "")).Push(new KeyValue("[[body]]", innerHtml)).ToArray());
                    Mail.SendAsync(email, "Password Reset Confirmation", html, Site.AppSettings("EmailFromAddress"), "Wakanow NPS");
                    Session.AddSafe("success-message", "An Email has been sent to " + email + " containing a password reset link");
                }
            }
            else Session.AddSafe("error-message", "Invalid Email Address");
            return Redirect("~/invite/login");
        }

        [Route("~/invite/validate")]
        public ActionResult Validate(string t)
        {
            if (Models.User.ValidateLoginRequest(t))
            {
                Site.Context().Session.AddSafe("json-edit-validated", true);
                Models.User currentUser = (Models.User)Session["json-site-user"];
                if (currentUser.IsRole("customer-feedback"))
                {
                    return this.Redirect("~/customerfeedback/admin");
                }
                return this.Redirect("~/edit");
            }
            return this.Redirect("~/signin");
        }

        [Route("~/invite/resetpassword")]
        public ActionResult ResetPassword()
        {
            string email = Request.QueryString["x"];
            string token = Request.QueryString["y"];
            if (!String.IsNullOrEmpty(email) && !String.IsNullOrEmpty(token))
            {
                email = email.Replace(" ", "+").Decrypt();
                Models.User emailUser = Models.User.GetUser(email);
                if (emailUser != null && emailUser.Active && Models.User.validatePasswordResetToken(email, token))
                {
                    bool status = Models.User.resetPassword(email);
                    if (status)
                    {
                        Models.User currentUser = Models.User.GetUser(email);
                        if (currentUser != null)
                        {
                            Session.AddSafe("json-site-user", currentUser);
                            Session.AddSafe("json-edit-validated", true);
                            return Redirect("~/customerfeedback/admin");
                        }
                        else Session.AddSafe("error-message", "Server failed to find account with id => " + email);
                    }
                    else Session.AddSafe("error-message", "Server failed to reset password for " + email);
                }
                else Session.AddSafe("error-message", "Invalid Email Address or Reset Password Link for " + email);
            }
            else Session.AddSafe("error-message", "Invalid Email Address");
            return Redirect("~/invite/login");
        }
    }
}
