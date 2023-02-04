using ProiectAnaliza.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Net;
using Microsoft.AspNet.Identity;
using System.Net.Mail;
using System.Text;

namespace ProiectAnaliza.Controllers
{
    public class AppointmentController : Controller
    {


        private ApplicationDbContext db = new ApplicationDbContext();

        

        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var appointments = db.Appointments.Include(a => a.Coach).OrderByDescending(a => a.Date).ToList();
            return View(appointments);
        }

        public void SendEmailAppointmentNotification()
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

            var appointments = db.Appointments.Include(a => a.Coach).OrderByDescending(a => a.Date).ToList();
            foreach (var appointment in appointments)
            {
                
                DateTime toCheck = appointment.Date;
                
                if (DateTime.Now.AddDays(1).Day == toCheck.Day && toCheck>=DateTime.Now)
                {
                    Console.WriteLine(toCheck);
                    string notificationBody = "<p>Nu uitati de programarea dvs. realizata in cadrul platformei  <b>NeverLoseYourHope</b> de pe data:</p>";
                    notificationBody += "<p><strong>" + appointment.Date.ToString("dd/MM/yyyy") + " de la ora " + appointment.TimeBlockHelper + "</strong></p>";
                    notificationBody += "<br />";
                    notificationBody += "<br />O zi frumoasa!";
                    System.Diagnostics.Debug.WriteLine(notificationBody);
                    MailMessage email = new MailMessage(senderEmail, appointment.Customer.Email, "Reminder programare", notificationBody);
                    email.IsBodyHtml = true;
                    email.BodyEncoding = UTF8Encoding.UTF8;
                    try
                    {
                        
                        
                        System.Diagnostics.Debug.WriteLine("Sending email...");
                        System.Diagnostics.Debug.WriteLine(toCheck.Day);
                        System.Diagnostics.Debug.WriteLine(DateTime.Now.AddDays(1).Day);
                        smtpClient.Send(email);
                        System.Diagnostics.Debug.WriteLine("Email sent!");
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine("Error occured while trying to send email");
                        System.Diagnostics.Debug.WriteLine(e.Message.ToString());

                    }
                }

                }

        }

        //Get: /Appointments/Details
        [Authorize]
        public ActionResult Details(int ?id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Appointment appointment = db.Appointments.Find(id);
            if(appointment == null)
            {
                return View("Error");
            }
            return View(appointment);
        }

        public ActionResult Detalii()
        {
            string id = User.Identity.GetUserId();
            Console.WriteLine(id);
            var Db = new ApplicationDbContext();
            Customer customer = Db.Customers.Find(id);
            System.Diagnostics.Debug.WriteLine(id);
            var model = new EditCustomerViewModel(customer);
            model.Appointments.Sort();
            return View(model);

        }

        //Get /Appointments/Create
        [Authorize(Roles ="User")]
        public ActionResult Create()
        {
            ViewBag.CoachId = new SelectList(db.Coaches.Where(x => x.DisableNewAppointments == false), "Id", "Name");
            ViewBag.TimeBlockHelper = new SelectList(String.Empty);
            var model = new Appointment
            {
                UserId = User.Identity.GetUserId()
            };

            return View(model);
        }

        // POST: /Appointments/Create
        [HttpPost]
        [Authorize(Roles ="User")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AppointmentId,CoachId,Date,TimeBlockHelper,Time")] Appointment appointment)
        {
            appointment.UserId = User.Identity.GetUserId();

            if(appointment.TimeBlockHelper == "DONT")
                ModelState.AddModelError("", "Nicio programare disponibila pentru " + appointment.Date.ToShortDateString());
            else
            {
                appointment.Time = DateTime.Parse(appointment.TimeBlockHelper);

                DateTime start = appointment.Date.Add(appointment.Time.TimeOfDay);
                Console.WriteLine(start);
                DateTime end = (appointment.Date.Add(appointment.Time.TimeOfDay)).AddMinutes(double.Parse(db.Administrations.Find(1).Value));
                if (!(BusinessLogic.IsInWorkingHours(start, end)))
                    ModelState.AddModelError("", "Antrenorul munceste doar intre " + int.Parse(db.Administrations.Find(8).Value));

                string check = BusinessLogic.ValidateNoAppoinmentClash(appointment);
                if (check != "")
                    ModelState.AddModelError("", check);
            }

            if(ModelState.IsValid)
            {
                db.Appointments.Add(appointment);
                db.SaveChanges();
                return RedirectToAction("/Detalii");
            }

            ViewBag.CoachId = new SelectList(db.Coaches.Where(x => x.DisableNewAppointments == false), "Id", "Name", appointment.CoachId);
            ViewBag.TimeBlockHelper = new SelectList(String.Empty);
            return View(appointment);
        }

        [HttpPost]
        public JsonResult GetAvailableAppointments(int coachId, DateTime date)
        {
            List<SelectListItem> resultlist = BusinessLogic.AvailableAppointments(coachId, date);
            return Json(resultlist);
        }

        [Authorize(Roles ="Admin,User")]
        public ActionResult Edit(int ?id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Appointment appointment = db.Appointments.Find(id);
            if (appointment == null)
                return View("Error");
            ViewBag.TimeBlockHelper = new SelectList(String.Empty);
            ViewBag.Date = appointment.Date.Date;
            ViewBag.CoachId = new SelectList(db.Coaches.Where(x => x.DisableNewAppointments == false), "Id", "Name", appointment.CoachId);
            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit([Bind(Include = "AppointmentID,CoachId,Date,TimeBlockHelper,Available")] Appointment appointment)
        {
            if(appointment.TimeBlockHelper == "DONT")
               ModelState.AddModelError("", "Nu avem nicio programare disponibila pentru " + appointment.Date.ToShortDateString());
            else
            {
                appointment.Time = DateTime.Parse(appointment.TimeBlockHelper);

                DateTime start = new DateTime(appointment.Date.Year, appointment.Date.Month, appointment.Date.Day, appointment.Time.Hour, appointment.Time.Minute, appointment.Time.Second);
                DateTime end = new DateTime(appointment.Date.Year, appointment.Date.Month, appointment.Date.Day, appointment.Time.Hour, appointment.Time.Minute, appointment.Time.Second).AddMinutes(double.Parse(db.Administrations.Find(7).Value));
                if (!BusinessLogic.IsInWorkingHours(start, end))
                    ModelState.AddModelError("", "Orele de lucru ale fiecarui antrenor sunt " + int.Parse(db.Administrations.Find(8).Value) + " la " + int.Parse(db.Administrations.Find(9).Value));
            }

            if(ModelState.IsValid)
            {
                db.Entry(appointment).State = EntityState.Modified;
                db.Entry(appointment).Property(u => u.UserId).IsModified = false;
                db.SaveChanges();
                if (User.IsInRole("Admin"))
                    return RedirectToAction("Index");
                return RedirectToAction("Detalii");
            }

            ViewBag.TimeBlockHelper = new SelectList(String.Empty);
            ViewBag.CoachId = new SelectList(db.Coaches.Where(x => x.DisableNewAppointments == false), "Id", "Name", appointment.CoachId);
            return View(appointment);
        }
        
        [Authorize]
        public ActionResult Delete(int ?id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Appointment appointment = db.Appointments.Find(id);
            if (appointment == null)
                return View("Error");
            return View(appointment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,User,Editor")]
        public ActionResult DeleteConfirmed(int id)
        {
            Appointment appointment = db.Appointments.Find(id);
            db.Appointments.Remove(appointment);
            db.SaveChanges();
            if (User.IsInRole("Admin"))
                return RedirectToAction("Index");
            else if (User.IsInRole("Editor"))
                return RedirectToAction("UpcomingAppointments", new { Controller = "Coach", Action = "UpcomingAppointments", id = User.Identity.GetUserId() });
            else if (User.IsInRole("User"))
                return RedirectToAction("Detalii", new { Controller = "Appointment", Action = "Detalii" });
            else
                return RedirectToAction("/Coach/Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }



    }
}