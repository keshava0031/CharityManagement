using CharityManagement.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using Model;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace CharityManagement.Controllers
{
    public class AdminController : Controller
    {
        CRMEntities db = new CRMEntities();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NGODetail()
        {
            return View(db.NGOes.ToList());
        }
        public ActionResult Nil()
        {
            return View();
        }
        public ActionResult Approvals()
        {
            if (Session["UserId"] != null)
            {
                List<Verification> verify = db.Verifications.Where(v => v.status == "pending").ToList();
                int count = verify.Count();
                if (count > 0)
                {
                    return View(verify);
                }
                else
                {
                    return RedirectToAction("Nil");
                }
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }
        public ActionResult Approve(string id)
        {
            Verification verification = db.Verifications.Find(id);
            Session["image"] = verification.Image;
            List<SelectListItem> status = new List<SelectListItem>();
            status.Add(new SelectListItem { Text = "Verify Account", Value = "0", Selected = true });
            status.Add(new SelectListItem { Text = "Verified", Value = "Verified" });
            status.Add(new SelectListItem { Text = "Reject", Value = "Rejected" });
            ViewData["Status"] = status;
            return View(verification);
        }
        [HttpPost]
        public ActionResult Approve([Bind(Include = "Id,Image,status")] Verification verification)
        {
            if (ModelState.IsValid)
            {
                db.Entry(verification).State = EntityState.Modified;
                db.Entry(verification).Property("Image").IsModified = false;

                db.SaveChanges();
                mail(verification);
                return RedirectToAction("Approvals");
            }
            return View();
        }
        public void mail(Verification verification)
        {
            NGO ngo = db.NGOes.Find(verification.Id);
            string subject = "Account Verification";
            string vbody = "Hello, "+ngo.NgName+" your request for account verification to raise funds is accepted. You can now raise donations from donars from the website.";
            string rbody = "Hello, "+ngo.NgName+" your request for account verification to raise funds is rejected as documents uploaded by you were not clear. So,please resubmit your application with correct and valid documents. Thank You.";
            if(verification.status.Equals("Verified"))
            {
                SendEmail(ngo.Username, subject, vbody);
            }
            if(verification.status.Equals("Rejected"))
            {
                SendEmail(ngo.Username, subject, rbody);
            }
        }
        public void SendEmail()
        {
        }
        [HttpPost]
        public void SendEmail(string receiver, string subject, string message)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    UserActions user = new UserActions();
                    user.Mail(receiver, subject, message);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
        }
        public ActionResult Donations()
        {
            if (Session["UserId"] != null)
            {
                int count = db.Donations.Count();
                if (count > 0)
                    return View(db.Donations.ToList());
                else
                    return RedirectToAction("empty");
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }
        public ActionResult empty()
        {
            return View();
        }
    }
}