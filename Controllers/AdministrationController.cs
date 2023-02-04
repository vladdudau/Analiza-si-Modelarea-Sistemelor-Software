using ProiectAnaliza.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ProiectAnaliza.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdministrationController : Controller
    {
            private ApplicationDbContext db = new ApplicationDbContext();

            // GET: /Administration/
            public ActionResult Index()
            {
                return View(db.Administrations.ToList());
            }

            // GET: /Administration/Edit/5
            public ActionResult Edit(int? id)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Administration Administration = db.Administrations.Find(id);
                if (Administration == null)
                {
                    return View("Error");
                }
                return View(Administration);
            }

            
            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Edit([Bind(Include = "ID,Name,Value")] Administration Administration)
            {
                if (ModelState.IsValid)
                {
                    db.Entry(Administration).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View(Administration);
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