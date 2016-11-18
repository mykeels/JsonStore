using System;
using System.Collections.Generic;
using System.Linq;
using JsonStore.Mvc.Data;

namespace JsonStore.Mvc.Models
{
    public partial class User
    {
        public class Role
        {
            public long ID { get; set; }
            public string Name { get; set; }
            public bool Deleted { get; set; }

            public static List<Role> Roles { get; set; }

            public tbl_UserRole Map()
            {
                tbl_UserRole ret = new tbl_UserRole();
                ret.ID = this.ID;
                ret.Name = this.Name;
                ret.Deleted = this.Deleted;
                return ret;
            }

            public static Role Map(tbl_UserRole role)
            {
                Role ret = new User.Role();
                ret.Deleted = role.Deleted;
                ret.ID = role.ID;
                ret.Name = role.Name;
                return ret;
            }

            public static List<Role> GetRoles()
            {
                if (Roles != null && Roles.Count > 0) return Roles;
                else
                {
                    Roles = new List<Role>();
                    using (JsonStoreDataContext db = new JsonStoreDataContext())
                    {
                        foreach (tbl_UserRole role in db.tbl_UserRoles.ToList())
                        {
                            Roles.Add(Map(role));
                        }
                        return Roles;
                    }
                }
            }

            public static Role GetRole(long ID)
            {
                return GetRoles().Where((role) => role.ID == ID).FirstOrDefault();
            }
        }
    }
}
