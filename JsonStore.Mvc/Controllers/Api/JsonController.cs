using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;
using Extensions;
using Extensions.Models;
using JsonStore.Models;
using Newtonsoft.Json.Linq;

namespace JsonStore.Controllers.Api
{
    public class JsonController : ApiController
    {
        [Route("~/api/json/add")]
        public Response<JsonFile> Add([FromBody] JsonFile values)
        {
            bool status = JsonFile.Add(ref values);
            return new Response<JsonFile>(status.If<string>("success").Else<string>("File aready Exists").Resolve<string>(), values, status);
        }

        [Route("~/api/json/edit")]
        public Response<JsonFile> Edit([FromBody] JObject values)
        {
            string filename = (string)values.GetValue("filename");
            long id = Convert.ToInt64(values.GetValue("id"));
            string contents = (string)values.GetValue("json");
            try
            {
                return new Response<JsonFile>("success", JsonFile.Edit(JsonFile.GetJsonFile(id, filename).Path, contents, id), true);
            }
            catch (Exception exception)
            {
                return new Response<JsonFile>(exception.Message, null, false);
            }
        }

        [HttpGet, Route("~/api/json/getfile")]
        public JObject GetFile(string url = "")
        {
            if (string.IsNullOrEmpty(url))
            {
                url = Site.ResolveURL("scripts/json/pressdata.json");
            }
            url = HttpUtility.UrlDecode(url);
            string json = Extensions.Api.Get(url);
            Site.Context().Response.ContentType = "application/json";
            return JObject.Parse(json);
        }

        [HttpGet, Route("~/api/json/getfiles")]
        public List<JsonFile> GetFiles() =>
            JsonFile.GetJsonFiles(0L);

        [HttpGet, Route("~/api/json/getpreviousstates")]
        public Response<List<string>> GetPreviousStates(long id = 0L)
        {
            string path = JsonFile.GetJsonFile(id, "").Path;
            return new Response<List<string>>("success", JsonFile.GetPreviousStates(path), false);
        }

        [HttpGet, Route("~/api/json/getrelations")]
        public List<JsonFileRelation> GetRelations() =>
            JsonFileRelation.GetFileRelations();

        [Route("~/api/json/restore")]
        public Response<JsonFile> Restore([FromBody] JObject values)
        {
            long id = Convert.ToInt64(values.GetValue("id"));
            string contents = (string)values.GetValue("json");
            return new Response<JsonFile>("success", JsonFile.Restore(id, contents), true);
        }
    }
}
