using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Web;
using Extensions;
using System.Web.Mvc;
using JsonStore.Mvc.Models;

namespace JsonStore.Mvc.Controllers
{
    public class ScriptsController : Controller
    {
        [Route("~/scripts/jsontoform")]
        public ActionResult JsonToForm()
        {
            return View();
        }

        [Route("~/scripts/jsontoformscript")]
        public JavaScriptResult JsonToFormScript()
        {
            return JavaScript("JsonToFormScript", null);
        }

        [Route("~/scripts/json/default.json")]
        public JsonResult GetDefaultJson()
        {
            return Json(new List<int>());
        }

        private JavaScriptResult JavaScript(string viewName, object model)
        {
            int num3;
            string input = this.RenderRazorViewToString(viewName, model);
            string pattern = string.Format("<{0}[^>]*>(.*?)</{0}>", "script");
            StringBuilder builder = new StringBuilder();
            MatchCollection matchs = Regex.Matches(input, pattern, RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase);
            for (int i = 0; i < matchs.Count; i = num3 + 1)
            {
                Match match = matchs[i];
                if (match.Success && (match.Groups.Count > 1))
                {
                    string str3 = "";
                    for (int j = 1; j < match.Groups.Count; j = num3 + 1)
                    {
                        str3 = str3 + match.Groups[j].Value;
                        num3 = j;
                    }
                    builder.Append(str3);
                    builder.AppendLine();
                }
                num3 = i;
            }
            return this.JavaScript(builder.ToString());
        }

        private string RenderRazorViewToString(string viewName, object model = null)
        {
            if (model != null)
            {
                ViewData.Model = model;
            }
            using (StringWriter writer = new StringWriter())
            {
                ViewEngineResult result = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext viewContext = new ViewContext(ControllerContext, result.View, ViewData, TempData, writer);
                result.View.Render(viewContext, writer);
                result.ViewEngine.ReleaseView(ControllerContext, result.View);
                return writer.GetStringBuilder().ToString();
            }
        }

    }
}
