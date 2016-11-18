using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Extensions.Models;
using System.IO;

namespace JsonStore.Models
{
    public class JsonFile
    {
        public bool Active { get; set; }
        public long BackupOf { get; set; }
        public string Contents { get; set; }
        public DateTime CreationTime { get; set; }
        public string Description { get; set; }
        public long ID { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public long UserRoleID { get; set; }

        private static tbl_JsonFile Map(JsonFile jfile)
        {
            if (jfile == null) return null;
            return new tbl_JsonFile
            {
                CreationTime = new DateTime?(jfile.CreationTime),
                Description = jfile.Description,
                ID = jfile.ID,
                LastUpdateTime = new DateTime?(jfile.LastUpdateTime),
                Name = jfile.Name,
                Path = jfile.Path,
                Active = new bool?(jfile.Active),
                BackupOf = new long?(jfile.BackupOf),
                UserRoleID = jfile.UserRoleID
            };
        }

        private JsonFile Map(tbl_JsonFile jfile)
        {
            if (jfile == null) return null;
            this.ID = jfile.ID;
            this.Name = jfile.Name;
            this.CreationTime = Convert.ToDateTime(jfile.CreationTime);
            this.LastUpdateTime = Convert.ToDateTime(jfile.LastUpdateTime);
            this.Path = jfile.Path;
            this.Description = jfile.Description;
            this.Active = Convert.ToBoolean(jfile.Active);
            this.BackupOf = Convert.ToInt64(jfile.BackupOf);
            this.UserRoleID = Convert.ToInt64(jfile.UserRoleID);
            return this;
        }

        private static string encodeFileName(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return string.Empty;
            }
            char[] separator = new char[] { '.' };
            char[] chArray2 = new char[] { '.' };
            string str = filename.Split(separator).First<string>((filename.Split(chArray2).Count<string>() - 1)).Join(".").EncodeURI(null);
            char[] chArray3 = new char[] { '.' };
            string str2 = filename.Split(chArray3).Last<string>();
            return (str + "." + str2);
        }

        private static string getRootPath()
        {
            string rootpath = Site.AppSettings("json-store-root");
            if (String.IsNullOrEmpty(rootpath)) return "~/scripts/json/";
            return rootpath;
        }

        private static string getRelativeFilePath(string filename)
        {
            return getRootPath() + filename;
        }

        public static bool Add(ref JsonFile file)
        {
            if (!file.Name.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase))
            {
                file.Name = file.Name + ".json";
            }
            if (Exists(file.Name))
            {
                return false;
            }
            if (!string.IsNullOrEmpty(file.Contents))
            {
                file.Path = getRelativeFilePath(encodeFileName(file.Name));
                file.Contents.SaveToFile(Site.MapPath(file.Path));
            }
            file.CreationTime = DateTime.Now;
            file.LastUpdateTime = DateTime.Now;
            using (JsonStoreDataContext context = new JsonStoreDataContext())
            {
                context.tbl_JsonFiles.InsertOnSubmit(Map(file));
                context.SubmitChanges();
            }
            file.ID = GetJsonFile(0L, file.Name).ID;
            return true;
        }

        private static string createFolder(string path)
        {
            path = Site.MapPath(path);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public static JsonFile Edit(string path, string contents, long id = 0L)
        {
            string str = path.ToString();
            path = Site.MapPath(path);
            string str2 = getFileName(path);
            if (!File.Exists(path))
            {
                contents.SaveToFile(path);
            }
            else
            {
                string str3 = createFolder(getFolderPath(path) + "/" + str2.Replace(".", "-"));
                object[] objArray1 = new object[5];
                char[] separator = new char[] { '.' };
                char[] chArray2 = new char[] { '.' };
                objArray1[0] = str2.Split(separator).First<string>((str2.Split(chArray2).Count<string>() - 1)).Join(".");
                objArray1[1] = "-";
                objArray1[2] = Convert.ToInt32((int)(Directory.GetFiles(str3).Count<string>() + 1));
                objArray1[3] = ".";
                char[] chArray3 = new char[] { '.' };
                objArray1[4] = str2.Split(chArray3).LastOrDefault<string>("json");
                string filename = string.Concat(objArray1);
                File.Copy(path, str3 + "/" + encodeFileName(filename));
                contents.SaveToFile(path);
                if (!id.Equals((long)0L))
                {
                    JsonFile jsonFile = GetJsonFile(id, "");
                    JsonFile file3 = new JsonFile
                    {
                        Active = true,
                        BackupOf = id,
                        Contents = "",
                        CreationTime = DateTime.Now,
                        Description = "Backup of " + jsonFile.Name,
                        LastUpdateTime = DateTime.Now,
                        Name = filename
                    };
                    string[] textArray1 = new string[5];
                    char[] chArray4 = new char[] { '/', '\\' };
                    char[] chArray5 = new char[] { '/', '\\' };
                    textArray1[0] = str.Split(chArray4).First<string>((str.Split(chArray5).Count<string>() - 1)).Join("/");
                    textArray1[1] = "/";
                    textArray1[2] = str2.Replace(".", "-");
                    textArray1[3] = "/";
                    textArray1[4] = filename;
                    file3.Path = string.Concat(textArray1);
                    JsonFile file = file3;
                    Add(ref file);
                    return UpdateJsonFile(0L, filename);
                }
            }
            return null;
        }

        public static bool Exists(string filename)
        {
            using (JsonStoreDataContext db = new JsonStoreDataContext())
            {
                return (from pp in db.tbl_JsonFiles where pp.Name.Equals(filename) select pp).Count() > 0;
            }
        }

        public string GetContent()
        {
            return File.ReadAllText(Site.MapPath(this.Path));
        }

        private static string getFileName(string path)
        {
            char ch = '/';
            if (path.Contains(@"\"))
            {
                ch = '\\';
            }
            char[] separator = new char[] { ch };
            return path.Split(separator).LastOrDefault<string>("");
        }

        private static string getFolderPath(string path)
        {
            char ch = '/';
            if (path.Contains(@"\"))
            {
                ch = '\\';
            }
            char[] separator = new char[] { ch };
            string[] myarray = path.Split(separator);
            if (File.Exists(path))
            {
                return myarray.First<string>((myarray.Count<string>() - 1)).Join("/");
            }
            if (Directory.Exists(path))
            {
                return path;
            }
            return path;
        }

        public static JsonFile GetJsonFile(long id = 0, string filename = "")
        {
            JsonFile file = new JsonFile();
            using (JsonStoreDataContext db = new JsonStoreDataContext())
            {
                return (new JsonFile()).Map((from pp in db.tbl_JsonFiles where id.Equals(0) || pp.ID.Equals(id) select pp).FirstOrDefault());
            }
        }

        public static List<JsonFile> GetJsonFiles(long backupOf = 0L)
        {
            List<JsonFile> list = new List<JsonFile>();
            using (JsonStoreDataContext db = new JsonStoreDataContext())
            {
                foreach (var result in (from pp in db.tbl_JsonFiles
                                        where
            ((backupOf.Equals(0)) || pp.BackupOf.Equals(backupOf))
                                        select pp).ToList())
                {
                    list.Add((new JsonFile()).Map(result));
                }
            }
            return list;
        }

        public static List<string> GetPreviousStates(string path)
        {
            List<string> ret = new List<string>();
            path = Site.MapPath(path);
            string str = getFileName(path);
            if (File.Exists(path))
            {
                Directory.GetFiles(createFolder(getFolderPath(path) + "/" + str.Replace(".", "-"))).ToList<string>().ForEach(delegate (string file)
                {
                    ret.Add(file);
                });
            }
            return ret;
        }

        public static JsonFile Restore(long id, string contents)
        {
            JsonFile jsonFile = GetJsonFile(GetJsonFile(id, "").BackupOf, "");
            JsonFile file3 = Edit(jsonFile.Path, jsonFile.GetContent(), jsonFile.ID);
            jsonFile.UpdateContent(contents);
            return file3;
        }

        public void UpdateContent(string content)
        {
            File.WriteAllText(Site.MapPath(this.Path), content);
        }

        public static JsonFile UpdateJsonFile(long id = 0L, string filename = "")
        {
            JsonFile file = new JsonFile();
            using (JsonStoreDataContext db = new JsonStoreDataContext())
            {
                tbl_JsonFile jfile = (from pp in db.tbl_JsonFiles where id.Equals(0) || pp.ID.Equals(id) select pp).FirstOrDefault();
                jfile.LastUpdateTime = DateTime.Now;
                file = file.Map(jfile);
                db.SubmitChanges();
            }
            return file;
        }
    }
}
