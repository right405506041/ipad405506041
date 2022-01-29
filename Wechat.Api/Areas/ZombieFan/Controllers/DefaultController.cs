using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Wechat.Api.Areas.ZombieFan.Controllers
{
    public class DefaultController : Controller
    {
        // GET: ZombieFan/Default
        public ActionResult Index()
        {
            return View();
        }

        // GET: ZombieFan/Default/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

    
    }
}
