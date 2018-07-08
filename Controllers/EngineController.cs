using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Web码神工具.Controllers
{
    public class EngineController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

    }
}
