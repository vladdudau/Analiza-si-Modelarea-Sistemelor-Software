using ProiectAnaliza.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProiectAnaliza.Controllers
{
    public class ReviewController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Review
        public ActionResult Index()
        {
            return View();
        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Review rev = db.Reviews.Find(id);
            db.Reviews.Remove(rev);
            db.SaveChanges();
            return Redirect("/Coach/Show/" + rev.CoachId);

        }

        public ActionResult Edit(int id)
        {
            Review rev = db.Reviews.Find(id);
            ViewBag.Review = rev;
            return View();
        }

        [HttpPut]
        public ActionResult Edit(int id, Review requestReview)
        {
            try
            {
                Review rev = db.Reviews.Find(id);
                if(TryUpdateModel(rev))
                {
                    rev.Content = requestReview.Content;
                    rev.Rating = requestReview.Rating;
                    db.SaveChanges();
                }
                return Redirect("/Coach/Show/" + rev.CoachId);
            }
            catch (Exception e)
            {
                return View();
            }
        }
    }
}