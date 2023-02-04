using ProiectAnaliza.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ProiectAnaliza.Controllers
{
    public class CoachController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Coach

        public ActionResult Index()
        {
            if(TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            var search = Request.Params["search"];
            var sort = Request.Params["sort"];
            var filtru = Request.Params["filter"];
            if (search == null)
                search = "";
            if (sort == null)
                sort = "";
            if (filtru == null)
                filtru = "";
            List < Coach > coach = db.Coaches.Include("Category").Include("User").Where(c=>c.Name.Contains(search)).ToList();
            CalculateCoachesFinalRating(coach);
            if (sort == "age-asc")
                coach = coach.OrderBy(c => c.Age).ToList();
            if (sort == "age-desc")
                coach = coach.OrderByDescending(c => c.Age).ToList();
            if (sort == "rating-asc")
                coach = coach.OrderBy(c => c.FinalRating).ToList();
            if (sort == "rating-desc")
                coach = coach.OrderByDescending(c => c.FinalRating).ToList();
            System.Diagnostics.Debug.WriteLine(sort);
            System.Diagnostics.Debug.WriteLine(filtru);
            var categories = from category in db.Categories
                             orderby category.CategoryName
                             select category;
            ViewBag.Categories = categories;
            List<Coach> filtrare = new List<Coach>();
            ViewBag.Filtru = filtru;
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            foreach (var filter in coach)
                if (filter.Category.CategoryName == filtru)
                    filtrare.Add(filter);
            if (filtru == "All" || filtru == "" || filtru == null)
                filtrare = coach;
            ViewBag.Coaches = filtrare;
                


            ViewBag.UserId = User.Identity.GetUserId();
            return View();
        }

        private void CalculateCoachesFinalRating(List<Coach> coaches)
        {
            foreach (Coach coach in coaches)
                CalculateCoachFinalRating(coach);
        }

        static public void CalculateCoachFinalRating(Coach coach)
        {
            var NrRev = coach.Reviews.Count;
            foreach (Review rev in coach.Reviews)
                coach.FinalRating += rev.Rating;
            if (NrRev!=0)
                coach.FinalRating = coach.FinalRating / NrRev;
            else
                coach.FinalRating = 0;
        }

        public ActionResult Show (int id)
        {
            Coach coach = db.Coaches.Find(id);
            CalculateCoachFinalRating(coach);
            if (TempData.ContainsKey("message"))
                ViewBag.Message = TempData["message"];
            ViewBag.Coach = coach;
            ViewBag.Category = coach.Category;
            ViewBag.FileName = coach.FileName;

            ViewBag.Reviews = from review in coach.Reviews
                              select review;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult Show(Review rev)
        {
            rev.Date = DateTime.Now;
            rev.UserId = User.Identity.GetUserId();
            try
            {
                if (ModelState.IsValid)
                {
                    db.Reviews.Add(rev);
                    db.SaveChanges();

                    Coach commCoach = db.Coaches.Find(rev.CoachId);
                    string authorEmail = commCoach.User.Email;

                    string notificationBody = "<p>A fost adaugat un nou feedback la profilul dvs cu numele:</p>";
                    notificationBody += "<p><strong>" + commCoach.Name + "<strong></p>";
                    notificationBody += "<br />";
                    notificationBody += "Feedbackul adaugat este: <br /><br /><em>";
                    notificationBody += rev.Content;
                    notificationBody += "</em>";
                    notificationBody += "<br /><br />O zi frumoasa!";
                    System.Diagnostics.Debug.WriteLine(notificationBody);
                    SendEmailNotification(authorEmail, "Un nou feedback a fost adaugat la profilul dvs", notificationBody);

                    return Redirect("/Coach/Show/" + rev.CoachId);
                }

                else
                {
                    Coach a = db.Coaches.Find(rev.CoachId);
                    return View(a);
                }

            }

            catch (Exception e)
            {
                Coach a = db.Coaches.Find(rev.CoachId);
                return View(a);
            }

        }

        [Authorize(Roles = "User")]
        public ActionResult New()
        {
            Coach coach = new Coach();
            var categories = from cat in db.Categories
                             select cat;
            coach.UserId = User.Identity.GetUserId();
            ViewBag.Categories = categories;
            return View(coach);
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        public ActionResult New(Coach coach)
        {
            var categories = from cat in db.Categories
                             select cat;
            ViewBag.Categories = categories;
            coach.UserId = User.Identity.GetUserId();
            string fileExtension = Path.GetExtension(coach.Image.FileName);
            try
            {
                if (ModelState.IsValid && (fileExtension == ".PNG" || fileExtension == ".png" || fileExtension == ".jpeg" || fileExtension == ".JPG" || fileExtension == ".JPEG" || fileExtension == ".jpg"))
                {
                    string uploadFolderPath = Server.MapPath("~//Files//");
                    coach.Image.SaveAs(uploadFolderPath + coach.Image.FileName);
                    coach.FileName = coach.Image.FileName;
                    coach.DisableNewAppointments = false;
                    db.Coaches.Add(coach);
                    db.SaveChanges();
                    TempData["message"] = "Ai trimis o cerere catre admin pentru a adauga un antrenor!";
                    return Redirect("/Coach/Index");
                }
                else
                {
                    return View();
                }
            }
            catch (Exception e)
            {
                return View(coach);
            }
        }
         public ActionResult Edit(int id)
        {
            Coach coach = db.Coaches.Find(id);
            ViewBag.Coach = coach;
            ViewBag.Category = coach.Category;
            var categories = from cat in db.Categories
                             select cat;
            ViewBag.Categories = categories;
            if(coach.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
                return View(coach);
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unei postari care nu va apartine!";
                return RedirectToAction("Index");
                   
            }
        }

        [HttpPut]
        public ActionResult Edit(int id, Coach requestCoach)
        {
            try
            {
                Coach coach = db.Coaches.Find(id);
                if (coach.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
                {
                    if (TryUpdateModel(coach))
                    {
                        coach.Name = requestCoach.Name;
                        coach.Content = requestCoach.Content;
                        coach.Age = requestCoach.Age;
                        coach.CategoryId = requestCoach.CategoryId;
                        coach.DisableNewAppointments = requestCoach.DisableNewAppointments;
                        db.SaveChanges();
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unei postari care nu va apartine!";
                    return RedirectToAction("Index");
                }
            }
            catch(Exception e)
            {
                return View();
            }
        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Coach coach = db.Coaches.Find(id);
            if (coach.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                db.Coaches.Remove(coach);
                db.SaveChanges();
                TempData["message"] = "Postarea a fost stearsa!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unei postari care nu va apartine!";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Approve()
        {
            var coaches = db.Coaches.Include("Category").Include("User");
            ViewBag.Coaches = coaches;
            if (TempData.ContainsKey("message"))
                ViewBag.Message = TempData["message"];
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Approve(int id)
        {
            
            Coach coach = db.Coaches.Find(id);
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            UserManager.AddToRole(coach.UserId, "Editor");
            coach.Approved = true;
            if (TryUpdateModel(coach))
            {
                db.SaveChanges();
                TempData["message"] = "Antrenorul a fost aprobat!";
                return RedirectToAction("Index");
            }
            return View();
        }

        public ActionResult Profile()
        {
            var coaches = db.Coaches.Include("Category").Include("User").ToList();
            CalculateCoachesFinalRating(coaches);
            ViewBag.UserId = User.Identity.GetUserId();
            ViewBag.Coaches = coaches;
            return View();
        }

        private void SendEmailNotification(string toEmail, string subject, string content)
        {
            const string senderEmail = "neverloseyourhope04@gmail.com";
            const string senderPassword = "dhpnphfxxndcfoxs";


            const string smtpServer = "smtp.gmail.com";
            const int smtpPort = 587;

            SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort);
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);

            MailMessage email = new MailMessage(senderEmail, toEmail, subject, content);
            email.IsBodyHtml = true;
            email.BodyEncoding = UTF8Encoding.UTF8;

            try
            {
                System.Diagnostics.Debug.WriteLine("Sending email...");
                smtpClient.Send(email);
                System.Diagnostics.Debug.WriteLine("Email sent!");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error occured while trying to send email");
                System.Diagnostics.Debug.WriteLine(e.Message.ToString());

            }

        }

        public ActionResult Availability(int id)
        {
            Coach coach = db.Coaches.Find(id);
            if (coach == null)
                return View("Error");
            Appointment appointment = new Appointment
            {
                CoachId = id
            };
            ViewBag.TimeBlockHelper = new SelectList(String.Empty);
            ViewBag.CoachName = coach.Name;
            return View(appointment);
        }

        [Authorize(Roles = "Admin, Editor")]
        public ActionResult History(string id,string Search)
        {
                Customer customer = db.Customers.Find(id);
                var model = new EditCustomerViewModel(customer);
                System.Diagnostics.Debug.WriteLine(id);
                Coach coach = db.Coaches.First(u => u.UserId == id);
                if (coach == null)
                    return View("Error");
            if (!String.IsNullOrEmpty(Search))
            {
                coach.Appointments = coach.Appointments.Where(s => s.Customer.Email.ToUpper().Contains(Search.ToUpper())).ToList();
            }
            coach.Appointments.Sort();
                return View(coach);
        }


        public ActionResult UpcomingAppointments(string id, string SearchString)
        {
            Customer customer = db.Customers.Find(id);
            var model = new EditCustomerViewModel(customer);
            System.Diagnostics.Debug.WriteLine(id);
            Coach coach = db.Coaches.First(u => u.UserId == id);
            if (coach == null)
                return View("Error");
            if (!String.IsNullOrEmpty(SearchString))
            {
                coach.Appointments = coach.Appointments.Where(s => s.Customer.Email.ToUpper().Contains(SearchString.ToUpper())).ToList();
            }
            coach.Appointments.Sort();
            return View(coach);
        }

       

    }
}