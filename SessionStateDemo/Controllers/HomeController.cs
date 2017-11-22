﻿using SessionStateDemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SessionStateDemo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Session["user" + "_1"] = new Users() {  Name="liufe",Id="1" };
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
    }
}