using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using XCoder;

namespace Web码神工具.Controllers
{
    [Route("api/[controller]")]
    public class FileController : Controller
    {
        // 获取模板文件列表
        [HttpGet("[action]")]
        public List<dynamic> GetTemplateList()
        {
            var list = new List<dynamic>();

            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Engine.TemplatePath);
            if (Directory.Exists(dir)) dir = Directory.GetCurrentDirectory().EnsureEnd("/") + Engine.TemplatePath;
            if (Directory.Exists(dir))
            {
                var ds = Directory.GetDirectories(dir);
                if (ds != null && ds.Length > 0)
                {
                    foreach (var item in ds)
                    {
                        var di = new DirectoryInfo(item);
                        var node = new
                        {
                            title = di.Name,
                            path = di.FullName,
                            children = di.GetFiles().ToList().Select(s => new
                            {
                                title = s.Name,
                                path = s.FullName
                            })
                        };
                        list.Add(node);
                    }
                }
            }
            return list;
        }

        // 获取文件内容
        [HttpGet("[action]")]
        public string GetFileContent(string path)
        {
            if (System.IO.File.Exists(path))
            {
                return System.IO.File.ReadAllText(path);
            }

            return null;
        }

        // 保存文件内容
        [HttpPost("[action]")]
        public bool Save(string path, [FromForm]string contents)
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.File.WriteAllText(path, contents);
                return true;
            }

            return false;
        }
    }
}
