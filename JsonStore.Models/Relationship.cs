using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Extensions.Models;
using System.IO;

namespace JsonStore.Models
{
    public class JsonFileRelation : JsonFile
    {
        public List<JsonFileRelation> BackUps { get; set; }

        private JsonFileRelation Map(JsonFile file)
        {
            return new JsonFileRelation
            {
                Active = file.Active,
                BackupOf = file.BackupOf,
                BackUps = new List<JsonFileRelation>(),
                Contents = file.Contents,
                CreationTime = file.CreationTime,
                Description = file.Description,
                ID = file.ID,
                LastUpdateTime = file.LastUpdateTime,
                Name = file.Name,
                Path = file.Path
            };
        }

        public static List<JsonFileRelation> GetFileRelations()
        {
            List<JsonFileRelation> ret = new List<JsonFileRelation>();
            JsonFile.GetJsonFiles(0L).ForEach(delegate (JsonFile f)
            {
                ret.Add(new JsonFileRelation().Map(f));
            });
            return recurseList(ret.ToList<JsonFileRelation>());
        }

        private static List<JsonFileRelation> Map(List<JsonFile> files)
        {
            List<JsonFileRelation> list = new List<JsonFileRelation>();
            foreach (JsonFile file in files)
            {
                list.Add(new JsonFileRelation().Map(file));
            }
            return list;
        }

        private static List<JsonFileRelation> recurseList(List<JsonFileRelation> rfiles)
        {
            List<int> indices = new List<int>();
            int index = 0;
            rfiles.ForEach(delegate (JsonFileRelation rt)
            {
                if (rt.BackupOf > 0L)
                {
                    rfiles.Where<JsonFileRelation>(rf => (rf.ID == rt.BackupOf)).First<JsonFileRelation>().BackUps.Add(rt);
                    indices.Add(index);
                }
                int num1 = index;
                index = num1 + 1;
            });
            int sub = 0;
            indices.ForEach(delegate (int i)
            {
                rfiles.Remove(rfiles[i - sub]);
                int num1 = sub;
                sub = num1 + 1;
            });
            return rfiles;
        }

    }
}
