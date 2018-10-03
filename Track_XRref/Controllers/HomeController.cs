using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Track_XRref.Models;

namespace Track_XRref.Controllers
{
    public class HomeController : Controller
    {
        private SageBillingEntities db = new SageBillingEntities();

        public ActionResult Index()
        {
            ViewBag.EmpMatchCount = db.spEmployeeMatchNullEmpCodeSelect().Count();
            return View();
        }

        //public ActionResult About()
        //{
        //    ViewBag.Message = "Your application description page.";

        //    return View();
        //}

        //public ActionResult Contact()
        //{
        //    ViewBag.Message = "Your contact page.";

        //    return View();
        //}
    }
}