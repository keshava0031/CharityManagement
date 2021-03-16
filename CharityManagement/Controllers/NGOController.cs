using CharityManagement.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CharityManagement.Controllers
{
    public class NGOController : Controller
    {
        CRMEntities db = new CRMEntities();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult success()
        {
            return View();
        }
        public ActionResult nil()
        {
            return View();
        }
        public ActionResult Donations()
        {
            if (Session["UserId"] != null)
            {
                string username = (string)Session["UserId"];
                NGO ngo = db.NGOes.FirstOrDefault(n => n.Username == username);
                List<Donation> donations = db.Donations.Where(d => d.NgId == ngo.NId).ToList();
                if (donations.Count() > 0)
                {
                    return View(donations);
                }
                else
                {
                    return RedirectToAction("nil");
                }
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }
        public ActionResult Donation()
        {

            if (Session["UserId"] != null)
            {
                string username = (string)Session["UserId"];
                NGO ngo = db.NGOes.FirstOrDefault(n => n.Username == username);
                Raisedonation raise = new Raisedonation();
                raise.NId = ngo.NId;
                return View(raise);
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }
        [HttpPost]
        public ActionResult Donation(Raisedonation raisedonation)
        {
            if(ModelState.IsValid)
            {
                string username = (string)Session["UserId"];
                NGO ngo = db.NGOes.FirstOrDefault(n => n.Username == username);
                Raisedonation donation = db.Raisedonations.FirstOrDefault(d => d.NId == ngo.NId);
                if (donation == null)
                {
                    db.Raisedonations.Add(raisedonation);
                    db.SaveChanges();
                    return RedirectToAction("success");
                }
                else
                {
                    return RedirectToAction("already");
                }
            }
            return View();
        }
        public ActionResult already()
        {
            return View();
        }
        public ActionResult start()
        {
            return View();
        }
        public ActionResult Details()
        {
            if (Session["UserId"] != null)
            {
                string username = (string)Session["UserId"];
                NGO ngo = db.NGOes.FirstOrDefault(n => n.Username == username);
                return View(ngo);
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }
        public ActionResult Pending()
        {
            return View();
        }
        public ActionResult verified()
        {
            if (Session["UserId"] != null)
            {
                string username = (string)Session["UserId"];
                NGO ngo = db.NGOes.FirstOrDefault(n => n.Username == username);
                Verification verification = db.Verifications.Find(ngo.NId);
                if (verification != null)
                {
                    if (verification.status.Equals("pending"))
                    {
                        return RedirectToAction("Pending");
                    }
                    if (verification.status.Equals("Rejected"))
                    {
                        return RedirectToAction("Reject");
                    }
                    if (verification.status.Equals("Verified"))
                    {
                        return View();
                    }
                }
                else
                {
                    return RedirectToAction("verification", "NGO");
                }
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
            return View();
        }
        public ActionResult Reject()
        {
            return View();
        }
        public ActionResult RaiseDonation()
        {
            string username = (string)Session["UserId"];
            NGO ngo = db.NGOes.FirstOrDefault(n => n.Username == username);
            Verification verification = db.Verifications.Find(ngo.NId);
            if(verification!=null)
            {
                if (verification.status.Equals("pending"))
                {
                    return RedirectToAction("Pending");
                }
                if(verification.status.Equals("Rejected"))
                {
                    return RedirectToAction("Reject");
                }
                if(verification.status.Equals("Verified"))
                {
                    return RedirectToAction("Donation");
                }
            }
            else
            {
                return RedirectToAction("verification", "NGO");
            }
            return View();
        }
        public ActionResult images()
        {
            return View(db.Verifications.ToList());
        }
        public ActionResult Edit()
        {
            if (Session["UserId"] != null)
            {
                string userid = (string)Session["UserId"];
                NGO ngo = db.NGOes.FirstOrDefault(n => n.Username == userid);
                return View(ngo);
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }
        [HttpPost]
        public ActionResult Edit([Bind(Include = "NId,NgName,Phone,Address,Username,Password,ConfirmPassword")] NGO ngo)
        {
            if (ModelState.IsValid)
            {
                LoginDetail login = db.LoginDetails.Find(ngo.Username);
                if (login.Password == ngo.Password)
                {
                    db.Entry(ngo).State = EntityState.Modified;
                    db.Entry(ngo).Property("Password").IsModified = false;
                    db.SaveChanges();
                    TempData["Ngname"] = ngo.NgName;
                    return View();
                }
                else
                {
                    ViewBag.Message = "Invalid Password";
                }
            }
            return View(ngo);
        }

        public ActionResult verify()
        {
            string username = (string)Session["UserId"];
            NGO ngo = db.NGOes.FirstOrDefault(n => n.Username == username);
            Verification verification = db.Verifications.Find(ngo.NId);
            if(verification==null)
            {
                return RedirectToAction("verification");
            }
            else
            {
                db.Verifications.Remove(verification);
                db.SaveChanges();
                return RedirectToAction("verification");
            }
        }
        public ActionResult verification()
        {
            return View();
        }
        [HttpPost]
        public ActionResult verification(HttpPostedFileBase file,Verification verification)
        {
                string username = Session["UserId"].ToString();
                NGO user = db.NGOes.FirstOrDefault(u => u.Username == username);
                verification.Id = user.NId;
                string filename = Path.GetFileName(file.FileName);
                string _filename = DateTime.Now.ToString("yymmssfff") + filename;
                string extension = Path.GetExtension(file.FileName);
                string path = Path.Combine(Server.MapPath("~/images/"), _filename);
                verification.Image = "~/images/" + _filename;
                if (extension.ToLower() == ".jpg" || extension.ToLower() == ".jpeg" || extension.ToLower() == ".png")
                {
                    if (file.ContentLength <= 2 * 1024 * 1024)
                    {
                        verification.status = "pending";
                        db.Verifications.Add(verification);
                        if (db.SaveChanges() > 0)
                        {
                            file.SaveAs(path);
                            ViewBag.msg = "Succesfull";
                            ModelState.Clear();
                            return RedirectToAction("Pending");
                        }
                    }
                    else
                    {
                        ViewBag.msg = "Max size allowed is only 2MB";
                        return View();
                    }
                }
                else
                {
                    ViewBag.msg = "Only .jpg,.png and .jpeg files are accepted";
                    return View();
                }
            return View();
        }
        public ActionResult Register()
        {
            NGO ng = new NGO();
            NGO lastNgo = db.NGOes.OrderByDescending(n => n.NId).FirstOrDefault();
            if (lastNgo == null)
            {
                ng.NId = "NG1";
            }
            else
            {
                ng.NId = "NG" + (Convert.ToInt32(lastNgo.NId.Substring(2, lastNgo.NId.Length - 2)) + 1).ToString();
            }
            return View(ng);
        }
        [HttpPost]
        public ActionResult Register(NGO ngo)
        {
            if (ModelState.IsValid)
            {
                LoginDetail login = new LoginDetail();
                login.Username = ngo.Username;
                login.Password = ngo.Password;
                login.LoginAs = "NGO";
                db.LoginDetails.Add(login);
                db.NGOes.Add(ngo);
                db.SaveChanges();
                return RedirectToAction("Login","Login");
            }
            return View();
        }
        public JsonResult IsUserNameAvailable(string UserName)
        {
            return Json(!db.LoginDetails.Any(u => u.Username == UserName), JsonRequestBehavior.AllowGet);
        }
    }
}