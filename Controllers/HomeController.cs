using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.WebEncoders.Testing;
using NewLife.Reflection;
using XCode.Membership;

namespace Web码神工具.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRazorPageFactoryProvider _pageFactory;
        //private readonly RazorPageFactoryResult _factoryResult;
        private readonly IViewCompilerProvider _viewCompilerProvider;
        private HtmlEncoder _htmlEncoder;
        private IViewCompiler _compiler;

        public HomeController(IRazorPageFactoryProvider pageFactory,
            IViewCompilerProvider viewCompilerProvider,
            HtmlEncoder htmlEncoder
            //,IViewCompiler compiler
            )
        {
            _pageFactory = pageFactory;
            //_factoryResult = factoryResult;
            _viewCompilerProvider = viewCompilerProvider;
            _htmlEncoder = htmlEncoder;
            //_compiler = compiler;
        }
        public IActionResult Index()
        {
            return View();

            //Test.T(_viewCompilerProvider, _pageFactory);
            var relativePath = @"/Views/Home/Index1.cshtml";
            var compiler = _viewCompilerProvider.GetCompiler();
            var compileTask = compiler.CompileAsync(relativePath);

            CompiledViewDescriptor viewDescriptor = compileTask.GetAwaiter().GetResult();

            var viewType = viewDescriptor.ViewAttribute.ViewType;

            var newExpression = Expression.New(viewType);
            var pathProperty = viewType.GetTypeInfo().GetProperty(nameof(IRazorPage.Path));

            // Generate: page.Path = relativePath;
            // Use the normalized path specified from the result.
            var propertyBindExpression = Expression.Bind(pathProperty, Expression.Constant(viewDescriptor.RelativePath));
            var objectInitializeExpression = Expression.MemberInit(newExpression, propertyBindExpression);
            var pageFactory = Expression
                .Lambda<Func<IRazorPage>>(objectInitializeExpression)
                .Compile();
            //var razorPageFactoryResult = new RazorPageFactoryResult(viewDescriptor, pageFactory);
            //var page = razorPageFactoryResult.RazorPageFactory();
            var page = pageFactory.Invoke();
            page.ViewContext = new ViewContext
            {
                Writer = new System.IO.StringWriter(),
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                //{
                //    {  "user" , new UserX(){ Name = "23333"} }
                //}
            };
            if (page is RazorPageBase rPageBase)
            {
                rPageBase.HtmlEncoder = _htmlEncoder;
                //rPageBase.ViewBag = new DynamicViewData();
                rPageBase.ViewBag.user = new UserX() {Name = "23333了就"};
            }

            //if (page is RazorPage<UserX> rPage)
            //{
            //    rPage.ViewData = new ViewDataDictionary<UserX>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            //    {
            //        {"user", new UserX() {Name = "23333"}},
            //    };
            //    rPage.ViewData.Model = new UserX() {Name = "哈哈哈哈"};
            //}
            var p = page.ExecuteAsync().GetAwaiter();
            var res = page.GetValue("Output").ToString();
            return Content(res, "text/html",Encoding.UTF8);
        }

        public IActionResult Error()
        {
            return View();
        }
    }

    public class Test
    {
        public static void T(IViewCompilerProvider viewCompilerProvider, IRazorPageFactoryProvider pageFactory1)
        {
            var relativePath = @"/Views/Home/Index1.cshtml";
            var result = new ViewLocationCacheResult(new []{ relativePath });

            var factoryResult = pageFactory1.CreateFactory(relativePath);
            var httpContext = new DefaultHttpContext { RequestServices = null };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            //result.ViewEntry.PageFactory();
            var page1 = factoryResult.RazorPageFactory();
            page1.ViewContext = new ViewContext();
            var text = page1.ExecuteAsync();
            text.GetAwaiter().GetResult();
            var res1 = page1.BodyContent;
            var compiler = viewCompilerProvider.GetCompiler();
            var compileTask = compiler.CompileAsync(relativePath);

            CompiledViewDescriptor viewDescriptor = compileTask.GetAwaiter().GetResult();

            var viewType = viewDescriptor.ViewAttribute.ViewType;

            var newExpression = Expression.New(viewType);
            var pathProperty = viewType.GetTypeInfo().GetProperty(nameof(IRazorPage.Path));

            // Generate: page.Path = relativePath;
            // Use the normalized path specified from the result.
            var propertyBindExpression = Expression.Bind(pathProperty, Expression.Constant(viewDescriptor.RelativePath));
            var objectInitializeExpression = Expression.MemberInit(newExpression, propertyBindExpression);
            var pageFactory = Expression
                .Lambda<Func<IRazorPage>>(objectInitializeExpression)
                .Compile();
            var razorPageFactoryResult = new RazorPageFactoryResult(viewDescriptor, pageFactory);
            var page = razorPageFactoryResult.RazorPageFactory();
            page.ExecuteAsync().GetAwaiter();
            var res = page.BodyContent;
        }
    }
}
