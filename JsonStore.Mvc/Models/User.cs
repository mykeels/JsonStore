using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Extensions.Models;
using JsonStore.Mvc.Data;

namespace JsonStore.Mvc.Models
{
    public partial class User
    {
        public long RoleID { get; set; }
        public bool Active { get; set; }
        public DateTime CreationTime { get; set; }
        public string Email { get; set; }
        public long ID { get; set; }
        private string Password { get; set; }
        private string PasswordHint { get; set; }
        public DateTime LastResetTime { get; set; }
        public DateTime LastActiveTime { get; set; }
        public string ResetToken { get; set; }
        public bool IsReset { get; set; }


        private string Token { get; set; }
        private DateTime TokenCreationTime { get; set; }

        public static List<User> LoginRequests = new List<User>();

        public static void Add(User user)
        {
            using (JsonStoreDataContext context = new JsonStoreDataContext())
            {
                context.tbl_Users.InsertOnSubmit(Map(user));
                context.SubmitChanges();
            }
        }

        public static tbl_User Map(User user)
        {
            if (user == null) return null;
            return new tbl_User
            {
                Active = user.Active,
                CreationTime = user.CreationTime,
                Email = user.Email,
                ID = user.ID,
                RoleID = Convert.ToInt64(user.RoleID),
                IsReset = user.IsReset,
                LastActiveTime = user.LastActiveTime,
                LastResetTime = user.LastResetTime,
                Password = user.Password,
                PasswordHint = user.PasswordHint,
                ResetToken = user.ResetToken
            };
        }

        public User Map(tbl_User user)
        {
            if (user == null) return null;
            this.Active = Convert.ToBoolean(user.Active);
            this.CreationTime = Convert.ToDateTime(user.CreationTime);
            this.Email = user.Email;
            this.ID = Convert.ToInt64(user.ID);
            this.RoleID = Convert.ToInt64(user.RoleID);
            this.IsReset = user.IsReset;
            this.LastActiveTime = Convert.ToDateTime(user.LastActiveTime);
            this.LastResetTime = Convert.ToDateTime(user.LastResetTime);
            this.Password = user.Password;
            this.PasswordHint = user.PasswordHint;
            this.ResetToken = user.ResetToken;
            return this;
        }

        public static bool ValidateLoginRequest(string token)
        {
            User user = LoginRequests.Where<User>(u => u.Token.Equals(token)).FirstOrDefault<User>();
            bool flag = (user != null);
            Site.Context().Session.AddSafe("json-site-user", user);
            LoginRequests.Remove(user);
            return flag;
        }

        public static bool Validate(string email, string password)
        {
            using (JsonStoreDataContext db = new JsonStoreDataContext())
            {
                var query = (from pp in db.tbl_Users
                             where pp.Email.ToLower().Equals(email.ToLower()) && pp.Password != null &&
     pp.Password.Equals(password.Encrypt("password")) && pp.Active
                             select pp).FirstOrDefault();
                Site.Context().Session.AddSafe("json-site-user", GetUser(email));
                return query != null;
            }
        }

        public static bool changePassword(string email, string newPassword, string newPasswordHint = "")
        {
            using (JsonStoreDataContext db = new JsonStoreDataContext())
            {
                var query = (from pp in db.tbl_Users where pp.Email.ToLower().Equals(email.ToLower()) && pp.Active select pp).FirstOrDefault();
                if (query != null)
                {
                    query.IsReset = false;
                    query.ResetToken = "";
                    query.Password = newPassword.Encrypt();
                    if (!String.IsNullOrEmpty(newPasswordHint)) query.PasswordHint = newPasswordHint;
                    db.SubmitChanges();
                    return true;
                }
            }
            return false;
        }

        public static bool resetPassword(string email)
        {
            using (JsonStoreDataContext db = new JsonStoreDataContext())
            {
                var query = (from pp in db.tbl_Users where pp.Email.ToLower().Equals(email.ToLower()) && pp.Active select pp).FirstOrDefault();
                if (query != null)
                {
                    query.IsReset = true;
                    query.ResetToken = "";
                    query.LastResetTime = DateTime.Now;
                    db.SubmitChanges();
                    return true;
                }
            }
            return false;
        }

        public static bool validatePasswordResetToken(string email, string token)
        {
            User user = GetUser(email);
            return (user != null && !String.IsNullOrEmpty(token) && user.ResetToken == token);
        }

        public static string CreateForgotPasswordToken(string email)
        {
            string resetToken = null;
            using (JsonStoreDataContext db = new JsonStoreDataContext())
            {
                var query = (from pp in db.tbl_Users where pp.Email.ToLower().Equals(email.ToLower()) && pp.Active select pp).FirstOrDefault();
                if (query != null)
                {
                    resetToken = "abcdefghijklmnopqrstuvwxyz".Shuffle().First(16);
                    query.ResetToken = resetToken;
                    db.SubmitChanges();
                }
            }
            return resetToken;
        }

        private static string GetRandomString(int count = 0x40)
        {
            int num3;
            string str = "";
            for (int i = 1; i <= count; i = num3 + 1)
            {
                double num2 = Number.Rnd();
                if (num2 <= 0.92)
                {
                    str = str + char.ConvertFromUtf32((int)(97.0 + Math.Floor((double)(num2 * 26.0)))).ToString();
                }
                else
                {
                    str = str + "-";
                }
                num3 = i;
            }
            return str;
        }

        public static User GetUser(string email = null, long id = 0L)
        {
            using (JsonStoreDataContext db = new JsonStoreDataContext())
            {
                return (new User()).Map((from pp in db.tbl_Users
                                         where (String.IsNullOrEmpty(email) || pp.Email.Equals(email)) &&
                 (id.Equals(0) || pp.ID.Equals(id))
                                         select pp).FirstOrDefault());
            }
        }

        public static bool AddLoginRequest(string email)
        {
            User item = null;
            User val = new User
            {
                Email = email
            };
            if (LoginRequests.Contains<User>(val, "email"))
            {
                LoginRequests.Where<User>(u => u.Email.Equals(email)).FirstOrDefault<User>().TokenCreationTime = DateTime.Now;
                return true;
            }
            item = GetUser(email, 0L);
            if (item != null)
            {
                item.Token = GetRandomString(0x20);
                item.TokenCreationTime = DateTime.Now;
                LoginRequests.Add(item);
                LoginRequests = LoginRequests.Distinct<User>("ID");
                KeyValue[] keyvalues = new KeyValue[3];
                KeyValue[] valueArray2 = new KeyValue[] { new KeyValue("[[link]]", Site.ResolveURL("~/invite/validate?t=" + item.Token)) };
                keyvalues[0] = new KeyValue("[[body]]", Mail.GetMailBody(Site.MapPath("~/mail/users/signin.html"), valueArray2));
                keyvalues[1] = new KeyValue("[[header]]", "You have attempted to sign in");
                keyvalues[2] = new KeyValue("[[subheader]]", "Click the link below to access your account");
                string mailBody = Mail.GetMailBody(Site.MapPath("~/mail/general/mail.html"), keyvalues);
                Mail.SendAsync(item.Email, "Sign in Request", mailBody, Site.AppSettings("fromEmailAddress"), "Sign in Request", null);
                return true;
            }
            return false;
        }

        public Role GetRole()
        {
            return Role.GetRole(this.RoleID);
        }

        public bool IsAdmin()
        {
            Role currentUserRole = this.GetRole();
            return currentUserRole.Name.ToLower().Equals("admin");
        }

        public bool IsRole(string roleName)
        {
            Role currentUserRole = this.GetRole();
            return currentUserRole.Name.ToLower().Equals(roleName);
        }

        public bool IsRoleOrAdmin(string roleName)
        {
            return this.IsAdmin() || this.IsRole(roleName);
        }
    }
}
