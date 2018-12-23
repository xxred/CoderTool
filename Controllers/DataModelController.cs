using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.DependencyInjection;
using NewLife.Log;
using NewLife.Reflection;
using XCode.DataAccessLayer;
using XCode.Membership;
using XCoder;

namespace Web码神工具.Controllers
{
    [Route("api/[controller]")]
    public class DataModelController : Controller
    {
        private readonly IViewCompilerProvider _viewCompilerProvider;
        private readonly HtmlEncoder _htmlEncoder;
        private readonly DataModel _dataModel;
        public DataModelController(DataModel dataModel
        ,IViewCompilerProvider viewCompilerProvider
            ,HtmlEncoder htmlEncoder
        )
        {
            _dataModel = dataModel;
            _viewCompilerProvider = viewCompilerProvider;
            _htmlEncoder = htmlEncoder;
        }

        //[Route("[action]")]

        [HttpGet("[action]")]
        public string[] GetDatabaseList()
        {
            var list = DAL.ConnStrs.Keys.ToArray();
            return list;
        }

        [HttpGet("[action]")]
        public IEnumerable<string> Connect(string connName)
        {
            var list = DAL.Create(connName).Tables;
            var tabList = _dataModel.SortTables(list).Select(s=>s.ToString());
            return tabList;
        }

        [HttpGet("[action]")]
        public IEnumerable<string> GetTemplateList()
        {
            return _dataModel.GetTemplateList();
        }

        [HttpGet("[action]")]
        public string GenTable(ModelConfig cfg)
        {
            //_dataModel._Engine.Config = cfg;
            var tabName = Request.Query["tabName"];

            var tables = DAL.Create(cfg.ConnName).Tables;

            var table = tables.Find(f=>f.ToString() == tabName);

            var reg = new Regex(@"\$\((\w+)\)", RegexOptions.Compiled);

            #region 输出目录预处理
            var outpath = cfg.OutputPath;
            // 使用正则替换处理 命名空间处已经定义
            //var reg = new Regex(@"\$\((\w+)\)", RegexOptions.Compiled);
            outpath = reg.Replace(outpath, math =>
            {
                var key = math.Groups[1].Value;
                if (String.IsNullOrEmpty(key)) return null;

                var pix = typeof(IDataTable).GetPropertyEx(key);
                if (pix != null)
                    return (String)table.GetValue(pix);
                else
                    return table.Properties[key];
            });
            #endregion

            var templatePath = Engine.TemplatePath;
            var templateDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, templatePath, cfg.TemplateName);
            if (Directory.Exists(templateDir)) templateDir =
                //Directory.GetCurrentDirectory().EnsureEnd("/") +
                templatePath + "/" + cfg.TemplateName;
            if (Directory.Exists(templateDir)) 
            {
                var ds = Directory.GetFiles(templateDir);
                if (ds != null && ds.Length > 0)
                {
                    foreach (var item in ds)
                    {
                        //var fi = new FileInfo(item);
                        var res = GenTemplate(item, table, tables, cfg);

                        // 计算输出文件名
                        var fileName = Path.GetFileName(item);

                        // 如果文件名以“.cshtml”结尾，并且去掉之后如果还是个完整的文件名，那么去掉“.cshtml”，
                        // 否则将生成cshtml文件
                        if (fileName.EndsWith(".cshtml",StringComparison.CurrentCultureIgnoreCase)
                            && Path.GetFileNameWithoutExtension(fileName).IndexOf('.')>0)
                        {
                            //去掉 “.cshtml”
                            fileName = Path.GetFileNameWithoutExtension(fileName);
                        }

                        var fname = cfg.UseCNFileName ? table.DisplayName : table.Name;
                        fname = fname.Replace("/", "_").Replace("\\", "_").Replace("?", null);
                        // 如果中文名无效，采用英文名
                        if (String.IsNullOrEmpty(Path.GetFileNameWithoutExtension(fname)) || fname[0] == '.') fname = table.Name;
                        fileName = fileName.Replace("类名", fname).Replace("中文名", fname).Replace("连接名", cfg.EntityConnName);

                        fileName = Path.Combine(outpath, fileName);

                        // 如果不覆盖，并且目标文件已存在，则跳过
                        //if (!cfg.Override && File.Exists(fileName)) continue;

                        //var content = tt.Render(item.Name, data);

                        var dir = Path.GetDirectoryName(fileName);
                        if (!String.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
                        //File.WriteAllText(fileName, content, Encoding.UTF8);
                        // 将文件保存为utf-8无bom格式
                        //File.WriteAllText(fileName, content, new UTF8Encoding(false));

                        // aspx页面如果不是UTF8编码，很有可能出现页面中文乱码，CMX生成的页面文件出现该情况
                        // 使用模版文件本身的文件编码来作为输出文件的编码，默认UTF8
                        System.IO.File.WriteAllText(fileName, res, Encoding.Default);
                    }
                }
            }

            return "生成成功!";
        }

        [HttpGet("[action]")]
        public string OpenDir(string dir)
        {
            if (dir.IsNullOrWhiteSpace())
            {
                return "目录不能为空";
            }

            var dirInfo = Directory.CreateDirectory(dir);

            Process.Start("explorer.exe", "\"" + dirInfo.FullName + "\"");

            return "打开成功！";
        }

        public String GenTemplate(string relativePath, IDataTable table, List<IDataTable> tables, ModelConfig cfg)
        {
            //var relativePath = @"/Views/Home/Index1.cshtml";
            if (relativePath == null)
            {
                relativePath = "/Template/实体数据/中文名.cs.cshtml";
            }
            var compiler = _viewCompilerProvider.GetCompiler();// new Microsoft.AspNetCore.Mvc.Razor.Internal.RazorViewCompiler
             var compileTask = compiler.CompileAsync(relativePath);

            var viewDescriptor = compileTask.GetAwaiter().GetResult();

            var viewType = viewDescriptor.Type;

            var newExpression = Expression.New(viewType);
            var pathProperty = viewType.GetTypeInfo().GetProperty(nameof(IRazorPage.Path));

            // Generate: page.Path = relativePath;
            // Use the normalized path specified from the result.
            var propertyBindExpression = Expression.Bind(pathProperty, Expression.Constant(viewDescriptor.RelativePath));
            var objectInitializeExpression = Expression.MemberInit(newExpression, propertyBindExpression);
            var pageFactory = Expression
                .Lambda<Func<IRazorPage>>(objectInitializeExpression)
                .Compile();

            var page = pageFactory.Invoke();

            var bufferScope = HttpContext.RequestServices.GetRequiredService<IViewBufferScope>();
            var buffer = new ViewBuffer(bufferScope, page.Path, ViewBuffer.ViewPageSize);
            var stringWriter = new StringWriter();
            var writer = new ViewBufferTextWriter(buffer, Encoding.UTF8, _htmlEncoder, stringWriter);

            page.ViewContext = new ViewContext
            {
                Writer = writer,
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            };

            if (page is RazorPageBase rPageBase)
            {
                rPageBase.HtmlEncoder = _htmlEncoder;
                rPageBase.ViewBag.Config = cfg;
                rPageBase.ViewBag.Table = table;
                rPageBase.ViewBag.Tables = tables;
                //rPageBase.ViewContext.ViewData.Model = 666;
            }

            var task = page.ExecuteAsync();
            //task.Wait();
            if (task.Exception!=null)
            {
                XTrace.WriteLine(task.Exception.ToString());
            }

            writer.Flush();

            var res = HttpUtility.HtmlDecode(stringWriter.ToString());

            return res;
        }
    }

    public class DataModel
    {

        #region 属性
        ///// <summary>配置</summary>
        public static ModelConfig Config { get { return ModelConfig.Current; } }

        public Engine _Engine;
        /// <summary>生成器</summary>
        Engine Engine
        {
            get { return _Engine ?? (_Engine = new Engine(Config)); }
            set { _Engine = value; }
        }
        #endregion

        //#region 界面初始化
        public DataModel()
        {
            AutoLoadTables(Config.ConnName);
            AutoDetectDatabase();
        }
        
        void AutoDetectDatabase()
        {
            // 加上本机MSSQL
            var localName = "local_MSSQL";
            var localstr = "Data Source=.;Initial Catalog=master;Integrated Security=True;";
            if (!ContainConnStr(localstr)) DAL.AddConnStr(localName, localstr, null, "mssql");

            // 检测本地Access和SQLite
            //Task.Factory.StartNew(DetectFile, TaskCreationOptions.LongRunning).LogException();

            //!!! 必须另外实例化一个列表，否则作为数据源绑定时，会因为是同一个对象而被跳过
            //var list = new List<String>();

            // 探测连接中的其它库
            //Task.Factory.StartNew(DetectRemote, TaskCreationOptions.LongRunning).LogException();
        }
        
        Boolean ContainConnStr(String connstr)
        {
            foreach (var item in DAL.ConnStrs)
            {
                if (connstr.EqualIgnoreCase(item.Value)) return true;
            }
            return false;
        }


        public List<IDataTable> SortTables(List<IDataTable> source)
        {
            if (source == null)
            {
                return new List<IDataTable>();
            }
            var list = source;
            if (list.Count > 0 && list[0].DbType == DatabaseType.SqlServer) // 增加对SqlServer 2000的特殊处理  ahuang
            {
                //list.Remove(list.Find(delegate(IDataTable p) { return p.Name == "dtproperties"; }));
                //list.Remove(list.Find(delegate(IDataTable p) { return p.Name == "sysconstraints"; }));
                //list.Remove(list.Find(delegate(IDataTable p) { return p.Name == "syssegments"; }));
                //list.RemoveAll(delegate(IDataTable p) { return p.Description.Contains("[0E232FF0-B466-"); });
                list.RemoveAll(dt => dt.Name == "dtproperties" || dt.Name == "sysconstraints" || dt.Name == "syssegments" || dt.Description.Contains("[0E232FF0-B466-"));
            }


            // 表名排序
            var tables = source;
            tables.Sort((t1, t2) => t1.Name.CompareTo(t2.Name));
            return tables;
        }

        void AutoLoadTables(String name)
        {
            if (String.IsNullOrEmpty(name)) return;
            if (!DAL.ConnStrs.TryGetValue(name, out var connstr) || connstr.IsNullOrWhiteSpace()) return;

            // 异步加载
            Task.Factory.StartNew(() => { var tables = DAL.Create(name).Tables; }).LogException();
        }

        public List<String> GetTemplateList()
        {
            var list = new List<String>();
            foreach (var item in Engine.FileTemplates)
            {
                list.Add(item);
            }
            return list;
        }
    }
}
