using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Vanrise_Internship.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult ClientReport()
        {
            return View();
        }

        public ActionResult DevicesReport()
        {
            return View();
        }

        public ActionResult login()
        {
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }



        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult PhoneReservation()
        {
            ViewBag.Message = "Your  page.";

            return View();
        }
       
    }
}