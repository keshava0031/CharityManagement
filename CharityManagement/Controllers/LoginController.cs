using CharityManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CharityManagement.Controllers
{
    public class LoginController : Controller
    {
        CRMEntities db = new CRMEntities();
        public ActionResult Logout()
        {
                Session.Clear();
                return RedirectToAction("Login", "Login");
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(LoginDetail loginDetails)
        {
            if(ModelState.IsValid)
            {
                LoginDetail user = new LoginDetail();
                user = db.LoginDetails.FirstOrDefault(u => u.Username == loginDetails.Username);
                if (user != null)
                {
                    if (user.Username == loginDetails.Username && user.Password == loginDetails.Password)
                    {
                        Session["UserId"] = user.Username;

                        if (user.LoginAs.Equals("Donar"))
                        {
                            Donar donar = db.Donars.FirstOrDefault(d => d.Username == user.Username);
                            TempData["Dname"] = donar.DName;
                            return RedirectToAction("Index", "Donar");
                        }
                        if (user.LoginAs.Equals("NGO"))
                        {
                            NGO ngo = db.NGOes.FirstOrDefault(n => n.Username == user.Username);
                            TempData["Ngname"] = ngo.NgName;
                            return RedirectToAction("start", "NGO");
                        }
                        if (user.LoginAs.Equals("Admin"))
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                    }
                    else
                        ViewBag.Message = "Invalid Password";
                    return View();
                }
                else
                {
                    ViewBag.Message = "User does not exist";
                    return View();
                }
            }
            return View();
        }
        public ActionResult Index()
        {
            return View();
        }
    }
}