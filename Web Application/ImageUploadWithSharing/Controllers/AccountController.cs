using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ImageSharingWithUpload.Controllers
{
    public class AccountController : Controller    
    {
        protected void CheckAda()
        {
            var cookie = Request.Cookies["ADA"];
            if (cookie != null && "true".Equals(cookie))
            {
                ViewBag.isADA = true;
            }
            else
            {
                ViewBag.isADA = false;
            }
        }

        // TODO
        [HttpGet]
        public ActionResult Register()
        {
            CheckAda();
            return View();
        }

        // TODO
        [HttpPost]
        public ActionResult Register(String Username, String ADA)
        {
            CheckAda();
            var options = new CookieOptions() { IsEssential = true, Secure = true, SameSite = SameSiteMode.None, Expires = DateTime.Now.AddMonths(3)  };

            // TODO add cookies for "Username".
            Response.Cookies.Append("Username", Username, options);
            
            // End TODO
            
            Response.Cookies.Append("ADA", ("on".Equals(ADA) ? "true" : "false"), options);


            ViewBag.Username = Username;

            return View("RegisterSuccess");
        }

    }
}
