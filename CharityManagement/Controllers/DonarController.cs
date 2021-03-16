using CharityManagement.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Model;
using System.Net.Mail;
using System.Net;

namespace CharityManagement.Controllers
{
    public class DonarController : Controller
    {
        CRMEntities db = new CRMEntities();
        public ActionResult Donate()
        {
            if (Session["UserId"] != null)
            {
                Donation donation = new Donation();
                Donation lastDonation = db.Donations.OrderByDescending(n => n.TransactionID).FirstOrDefault();
                string username = (string)Session["UserId"];
                Donar donar = db.Donars.FirstOrDefault(d => d.Username == username);
                donation.DId = donar.DId;
                if (lastDonation == null)
                {
                    donation.TransactionID = "TR1";
                }
                else
                {
                    donation.TransactionID = "TR" + (Convert.ToInt32(lastDonation.TransactionID.Substring(2, lastDonation.TransactionID.Length - 2)) + 1).ToString();
                }
                List<SelectListItem> ngo = new List<SelectListItem>();
                List<NGO> ng = db.NGOes.ToList();
                List<NGO> n1 = new List<NGO>();
                foreach (NGO ngo1 in ng)
                {
                    Verification verify = db.Verifications.Find(ngo1.NId);
                    if (verify != null)
                    {
                        if (verify.status.Equals("Verified"))
                        {
                            n1.Add(ngo1);
                        }
                    }
                }
                ViewBag.NGO = new SelectList(n1,"NId","NId");
                return View(donation);
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }
        [HttpPost]
        public ActionResult Donate(Donation donation)
        {
            if (Session["UserId"] != null)
            {
                if (ModelState.IsValid)
                {
                    donation.TransactionDate = System.DateTime.Now;
                    db.Donations.Add(donation);
                    string username = Session["UserId"].ToString();
                    mail(username, donation.TransactionID);
                    db.SaveChanges();
                    return RedirectToAction("success");
                }
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }

        public void mail(string Username,string trid)
        {
            Donation dn = db.Donations.Find(trid);
            Donar donar = db.Donars.FirstOrDefault(d=>d.DId==dn.DId);
            NGO ngo = db.NGOes.FirstOrDefault(n => n.NId == dn.NgId);
            string subject = "Donation Receipt";
            string body = "Hello "+donar.DName.ToUpperInvariant()+",\nThank you for making donation for NGO.Here is the receipt of Donation you have made for your future references.\n\nAmount          : Rs"+dn.Amount+"\n\n"+
                                                                                                                                                                                        "Donar ID        : "+donar.DId+"\n\n"+
                                                                                                                                                                                        "NGO ID          : "+ngo.NId+"\n\n"+
                                                                                                                                                                                        "NGO Name        : "+ngo.NgName+"\n\n"+
                                                                                                                                                                                        "Transaction Id  : "+trid+"\n\n"+
                                                                                                                                                                                        "Transaction Date: "+dn.TransactionDate+"\n\n\n\n"+
                                                                                                                                                                                        "Thank You";
            SendEmail(Username, subject, body);
        }

        public void SendEmail()
        {
            //return "";
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


        public ActionResult success()
        {
            return View();
        }
        public ActionResult NGODetail()
        {
            string username = (string)Session["UserId"];
            List<Verification> verified = db.Verifications.Where(n => n.status == "Verified").ToList();
            NGO ngo = new NGO();
            List<NGO> ngos=new List<NGO>();
            foreach(Verification verify in verified)
            {
                ngo = db.NGOes.FirstOrDefault(n=>n.NId==verify.Id);
                ngos.Add(ngo);
            }
            return View(ngos);
        }
        public string start()
        {
            return "HEllO Donar";
        }
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Details()
        {
            if (Session["UserId"] != null)
            {
                string username = (string)Session["UserId"];
                Donar donar = db.Donars.FirstOrDefault(n => n.Username == username);
                return View(donar);
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }
        public ActionResult Edit()
        {
            if (Session["UserId"] != null)
            {
                string username = (string)Session["UserId"];
                Donar donar = db.Donars.FirstOrDefault(n => n.Username == username);
                return View(donar);
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }
        [HttpPost]
        public ActionResult Edit([Bind(Include = "DId,DName,Phone,Address,Username,Password,ConfirmPassword")] Donar donar)
        {
            if (ModelState.IsValid)
            {
                LoginDetail login = db.LoginDetails.Find(donar.Username);
                if (login.Password == donar.Password)
                {
                    db.Entry(donar).State = EntityState.Modified;
                    db.Entry(donar).Property("Password").IsModified = false;
                    db.SaveChanges();
                    TempData["Dname"] = donar.DName;
                    return View();
                }
                else
                {
                    ViewBag.Message = "Invalid Password";
                }
            }
            return View(donar);
        }
        public ActionResult Register()
        {
            Donar dr = new Donar();
            Donar lastDonar = db.Donars.OrderByDescending(n => n.DId).FirstOrDefault();
            if (lastDonar == null)
            {
                dr.DId = "DR1";
            }
            else
            {
                dr.DId = "DR" + (Convert.ToInt32(lastDonar.DId.Substring(2, lastDonar.DId.Length - 2)) + 1).ToString();
            }
            return View(dr);
        }
        [HttpPost]
        public ActionResult Register(Donar donar)
        {
            if (ModelState.IsValid)
            {
                LoginDetail login = new LoginDetail();
                login.Username = donar.Username;
                login.Password = donar.Password;
                login.LoginAs = "Donar";
                db.LoginDetails.Add(login);
                db.Donars.Add(donar);
                db.SaveChanges();
                return RedirectToAction("Login", "Login");
            }
            return View();
        }
        public JsonResult IsUserNameAvailable(string UserName)
        {
            return Json(!db.LoginDetails.Any(d => d.Username == UserName), JsonRequestBehavior.AllowGet);
        }
    }
}