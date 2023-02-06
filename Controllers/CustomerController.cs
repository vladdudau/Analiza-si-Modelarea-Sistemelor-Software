using ProiectAnaliza.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProiectAnaliza.Controllers
{
    public class CustomerController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [Authorize]
        public ActionResult Details()
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